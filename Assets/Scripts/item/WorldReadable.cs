using UnityEngine;

// A piece of environmental storytelling the player discovers by exploring: a readable note
// / text fragment, or an examinable trace ("dried blood leads to the back room"). Interact
// ([E]) to show the text in the dialogue window; the first read can raise a GameState flag.
// Put it on the Interactable layer.
[RequireComponent(typeof(Collider))]
public class WorldReadable : MonoBehaviour, IInteractable
{
    [SerializeField] string title = "Note";
    [SerializeField] string promptVerb = "Read";                 // "Read" for notes, "Examine" for traces
    [TextArea(2, 6)] [SerializeField] string body = "";

    [Header("Optional narrative hooks")]
    [SerializeField] string discoverFlag = "";                   // GameState flag set on first read
    [SerializeField] bool removeAfterReading = false;            // e.g. a note the player pockets

    public string getPrompt() => promptVerb + " " + title;

    public void interact(PlayerInteractor interactor)
    {
        var dialogue = DialogueUI.Instance;
        if (dialogue == null || dialogue.IsOpen) return;

        var state = GameState.Instance;
        bool firstTime = state == null || string.IsNullOrEmpty(discoverFlag) || !state.getFlag(discoverFlag);

        dialogue.show(title, body, () => dialogue.close());

        if (firstTime && state != null && !string.IsNullOrEmpty(discoverFlag))
            state.setFlag(discoverFlag);

        if (removeAfterReading)
            Destroy(gameObject);
    }
}
