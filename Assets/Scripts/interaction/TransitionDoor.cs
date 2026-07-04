using UnityEngine;

// A door the player interacts with to move between the safe room and the store.
// Loads the target scene and tells the matching SpawnPoint there to position the
// player, so the same script works in both directions (one door per scene, each
// pointing at the other). Needs a collider on the Interactable layer.
public class TransitionDoor : MonoBehaviour, IInteractable
{
    [SerializeField] string targetScene = GameManager.SCENE_MAIN;
    [SerializeField] string targetSpawnId = SpawnPoint.STORE_DOOR;
    [SerializeField] string prompt = "Open door";

    public string getPrompt() => prompt;

    public void interact(PlayerInteractor interactor)
    {
        if (targetScene == GameManager.SCENE_SAFE_ROOM) {
            recordEarlyReturn();
            DayCycle.endRun();
        }
        else if (targetScene == GameManager.SCENE_MAIN) {
            DayCycle.startRun();
        }

        SpawnPoint.nextSpawnId = targetSpawnId;
        SceneLoader.load(targetScene);
    }

    // Record how early this return is (spec: time remaining converts to a bond bump).
    // Only recorded here — the bond itself banks when the player actually spends the
    // evening: FriendNpc applies it on "Rest until morning". Each return overwrites
    // the record, so the day's LAST return is what counts; nightfall forfeits it.
    void recordEarlyReturn()
    {
        GameState state = GameState.Instance;
        if (state == null) return;

        DaylightTimer timer = DaylightTimer.Instance;
        int bump = 0;
        if (timer != null && !timer.NightFell) {
            int minutesLeft = Mathf.FloorToInt(timer.RemainingSeconds / 60f);
            bump = minutesLeft * GameManager.BOND_PER_EARLY_MINUTE;
        }

        state.setCounter(GameManager.COUNTER_LAST_RUN_BOND, bump);
    }
}
