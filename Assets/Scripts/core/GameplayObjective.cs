using UnityEngine;

// Keeps the player's current objective on the HUD, driven by GameState progress:
// gather supplies for Mia, then reach the rescue point. Put one in the gameplay scene.
public class GameplayObjective : MonoBehaviour
{
    void Start()
    {
        var state = GameState.Instance;
        if (state != null)
            state.onCounterChanged.AddListener(onCounterChanged);
        refresh();
    }

    void OnDestroy()
    {
        var state = GameState.Instance;
        if (state != null)
            state.onCounterChanged.RemoveListener(onCounterChanged);
    }

    void onCounterChanged(string key, int value)
    {
        if (key == GameManager.COUNTER_SUPPLIES) refresh();
    }

    void refresh()
    {
        if (PlayerHUD.Instance == null) return;

        int supplies = GameState.Instance != null ? GameState.Instance.getCounter(GameManager.COUNTER_SUPPLIES) : 0;
        PlayerHUD.Instance.setObjective(supplies >= GameManager.SUPPLIES_GOAL
            ? "Reach the rescue point"
            : $"Find supplies for Mia ({supplies}/{GameManager.SUPPLIES_GOAL})");
    }
}
