using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Reusable content/scene tools for the game (the one-time bootstrap tools were removed once
// the scene was set up). Menu: Tools > M2.
public static class M2Setup
{
    const float ZOMBIE_HEIGHT = 1.9f;   // zombies stand a touch taller than the 1.8m player

    // Tools > M2 > Fix Lighting — keep one directional light at a sane intensity with soft
    // shadows and a moderate ambient, so surfaces aren't blown out to white.
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
                l.intensity = 0.85f;
                l.shadows = LightShadows.Soft;
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

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.26f, 0.28f, 0.32f);
        RenderSettings.ambientIntensity = 1f;

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log($"[M2] Lighting balanced: 1 directional @0.85 (soft shadows) + ambient, disabled {disabled} extra. " +
                  "Tweak Directional Intensity (lit side) / Environment Ambient (dark side) to taste. Ctrl+S.");
    }

    // Tools > M2 > Match NPC Height To Player — scale the NPC model to the player's 1.8m,
    // wrap its capsule to the model bounds, and stand it on the floor.
    [MenuItem("Tools/M2/Match NPC Height To Player (1.8m)")]
    public static void MatchNpcHeight()
    {
        var npc = Object.FindAnyObjectByType<Npc>();
        if (npc == null) { Debug.LogError("[M2] No Npc in the scene."); return; }

        if (!tryMeasure(npc.gameObject, out Bounds bounds))
        {
            Debug.LogError("[M2] NPC has no Renderer to measure. Aborting.");
            return;
        }

        float currentHeight = bounds.size.y;
        if (currentHeight > 0.001f)
        {
            Undo.RecordObject(npc.transform, "Match NPC height");
            npc.transform.localScale *= GameManager.PLAYER_HEIGHT / currentHeight;
            Debug.Log($"[M2] Scaled NPC from {currentHeight:0.00}m to {GameManager.PLAYER_HEIGHT}m tall.");
        }

        var extra = npc.GetComponent<CapsuleCollider>();
        if (extra != null) { Undo.DestroyObjectImmediate(extra); }   // keep only the CharacterController

        snapToFloor(npc.gameObject);
        wrapCapsule(npc.gameObject);

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log("[M2] NPC height matched and placed on the floor. Ctrl+S to keep.");
    }

    // Tools > M2 > Apply Zombie Model — replaces the placeholder capsule visual on every zombie
    // with the Zombie_Male model (kept as a child), then sizes it and stands it on the floor.
    [MenuItem("Tools/M2/Apply Zombie Model")]
    public static void ApplyZombieModel()
    {
        var model = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Models/Zombie_Male.fbx");
        if (model == null) { Debug.LogError("[M2] Assets/Models/Zombie_Male.fbx not found."); return; }

        var zombies = Object.FindObjectsByType<Zombie>(FindObjectsSortMode.None);
        if (zombies.Length == 0) { Debug.LogError("[M2] No Zombie in the scene."); return; }

        int applied = 0;
        foreach (var z in zombies)
        {
            if (z.GetComponentInChildren<SkinnedMeshRenderer>() != null) continue;   // already has a model

            // hide the primitive capsule visual, but keep the CharacterController for collision
            var mr = z.GetComponent<MeshRenderer>();
            var mf = z.GetComponent<MeshFilter>();
            if (mr != null) Undo.DestroyObjectImmediate(mr);
            if (mf != null) Undo.DestroyObjectImmediate(mf);

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(model, z.transform);
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            Undo.RegisterCreatedObjectUndo(instance, "Apply Zombie Model");

            // size the model, wrap the capsule around it, and drop it onto the floor
            if (tryMeasure(z.gameObject, out Bounds b) && b.size.y > 0.001f)
            {
                Undo.RecordObject(z.transform, "Scale Zombie");
                z.transform.localScale *= ZOMBIE_HEIGHT / b.size.y;
            }
            snapToFloor(z.gameObject);
            wrapCapsule(z.gameObject);
            applied++;
        }

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log($"[M2] Applied the zombie model to {applied} zombie(s). It stands in a T-pose until an " +
                  "Animator is added (import the FBX as Humanoid + use Tools > Setup Idle Animation). Ctrl+S.");
    }

    // ---------------------------------------------------------------- shared model helpers

    static bool tryMeasure(GameObject go, out Bounds bounds)
    {
        var renderers = go.GetComponentsInChildren<Renderer>();
        bounds = default;
        if (renderers.Length == 0) return false;

        bounds = renderers[0].bounds;
        foreach (var r in renderers) bounds.Encapsulate(r.bounds);
        return true;
    }

    // Lifts/drops the object so the bottom of its model rests on the floor below it. Its own
    // colliders are disabled during the raycast so it can't hit itself.
    static void snapToFloor(GameObject go)
    {
        if (!tryMeasure(go, out Bounds bounds)) return;

        var colliders = go.GetComponentsInChildren<Collider>();   // CharacterController is a Collider too
        var wasEnabled = new bool[colliders.Length];
        for (int i = 0; i < colliders.Length; i++) { wasEnabled[i] = colliders[i].enabled; colliders[i].enabled = false; }

        Vector3 origin = new Vector3(bounds.center.x, bounds.max.y + 0.3f, bounds.center.z);
        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 200f))
        {
            Undo.RecordObject(go.transform, "Snap to floor");
            go.transform.position += Vector3.up * (hit.point.y - bounds.min.y);
        }
        else
        {
            Debug.LogWarning("[M2] Couldn't find a floor under '" + go.name + "' to snap to — position it manually.");
        }

        for (int i = 0; i < colliders.Length; i++) colliders[i].enabled = wasEnabled[i];
    }

    // Rebuilds the CharacterController so its capsule wraps the model, regardless of pivot.
    static void wrapCapsule(GameObject go)
    {
        var cc = go.GetComponent<CharacterController>();
        if (cc == null || !tryMeasure(go, out Bounds b)) return;

        Transform t = go.transform;
        float sy  = Mathf.Approximately(t.lossyScale.y, 0f) ? 1f : t.lossyScale.y;
        float sxz = Mathf.Max(Mathf.Abs(t.lossyScale.x), Mathf.Abs(t.lossyScale.z));
        if (sxz <= 0f) sxz = 1f;

        float worldCenterY = b.min.y + b.size.y * 0.5f;
        Undo.RecordObject(cc, "Wrap capsule");
        cc.height = b.size.y / sy;
        cc.radius = GameManager.PLAYER_RADIUS / sxz;
        cc.center = new Vector3(0f, (worldCenterY - t.position.y) / sy, 0f);
    }

    // ---------------------------------------------------------------- content spawners

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

    // ---------------------------------------------------------------- spawn helpers

    static void setInteractableLayer(GameObject go)
    {
        int layer = LayerMask.NameToLayer(GameManager.INTERACTABLE_LAYER_NAME);
        if (layer >= 0) go.layer = layer;
        else Debug.LogError("[M2] No 'Interactable' layer — set it in Project Settings > Tags and Layers.");
    }

    static void setupReadable(GameObject go, string title, string verb, string body, string flag, string objective = "")
    {
        var readable = go.AddComponent<WorldReadable>();
        var so = new SerializedObject(readable);
        so.FindProperty("title").stringValue = title;
        so.FindProperty("promptVerb").stringValue = verb;
        so.FindProperty("body").stringValue = body;
        so.FindProperty("discoverFlag").stringValue = flag;
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
        Debug.Log($"[M2] {go.name} created in front of the player. Move it into place, then Ctrl+S.");
    }
}
