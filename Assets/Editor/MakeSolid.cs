using UnityEngine;
using UnityEditor;

// One-click pass to make map props solid and static. Select the props' root(s) in
// the Hierarchy and run Tools > M2 > Make Selection Solid: every child mesh gets a
// MeshCollider (unless it already has some collider) and is marked fully static.
// Entities (player, zombies, NPCs) and Interactable-layer objects are skipped.
// The whole pass is one Undo step.
public static class MakeSolid
{
    [MenuItem("Tools/M2/Make Selection Solid")]
    public static void makeSelectionSolid()
    {
        if (Selection.gameObjects.Length == 0)
        {
            Debug.LogWarning("[MakeSolid] Nothing selected — select the map/props root first.");
            return;
        }

        int added = 0, marked = 0, skipped = 0;
        int interactableLayer = LayerMask.NameToLayer(GameManager.INTERACTABLE_LAYER_NAME);

        Undo.SetCurrentGroupName("Make Selection Solid");
        int undoGroup = Undo.GetCurrentGroup();

        foreach (GameObject root in Selection.gameObjects)
        {
            foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            {
                GameObject go = t.gameObject;

                if (isEntity(t) || go.layer == interactableLayer)
                {
                    skipped++;
                    continue;
                }

                MeshFilter filter = go.GetComponent<MeshFilter>();
                if (filter != null && filter.sharedMesh != null && go.GetComponent<Collider>() == null)
                {
                    Undo.AddComponent<MeshCollider>(go);
                    added++;
                }

                // Unity 6 dropped StaticEditorFlags.Everything — ~0 sets every flag bit
                if (!go.isStatic)
                {
                    Undo.RecordObject(go, "Set Static");
                    GameObjectUtility.SetStaticEditorFlags(go, (StaticEditorFlags)~0);
                    marked++;
                }
            }
        }

        Undo.CollapseUndoOperations(undoGroup);
        Debug.Log($"[MakeSolid] Added {added} MeshColliders, marked {marked} objects static, " +
                  $"skipped {skipped} (entities / Interactable layer). One Ctrl+Z reverts.");
    }

    // Anything that moves must never be made static or given a scenery collider.
    static bool isEntity(Transform t)
    {
        return t.GetComponentInParent<PlayerController>() != null
            || t.GetComponentInParent<Zombie>() != null
            || t.GetComponentInParent<Npc>() != null
            || t.GetComponentInParent<CharacterController>() != null;
    }
}
