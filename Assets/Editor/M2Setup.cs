using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Wires the Milestone 2 gameplay systems into the currently open scene so they don't
// have to be added by hand. Run once on GameScene.
public static class M2Setup
{
    // Tools > M2 > Setup Survival (Health + HUD + test Zombie)
    [MenuItem("Tools/M2/Setup Survival (Health + HUD + test Zombie)")]
    public static void SetupSurvival()
    {
        var player = Object.FindAnyObjectByType<PlayerController>();
        if (player == null)
        {
            Debug.LogError("[M2] No PlayerController in the scene. Open GameScene first.");
            return;
        }

        // give the player hit points
        var health = player.GetComponent<Health>();
        if (health == null)
        {
            health = Undo.AddComponent<Health>(player.gameObject);
            Debug.Log("[M2] Added Health to the player.");
        }

        // make sure the self-building HUD is present
        if (Object.FindAnyObjectByType<PlayerHUD>() == null)
        {
            var hud = new GameObject("PlayerHUD");
            hud.AddComponent<PlayerHUD>();
            Undo.RegisterCreatedObjectUndo(hud, "Create PlayerHUD");
            Debug.Log("[M2] Created PlayerHUD (health bar + objective + death screen build at play time).");
        }

        // a zombie to test the damage loop against
        if (Object.FindAnyObjectByType<Zombie>() == null)
            EntitySpawner.CreateZombie();

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log("[M2] Survival setup done. Press Play: the zombie will chase and bite; watch the health bar. " +
                  "Save the scene (Ctrl+S) to keep the Health component and PlayerHUD.");
    }

    // Tools > M2 > Setup Friend Dialogue (NPC talk + DialogueUI)
    [MenuItem("Tools/M2/Setup Friend Dialogue (NPC talk + DialogueUI)")]
    public static void SetupFriendDialogue()
    {
        var npc = Object.FindAnyObjectByType<Npc>();
        if (npc == null)
        {
            Debug.LogError("[M2] No Npc in the scene. Create one with Tools > Create NPC first.");
            return;
        }

        // make the friend interactable ([E] to talk)
        if (npc.GetComponent<FriendNpc>() == null)
        {
            Undo.AddComponent<FriendNpc>(npc.gameObject);
            Debug.Log("[M2] Added FriendNpc (dialogue) to '" + npc.name + "'.");
        }

        // the interactor only raycasts against the Interactable layer
        int layer = LayerMask.NameToLayer(GameManager.INTERACTABLE_LAYER_NAME);
        if (layer < 0)
            Debug.LogError("[M2] No 'Interactable' layer exists — add it under Project Settings > Tags and Layers, then re-run.");
        else if (npc.gameObject.layer != layer)
        {
            npc.gameObject.layer = layer;
            Debug.Log("[M2] Set the NPC to the Interactable layer so the player can target it.");
        }

        // the self-building dialogue window
        if (Object.FindAnyObjectByType<DialogueUI>() == null)
        {
            var d = new GameObject("DialogueUI");
            d.AddComponent<DialogueUI>();
            Undo.RegisterCreatedObjectUndo(d, "Create DialogueUI");
            Debug.Log("[M2] Created DialogueUI.");
        }

        // GameState stores the narrative flags/counters that dialogue writes; without it,
        // choices wouldn't survive into the ending. Attach it to the GameManager object.
        if (Object.FindAnyObjectByType<GameState>() == null)
        {
            var gm = Object.FindAnyObjectByType<GameManager>();
            var host = gm != null ? gm.gameObject : new GameObject("GameState");
            Undo.AddComponent<GameState>(host);
            Debug.Log("[M2] Added GameState (narrative flags/counters) to '" + host.name + "'.");
        }

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log("[M2] Friend dialogue setup done. Press Play, walk up to the NPC and press [E] to talk. " +
                  "Save the scene (Ctrl+S) to keep it.");
    }

