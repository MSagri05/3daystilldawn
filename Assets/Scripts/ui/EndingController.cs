using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Builds the ending screen from the run's accumulated GameState, so the outcome is a
// consequence of prior play (supplies gathered, the bond with Mia, whether the player
// survived) rather than a single final choice. Lives in the Ending scene.
public class EndingController : MonoBehaviour
{
    static readonly Color BG = new Color(0.05f, 0.06f, 0.08f, 1f);

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        build();
    }

    (string title, string body) resolveEnding()
    {
        var s = GameState.Instance;
        if (s == null)
            return ("THREE DAYS TILL DAWN", "Thanks for playing the prototype.");

        if (s.getFlag(GameManager.FLAG_DIED))
            return ("YOU DIED",
                s.getFlag(GameManager.FLAG_FRIEND_MET)
                    ? "The dark swallowed you before the third dawn. Somewhere in the mart, Mia waits for a friend who will never come back."
                    : "The horde caught you alone in the aisles. No one was left to remember your name.");

        // TODO(day cycle): the day-3 ending cascade goes here — health gate first,
        // then bond (TURNS / SLIPS_AWAY / BOTH_SAVED per the Friend System spec).
        return ("THREE DAYS TILL DAWN", "The story isn't over yet...");
    }

    void build()
    {
        UiFactory.ensureEventSystem();
        var canvas = UiFactory.overlayCanvas(transform, "EndingCanvas");

        var bg = UiFactory.image(canvas.transform, "Background", BG);
        UiFactory.stretch(bg.rectTransform);

        var (title, body) = resolveEnding();

        var titleLabel = UiFactory.text(canvas.transform, "Title", title, 84,
            new Color(0.85f, 0.3f, 0.25f, 1f), TextAlignmentOptions.Center);
        titleLabel.fontStyle = FontStyles.Bold;
        UiFactory.anchor(titleLabel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        titleLabel.rectTransform.anchoredPosition = new Vector2(0, 240);
        titleLabel.rectTransform.sizeDelta = new Vector2(1500, 140);

        var bodyLabel = UiFactory.text(canvas.transform, "Body", body, 34, Color.white, TextAlignmentOptions.Top);
        UiFactory.anchor(bodyLabel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        bodyLabel.rectTransform.anchoredPosition = new Vector2(0, 40);
        bodyLabel.rectTransform.sizeDelta = new Vector2(1200, 260);

        var button = UiFactory.button(canvas.transform, "TitleButton", "Return to Title", 30f);
        UiFactory.anchor(button.image.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        button.image.rectTransform.anchoredPosition = new Vector2(0, -230);
        button.image.rectTransform.sizeDelta = new Vector2(360, 72);
        button.onClick.AddListener(() =>
        {
            if (GameState.Instance != null) resetRun();
            SceneLoader.load(GameManager.SCENE_TITLE);
        });
    }

    // clear the run's narrative state so a new playthrough starts fresh
    void resetRun()
    {
        var s = GameState.Instance;
        s.clearFlag(GameManager.FLAG_DIED);
        s.clearFlag(GameManager.FLAG_FRIEND_MET);
        s.clearFlag(GameManager.FLAG_FRIEND_RESTING);
        s.clearFlag(GameManager.FLAG_REASSURED);
        s.setCounter(GameManager.COUNTER_BOND, 0);
    }
}
