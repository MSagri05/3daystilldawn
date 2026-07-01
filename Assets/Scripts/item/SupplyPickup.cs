using UnityEngine;

// A scavengeable supply (meds / food / bandages) that counts toward the goal the player
// needs before they can call for rescue. Picking it up bumps the GameState supply counter,
// which the objective HUD and the ending logic read. Put it on the Interactable layer.
[RequireComponent(typeof(Collider))]
public class SupplyPickup : MonoBehaviour, IInteractable
{
    [SerializeField] string label = "supplies";

    public string getPrompt() => "Take " + label;

    public void interact(PlayerInteractor interactor)
    {
        var state = GameState.Instance;
        if (state != null)
            state.addCounter(GameManager.COUNTER_SUPPLIES, 1);   // objective HUD updates via onCounterChanged

        Destroy(gameObject);
    }
}
