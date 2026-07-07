using UnityEngine;

// Day escalation: the store gets more dangerous as the three days pass. On scene
// load, spawns extra zombies at marked points based on the current day — none on
// day 1 (the hand-placed set is the baseline), more each day after. Put one in the
// store scene and assign spawn points; day 1 runs are untouched.
public class DayZombieSpawner : MonoBehaviour
{
    [Tooltip("Where escalation zombies appear. Spread these away from the store entrance.")]
    [SerializeField] Transform[] spawnPoints;

    [Tooltip("Zombie prefab. If unset, an existing scene zombie is cloned.")]
    [SerializeField] GameObject zombiePrefab;

    void Start()
    {
        int extras = (DayCycle.CurrentDay - 1) * GameManager.ZOMBIES_PER_EXTRA_DAY;
        ZombieSpawning.spawnAt(zombiePrefab, spawnPoints, extras, "DayZombieSpawner");
    }
}