    // Tools > M2 > Match NPC Height To Player (1.8m)
    // Scales the NPC model so it stands the same height as the player (Minecraft 1.8m),
    // so their eye levels line up, and matches its capsule to the player's proportions.
    [MenuItem("Tools/M2/Match NPC Height To Player (1.8m)")]
    public static void MatchNpcHeight()
    {
        var npc = Object.FindAnyObjectByType<Npc>();
        if (npc == null)
        {
            Debug.LogError("[M2] No Npc in the scene.");
            return;
        }

        if (!tryMeasure(npc, out Bounds bounds))
        {
            Debug.LogError("[M2] NPC has no Renderer to measure. Aborting.");
            return;
        }

        // 1) scale the whole NPC so the visible model is exactly PLAYER_HEIGHT tall
        float currentHeight = bounds.size.y;
        if (currentHeight > 0.001f)
        {
            Undo.RecordObject(npc.transform, "Match NPC height");
            npc.transform.localScale *= GameManager.PLAYER_HEIGHT / currentHeight;
            Debug.Log($"[M2] Scaled NPC from {currentHeight:0.00}m to {GameManager.PLAYER_HEIGHT}m tall.");
        }

        // 2) drop a leftover CapsuleCollider — the CharacterController is the only collider we want
        var extra = npc.GetComponent<CapsuleCollider>();
        if (extra != null)
        {
            Undo.DestroyObjectImmediate(extra);
            Debug.Log("[M2] Removed a redundant CapsuleCollider from the NPC.");
        }

        // 3) put the model's feet on the floor (works regardless of where the pivot is)
        snapToFloor(npc);

        // 4) rebuild the CharacterController from the model's real bounds, so the capsule
        //    wraps the model no matter whether the pivot is at the feet or the centre
        var cc = npc.GetComponent<CharacterController>();
        if (cc != null && tryMeasure(npc, out Bounds finalBounds))
        {
            Transform t = npc.transform;
            float sy  = Mathf.Approximately(t.lossyScale.y, 0f) ? 1f : t.lossyScale.y;
            float sxz = Mathf.Max(Mathf.Abs(t.lossyScale.x), Mathf.Abs(t.lossyScale.z));
            if (sxz <= 0f) sxz = 1f;

            float worldCenterY = finalBounds.min.y + finalBounds.size.y * 0.5f;
            Undo.RecordObject(cc, "Match NPC capsule");
            cc.height = finalBounds.size.y / sy;
            cc.radius = GameManager.PLAYER_RADIUS / sxz;
            cc.center = new Vector3(0f, (worldCenterY - t.position.y) / sy, 0f);
        }

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log("[M2] NPC height matched and placed on the floor. Save the scene (Ctrl+S) to keep it.");
    }

    static bool tryMeasure(Npc npc, out Bounds bounds)
    {
        var renderers = npc.GetComponentsInChildren<Renderer>();
        bounds = default;
        if (renderers.Length == 0) return false;

        bounds = renderers[0].bounds;
        foreach (var r in renderers) bounds.Encapsulate(r.bounds);
        return true;
    }

