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
            applyEarlyReturnBond();
            DayCycle.endRun();
        }
        else if (targetScene == GameManager.SCENE_MAIN) {
            DayCycle.startRun();
        }

        SpawnPoint.nextSpawnId = targetSpawnId;
        SceneLoader.load(targetScene);
    }

    // Ending the run early means more evening with the friend (spec: time remaining
    // converts to a small bond bump; letting night fall forfeits it).
    void applyEarlyReturnBond()
    {
        GameState state = GameState.Instance;
        if (state != null) state.setCounter(GameManager.COUNTER_LAST_RUN_BOND, 0);

        DaylightTimer timer = DaylightTimer.Instance;
        if (timer == null || timer.NightFell) return;

        int minutesLeft = Mathf.FloorToInt(timer.RemainingSeconds / 60f);
        int bump = minutesLeft * GameManager.BOND_PER_EARLY_MINUTE;
        if (bump > 0 && state != null) {
            state.addCounter(GameManager.COUNTER_BOND, bump);
            // remembered separately so the night check-in can acknowledge it
            state.setCounter(GameManager.COUNTER_LAST_RUN_BOND, bump);
        }
    }
}
