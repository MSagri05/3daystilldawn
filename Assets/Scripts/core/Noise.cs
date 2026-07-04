using System;
using UnityEngine;

// Global noise event bus. Anything loud calls emit(position, radius); listeners
// (zombies) decide whether they were close enough to hear it. Static like DayCycle,
// so emitters and listeners never need references to each other — one call site per
// new sound source (footsteps now; dropped items, doors, distractions later).
public static class Noise
{
    // (position of the sound, how far it carries in units)
    public static event Action<Vector3, float> onNoise;

    public static void emit(Vector3 position, float radius)
    {
        onNoise?.Invoke(position, radius);
    }
}
