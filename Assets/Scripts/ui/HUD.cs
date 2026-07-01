using UnityEngine;
using TMPro;

public class HUD : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI promptText;

    void Awake()
    {
        if (promptText == null) return;

        // dark outline keeps the white "[E] ..." prompt legible over bright surfaces
        promptText.color = Color.white;
        UiFactory.outline(promptText);

        // drop it below the centre so it doesn't overlap the crosshair
        var rt = promptText.rectTransform;
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(0f, -130f);
        promptText.alignment = TMPro.TextAlignmentOptions.Center;
    }

    public void setPrompt(string text)
    {
        if (promptText != null) {
            promptText.text = text;
        }
    }
}
