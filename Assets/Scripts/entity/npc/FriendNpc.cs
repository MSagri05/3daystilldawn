using UnityEngine;

// The player's stranded friend. Interactable ([E]) — a short branching conversation
// that builds a "bond" and sets story flags in GameState, which later decide the
// ending. Put this on the NPC (same object as Npc) and set its layer to Interactable.
public class FriendNpc : MonoBehaviour, IInteractable
{
    [SerializeField] string friendName = "Mia";

    public string getPrompt() => "Talk to " + friendName;

    public void interact(PlayerInteractor interactor)
    {
        var dialogue = DialogueUI.Instance;
        if (dialogue == null || dialogue.IsOpen) return;

        var state = GameState.Instance;
        if (DayCycle.CurrentPhase == DayCycle.Phase.Night)
            nightCheckIn(dialogue, state);
        else if (state != null && state.getFlag(GameManager.FLAG_FRIEND_MET))
            talkAgain(dialogue, state);
        else
            firstMeeting(dialogue, state);
    }

    // The night check-in: talking is the one night action so far (feed / comfort item /
    // rest come later). Closing the conversation resolves the night and starts the next
    // morning — or, after the last day, the ending cascade.
    void nightCheckIn(DialogueUI dialogue, GameState state)
    {
        state?.addCounter(GameManager.COUNTER_BOND, GameManager.BOND_TALK_AT_NIGHT);

        int banked = state != null ? state.getCounter(GameManager.COUNTER_LAST_RUN_BOND) : 0;
        string line = banked > 0
            ? "You came back while it was still light... it helps, having you here. Stay a while?"
            : "It got so dark out there. I keep thinking one of these nights you won't come back.";

        dialogue.show(friendName, line, () =>
        {
            dialogue.close();
            DayCycle.resolveNight();
        });
    }

    // Opening conversation: one branching choice that sets the tone of the relationship.
    void firstMeeting(DialogueUI dialogue, GameState state)
    {
        dialogue.show(friendName, "You're alive... I really thought that horde got you.", () =>
            dialogue.showChoice(friendName,
                "My leg's wrecked — I can't run like this. What do we do?",
                new[]
                {
                    "We get out together. I'm not leaving you.",
                    "Stay and rest here. I'll find us a way out."
                },
                pick =>
                {
                    string reply;
                    if (pick == 0)
                    {
                        if (state != null)
                        {
                            state.addCounter(GameManager.COUNTER_BOND, 2);
                            state.setFlag(GameManager.FLAG_REASSURED);
                        }
                        reply = "...Okay. Together. Don't you dare leave me behind.";
                    }
                    else
                    {
                        if (state != null)
                        {
                            state.addCounter(GameManager.COUNTER_BOND, 1);
                            state.setFlag(GameManager.FLAG_FRIEND_RESTING);
                        }
                        reply = "Yeah... hurry. I don't feel so good.";
                    }

                    if (state != null) state.setFlag(GameManager.FLAG_FRIEND_MET);
                    dialogue.show(friendName, reply, () => dialogue.close());
                }));
    }

    // Later visits: an observational hint that the friend's condition is worsening.
    // TODO(night check-in): vary this by day number and friend health per the spec —
    // talkative early, scared and distant midway, barely responsive by the end.
    void talkAgain(DialogueUI dialogue, GameState state)
    {
        dialogue.show(friendName,
            "You're back... good. My fever's worse — don't say it's nothing.",
            () => dialogue.close());
    }
}