    // Lifts/drops the NPC so the bottom of its model rests exactly on the floor below it.
    // All of the NPC's own colliders are disabled during the raycast so it can't hit itself.
    static void snapToFloor(Npc npc)
    {
        if (!tryMeasure(npc, out Bounds bounds)) return;

        // CharacterController is itself a Collider, so this also covers it — disable every
        // collider so the downward ray can't hit the NPC's own body, then restore them.
        var colliders = npc.GetComponentsInChildren<Collider>();
        var wasEnabled = new bool[colliders.Length];
        for (int i = 0; i < colliders.Length; i++) { wasEnabled[i] = colliders[i].enabled; colliders[i].enabled = false; }

        // start just above the head (avoids hitting a roof) and cast down to the floor
        Vector3 origin = new Vector3(bounds.center.x, bounds.max.y + 0.3f, bounds.center.z);
        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 200f))
        {
            float lift = hit.point.y - bounds.min.y;   // move feet onto the floor
            Undo.RecordObject(npc.transform, "Snap NPC to floor");
            npc.transform.position += Vector3.up * lift;
        }
        else
        {
            Debug.LogWarning("[M2] Couldn't find a floor under the NPC to snap to — position it manually.");
        }

        for (int i = 0; i < colliders.Length; i++) colliders[i].enabled = wasEnabled[i];
    }

    // Tools > M2 > Fix Lighting — the scene had two stacked directional lights blowing
    // everything out to white. Keep one at a sane intensity and set a survival-mood ambient.
    [MenuItem("Tools/M2/Fix Lighting (dim to survival mood)")]
    public static void FixLighting()
    {
        var lights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
        int keptCount = 0, disabled = 0;
        foreach (var l in lights)
        {
            if (l.type != LightType.Directional) continue;
            Undo.RecordObject(l, "Fix lighting");
            if (keptCount == 0)
            {
                l.enabled = true;
                l.intensity = 0.85f;       // keeps clear directional shading (map albedo is dark enough not to blow out)
                l.shadows = LightShadows.Soft;   // the light we keep might have had shadows off
                l.shadowStrength = 0.8f;
                Undo.RecordObject(l.transform, "Fix lighting");
                l.transform.rotation = Quaternion.Euler(50f, -30f, 0f);   // angled sun so shadows are visible
                keptCount++;
            }
            else
            {
                l.enabled = false;
                disabled++;
            }
        }

        // moderate flat ambient: fills shadows so they aren't pitch black, but keeps some contrast
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.26f, 0.28f, 0.32f);
        RenderSettings.ambientIntensity = 1f;

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log($"[M2] Lighting balanced: 1 directional @0.85 (shadows on) + moderate ambient, disabled {disabled} extra. " +
                  "Nudge the Directional Light Intensity (lit side) and Environment Ambient (dark side) to taste. Ctrl+S.");
    }

    // ---------------------------------------------------------------- environmental storytelling

    // Tools > M2 > Create Readable Note — a findable text fragment that reveals story + objective.
    [MenuItem("Tools/M2/Create Readable Note")]
    public static void CreateNote()
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = "Note";
        go.transform.localScale = new Vector3(0.22f, 0.3f, 0.02f);
        colorize(go, new Color(0.92f, 0.89f, 0.75f));   // paper
        setupReadable(go, "Torn Page", "Read",
            "Day 2. The pharmacy out front is picked clean, but I stashed painkillers behind the counter. " +
            "If Mia's fever is climbing, that's your first stop. Move quiet — noise brings them.",
            "read_note_pharmacy", "Find 3 supplies for Mia");
        placeInFront(go, 2f, 1.2f);
        finish(go, "Create Note");
    }

    // Tools > M2 > Create Trace Marker — an examinable "traces & remains" clue on the floor.
    [MenuItem("Tools/M2/Create Trace Marker")]
    public static void CreateTrace()
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = "BloodTrace";
        go.transform.localScale = new Vector3(0.7f, 0.02f, 0.7f);
        colorize(go, new Color(0.35f, 0.05f, 0.05f));   // dried blood
        setupReadable(go, "Dried Blood", "Examine",
            "Dark streaks smear across the tile, dragging toward the storeroom. Something heavy was pulled " +
            "through here — recently.",
            "saw_blood_trail", "");
        placeInFront(go, 2.5f, 0.02f);
        finish(go, "Create Trace");
    }

    // ---------------------------------------------------------------- ending path (M2 #4)

    // Tools > M2 > Build Ending Scene — creates Ending.unity (self-building EndingController)
    // and registers it in Build Settings, then reopens whatever scene you were editing.
    [MenuItem("Tools/M2/Build Ending Scene")]
    public static void BuildEndingScene()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        string previous = SceneManager.GetActiveScene().path;

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        new GameObject("EndingController").AddComponent<EndingController>();
        const string path = "Assets/Scenes/Ending.unity";
        EditorSceneManager.SaveScene(scene, path);

        var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        if (!scenes.Exists(s => s.path == path))
            scenes.Add(new EditorBuildSettingsScene(path, true));
        EditorBuildSettings.scenes = scenes.ToArray();

        if (!string.IsNullOrEmpty(previous))
            EditorSceneManager.OpenScene(previous, OpenSceneMode.Single);

        Debug.Log("[M2] Built Assets/Scenes/Ending.unity and added it to Build Settings.");
    }

    // Tools > M2 > Create Supply Pickup — a scavengeable that counts toward the rescue goal.
    [MenuItem("Tools/M2/Create Supply Pickup")]
    public static void CreateSupply()
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = "Supply";
        go.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        colorize(go, new Color(0.3f, 0.7f, 0.4f));      // medkit green
        go.AddComponent<SupplyPickup>();
        setInteractableLayer(go);
        placeInFront(go, 2f, 0.5f);
        finish(go, "Create Supply");
    }

    // Tools > M2 > Create Escape Zone — the rescue point that ends the run.
    [MenuItem("Tools/M2/Create Escape Zone")]
    public static void CreateEscapeZone()
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = "EscapeZone";
        go.transform.localScale = new Vector3(1.6f, 0.1f, 1.6f);
        colorize(go, new Color(0.85f, 0.75f, 0.2f));    // signal pad
        go.AddComponent<EscapeZone>();
        setInteractableLayer(go);
        placeInFront(go, 3f, 0.05f);
        finish(go, "Create Escape Zone");
    }

    // Tools > M2 > Setup Objective Tracker — drives the objective line on the HUD.
    [MenuItem("Tools/M2/Setup Objective Tracker")]
    public static void SetupObjective()
    {
        if (Object.FindAnyObjectByType<GameplayObjective>() == null)
        {
            var go = new GameObject("GameplayObjective");
            go.AddComponent<GameplayObjective>();
            Undo.RegisterCreatedObjectUndo(go, "Create GameplayObjective");
        }
        if (Object.FindAnyObjectByType<GameState>() == null)
        {
            var gm = Object.FindAnyObjectByType<GameManager>();
            var host = gm != null ? gm.gameObject : new GameObject("GameState");
            Undo.AddComponent<GameState>(host);
        }
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log("[M2] Objective tracker set up — the HUD now shows supplies gathered / rescue prompt.");
    }

    static void setInteractableLayer(GameObject go)
    {
        int layer = LayerMask.NameToLayer(GameManager.INTERACTABLE_LAYER_NAME);
        if (layer >= 0) go.layer = layer;
        else Debug.LogError("[M2] No 'Interactable' layer — set it in Project Settings > Tags and Layers.");
    }

    static void setupReadable(GameObject go, string title, string verb, string body, string flag, string objective)
    {
        var readable = go.AddComponent<WorldReadable>();
        var so = new SerializedObject(readable);
        so.FindProperty("title").stringValue = title;
        so.FindProperty("promptVerb").stringValue = verb;
        so.FindProperty("body").stringValue = body;
        so.FindProperty("discoverFlag").stringValue = flag;
        so.FindProperty("revealObjective").stringValue = objective;
        so.ApplyModifiedPropertiesWithoutUndo();

        setInteractableLayer(go);
    }

    static void colorize(GameObject go, Color color)
    {
        var renderer = go.GetComponent<Renderer>();
        if (renderer != null) renderer.sharedMaterial = new Material(Shader.Find("Standard")) { color = color };
    }

    static void placeInFront(GameObject go, float distance, float height)
    {
        var player = Object.FindAnyObjectByType<PlayerController>();
        Vector3 basePos = player != null ? player.transform.position : Vector3.zero;
        Vector3 forward = player != null ? player.transform.forward  : Vector3.forward;

        Vector3 pos = basePos + forward * distance;
        pos.y = basePos.y + height;
        go.transform.position = pos;
    }

    static void finish(GameObject go, string label)
    {
        Undo.RegisterCreatedObjectUndo(go, label);
        Selection.activeGameObject = go;
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log($"[M2] {go.name} created in front of the player. Move it onto a surface, set its text in the " +
                  "Inspector (WorldReadable), then Ctrl+S.");
    }
}
