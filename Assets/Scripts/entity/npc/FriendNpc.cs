using System;
using System.Collections.Generic;
using UnityEngine;

// The player's stranded friend. Interactable ([E]). During the day: short story
// conversations. At night (after returning from a run): the night check-in — an
// action menu (feed / medicine / comfort item / talk / rest) that spends items and
// evening time to move the two hidden axes, health and bond, which decide the
// ending. Put this on the NPC (same object as Npc), layer Interactable.
public class FriendNpc : MonoBehaviour, IInteractable
{
    [SerializeField] string friendName = "Mia";

    bool talkedTonight;   // the +bond for talking applies once per night

    public string getPrompt() => "Talk to " + friendName;

    public void interact(PlayerInteractor interactor)
    {
        var dialogue = DialogueUI.Instance;
        if (dialogue == null || dialogue.IsOpen) return;

        var state = GameState.Instance;
        if (DayCycle.CurrentPhase == DayCycle.Phase.Night)
            nightCheckIn(dialogue, state, interactor.getInventory());
        else if (state != null && state.getFlag(GameManager.FLAG_FRIEND_MET))
            talkAgain(dialogue, state);
        else
            firstMeeting(dialogue, state);
    }

    // ---------------------------------------------------------------- night check-in

    void nightCheckIn(DialogueUI dialogue, GameState state, Inventory inventory)
    {
        talkedTonight = false;
        dialogue.show(friendName, nightOpener(state), () => nightMenu(dialogue, state, inventory));
    }

    // Three distinct nights (spec): talkative day 1, scared and distant day 2, barely
    // responsive day 3 — with the low-health decline audible on any night, and the
    // early return (banked bond) acknowledged.
    string nightOpener(GameState state)
    {
        bool early  = state != null && state.getCounter(GameManager.COUNTER_LAST_RUN_BOND) > 0;
        bool sick   = state != null && state.getCounter(GameManager.COUNTER_FRIEND_HEALTH) < GameManager.HEALTH_LINE;

        if (sick)
            return "...Don't look at me like that. I can feel it moving, you know. Under the skin. " +
                   "Whatever you were going to do tonight... do it soon.";

        switch (DayCycle.CurrentDay)
        {
            case 1:
                return early
                    ? "You're back early! Good — this place gets so quiet I start naming the shopping carts. Sit with me?"
                    : "There you are! Okay — tell me everything about out there. Don't skip the gross parts.";
            case 2:
                return early
                    ? "You came back while it was still light... thank you. I keep hearing them against the walls at night."
                    : "I didn't think you were coming back. Don't do that to me again. Please.";
            default:
                return early
                    ? "...You're here. Sorry, it's... hard to stay awake now. Stay close, okay?"
                    : "...Mm. It's dark. Everything aches... don't go far.";
        }
    }

    void nightMenu(DialogueUI dialogue, GameState state, Inventory inventory)
    {
        var labels  = new List<string>();
        var actions = new List<Action>();

        ItemData food = inventory != null ? inventory.firstOfType(ItemType.Survival) : null;
        if (food != null) {
            labels.Add($"Give her the {food.itemName}");
            actions.Add(() => giveItem(dialogue, state, inventory, food,
                GameManager.FRIEND_HEALTH_FOOD, 0,
                "She eats slowly, like she has to remember how. A little color comes back to her face."));
        }

        ItemData meds = inventory != null ? inventory.firstOfType(ItemType.Medicine) : null;
        if (meds != null) {
            labels.Add($"Give her the {meds.itemName}");
            actions.Add(() => giveItem(dialogue, state, inventory, meds,
                GameManager.FRIEND_HEALTH_MEDICINE, 0,
                "She swallows the meds without a word. Her breathing evens out, just slightly."));
        }

        ItemData comfort = inventory != null ? inventory.firstOfType(ItemType.Comfort) : null;
        if (comfort != null) {
            labels.Add($"Give her the {comfort.itemName}");
            actions.Add(() => giveItem(dialogue, state, inventory, comfort,
                0, GameManager.BOND_COMFORT_ITEM,
                $"She stares at the {comfort.itemName} for a long moment. \"You remembered,\" she says, and something in her settles."));
        }

        labels.Add("Talk");
        actions.Add(() => talkTonight(dialogue, state, inventory));

        labels.Add("Rest until morning");
        actions.Add(() => rest(dialogue, state));

        dialogue.showChoice(friendName, "The night stretches ahead of you both.",
                            labels.ToArray(), pick => actions[pick]());
    }

    void giveItem(DialogueUI dialogue, GameState state, Inventory inventory, ItemData item,
                  int healthDelta, int bondDelta, string response)
    {
        inventory.removeItem(item);
        addClamped(state, GameManager.COUNTER_FRIEND_HEALTH, healthDelta);
        addClamped(state, GameManager.COUNTER_BOND, bondDelta);
        dialogue.show(friendName, response, () => nightMenu(dialogue, state, inventory));
    }

    void talkTonight(DialogueUI dialogue, GameState state, Inventory inventory)
    {
        if (!talkedTonight) {
            talkedTonight = true;
            addClamped(state, GameManager.COUNTER_BOND, GameManager.BOND_TALK_AT_NIGHT);
        }

        string line;
        switch (DayCycle.CurrentDay)
        {
            case 1:
                line = "Remember the road trip where we lost the tent? And you swore the raccoon was \"basically a bear\"? " +
                       "...We're getting out of this one too.";
                break;
            case 2:
                line = "Be honest with me. If I start to turn... no. Forget it. Just — talk about something else. Anything.";
                break;
            default:
                line = "...I'm still here. Keep talking... I like the sound better than the quiet.";
                break;
        }

        dialogue.show(friendName, line, () => nightMenu(dialogue, state, inventory));
    }

    void rest(DialogueUI dialogue, GameState state)
    {
        string line = DayCycle.CurrentDay >= GameManager.TOTAL_DAYS
            ? "Whatever tomorrow is... I'm glad you were here for this part."
            : "Get some sleep. I'll... try to do the same.";

        dialogue.show(friendName, line, () =>
        {
            dialogue.close();
            DayCycle.resolveNight();
        });
    }

    static void addClamped(GameState state, string key, int delta)
    {
        if (state == null || delta == 0) return;
        int value = Mathf.Clamp(state.getCounter(key) + delta, 0, GameManager.FRIEND_STAT_MAX);
        state.setCounter(key, value);
    }

    // ---------------------------------------------------------------- day conversations

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

    // Daytime small talk: no mechanics, but the overnight decline has to be readable
    // here (spec: the meter is hidden, the morning shows what the night cost).
    void talkAgain(DialogueUI dialogue, GameState state)
    {
        int health = state != null ? state.getCounter(GameManager.COUNTER_FRIEND_HEALTH) : GameManager.FRIEND_HEALTH_START;

        string line;
        if (health < GameManager.HEALTH_LINE)
            line = "...Hey. Don't say it's nothing — I saw my eyes in the window this morning. " +
                   "If you're going out there, find something strong. Today.";
        else if (health < GameManager.FRIEND_HEALTH_START)
            line = "Rough night. The fever comes in waves now... I'm okay. Just — don't stay out too long.";
        else
            line = "I actually slept a little! Bring back something good, and maybe I'll even eat it.";

        dialogue.show(friendName, line, () => dialogue.close());
    }
}
