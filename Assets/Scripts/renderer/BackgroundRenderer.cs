using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class BackgroundRenderer : MonoBehaviour
{
    public string spritePath = GameManager.BACKGROUND_SPRITE;

    void Start()
    {
        Sprite sprite = Resources.Load<Sprite>(spritePath);
        if (sprite == null) {
            return;
        }

        float width = GameManager.MAP_WIDTH * GameManager.CELL_SIZE * 1.9f;
        float height = GameManager.MAP_HEIGHT * GameManager.CELL_SIZE * 1.5f;

        var renderer = GetComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = GameManager.BACKGROUND_SORTING_ORDER;

        float mapCenterX = GameManager.MAP_WIDTH * GameManager.CELL_SIZE * 0.5f;
        float mapCenterY = GameManager.MAP_HEIGHT * GameManager.CELL_SIZE * 0.5f;
        transform.position = new Vector3(mapCenterX, mapCenterY, GameManager.BACKGROUND_Z);
        Vector2 spriteSize = sprite.bounds.size;
        transform.localScale = new Vector3(width / spriteSize.x, height / spriteSize.y, 1f);
    }
}
