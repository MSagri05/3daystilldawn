using UnityEngine;

// Shared spawn helper for the systems that add zombies at marked points
// (night-fall extras, day escalation). If no prefab is assigned, an existing
// scene zombie is cloned so editor wiring stays optional.
public static class ZombieSpawning
{
    public static void spawnAt(GameObject prefab, Transform[] points, int count, string context)
    {
        if (count <= 0 || points == null || points.Length == 0) return;

        if (prefab == null) {
            Zombie existing = Object.FindAnyObjectByType<Zombie>();
            if (existing == null) {
                Debug.LogWarning($"[{context}] No zombie prefab assigned and no scene zombie to clone.");
                return;
            }
            prefab = existing.gameObject;
        }

        for (int i = 0; i < count; i++) {
            Transform point = points[i % points.Length];
            Object.Instantiate(prefab, point.position, point.rotation);
        }
    }
}
