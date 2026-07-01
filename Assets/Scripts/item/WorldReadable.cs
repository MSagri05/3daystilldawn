using UnityEngine;

// A piece of environmental storytelling the player discovers by exploring: a readable note
// / text fragment, or an examinable trace ("dried blood leads to the back room"). Interact
// ([E]) to show the text in the dialogue window; the first read can raise a GameState flag
// and/or update the player's current objective. Put it on the Interactable layer.
[RequireComponent(typeof(Collider))]
public class WorldReadable : MonoBehaviour, IInteractable
{
    [SerializeField] string title = "Note";
    [SerializeField] string promptVerb = "Read";                 // "Read" for notes, "Examine" for traces
    [TextArea(2, 6)] [SerializeField] string body = "";

    [Header("Optional narrative hooks")]
    [SerializeField] string discoverFlag = "";                   // GameState flag set on first read
    [SerializeField] string revealObjective = "";               // sets the HUD objective when read
    [SerializeField] bool removeAfterReading = false;            // e.g. a note the player pockets

    public string getPrompt() => promptVerb + " " + title;

    public void interact(PlayerInteractor interactor)
    {
        var dialogue = DialogueUI.Instance;
        if (dialogue == null || dialogue.IsOpen) return;

        var state = GameState.Instance;
        bool firstTime = state == null || string.IsNullOrEmpty(discoverFlag) || !state.getFlag(discoverFlag);

        dialogue.show(title, body, () => dialogue.close());

        if (firstTime)
        {
            if (state != null && !string.IsNullOrEmpty(discoverFlag))
                state.setFlag(discoverFlag);

            if (!string.IsNullOrEmpty(revealObjective) && PlayerHUD.Instance != null)
                PlayerHUD.Instance.setObjective(revealObjective);
        }

        if (removeAfterReading)
            Destroy(gameObject);
    }
}
