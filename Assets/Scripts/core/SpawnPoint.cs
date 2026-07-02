using UnityEngine;

// Marks where the player appears after a scene transition. A TransitionDoor sets
// nextSpawnId before loading; when the new scene starts, the SpawnPoint with that
// id moves the player onto itself (facing the spawn point's forward direction).
// If no id was set (e.g. entering play mode directly), the player stays where the
// scene placed them.
public class SpawnPoint : MonoBehaviour
{
    // well-known ids so doors and spawn points agree without magic strings
    public const string SAFE_ROOM_DOOR = "SafeRoomDoor";
    public const string STORE_DOOR     = "StoreDoor";

    public static string nextSpawnId;

    [SerializeField] string id = STORE_DOOR;

    void Start()
    {
        if (nextSpawnId != id) return;
        nextSpawnId = null;

        PlayerController player = FindAnyObjectByType<PlayerController>();
        if (player == null) {
            Debug.LogWarning($"SpawnPoint '{id}': no PlayerController in scene.");
            return;
        }

        // CharacterController overrides transform writes while enabled, so toggle it
        CharacterController controller = player.GetComponent<CharacterController>();
        if (controller != null) controller.enabled = false;

        player.transform.SetPositionAndRotation(
            transform.position,
            Quaternion.Euler(0f, transform.eulerAngles.y, 0f));

        if (controller != null) controller.enabled = true;
    }
}
