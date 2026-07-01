using UnityEngine;

// The rescue point (rooftop signal). Interacting here ends the run: if the player has
// gathered enough supplies for Mia they escape (good ending); otherwise the friend won't
// leave without meds, and the player is nudged to keep scavenging. Interactable layer.
[RequireComponent(typeof(Collider))]
public class EscapeZone : MonoBehaviour, IInteractable
{
    bool ready() => GameState.Instance != null &&
                    GameState.Instance.getCounter(GameManager.COUNTER_SUPPLIES) >= GameManager.SUPPLIES_GOAL;

    public string getPrompt() => ready() ? "Signal for rescue" : "Rescue point — gather supplies first";

    public void interact(PlayerInteractor interactor)
    {
        if (ready())
        {
            GameState.Instance.setFlag(GameManager.FLAG_ESCAPED);
            if (Application.CanStreamedLevelBeLoaded(GameManager.SCENE_ENDING))
                SceneLoader.load(GameManager.SCENE_ENDING);
            else
                Debug.LogWarning("[Escape] Ending scene isn't in Build Settings — run Tools > M2 > Build Ending Scene.");
            return;
        }

        // not enough yet — explain why through the friend's voice, and re-assert the objective
        int have = GameState.Instance != null ? GameState.Instance.getCounter(GameManager.COUNTER_SUPPLIES) : 0;
        if (DialogueUI.Instance != null && !DialogueUI.Instance.IsOpen)
            DialogueUI.Instance.show("Mia",
                "I'm not leaving without something for this fever. Find more supplies first — please.",
                () => DialogueUI.Instance.close());

        if (PlayerHUD.Instance != null)
            PlayerHUD.Instance.setObjective($"Find supplies for Mia ({have}/{GameManager.SUPPLIES_GOAL})");
    }
}
