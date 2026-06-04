using UnityEngine;

// Designer-friendly: replace Assets/Resources/Sprites/Player.png
// and press Play — no Inspector changes needed.
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerVisual : MonoBehaviour
{
    void Start()
    {
        var sprite = Resources.Load<Sprite>("Sprites/Player");
        if (sprite != null)
            GetComponent<SpriteRenderer>().sprite = sprite;
        else
            Debug.LogWarning("Missing file: Assets/Resources/Sprites/Player.png");
    }
}
