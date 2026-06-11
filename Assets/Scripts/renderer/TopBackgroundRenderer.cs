using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class TopBackgroundRenderer : MonoBehaviour
{
    void Start()
    {
        Sprite sprite = Resources.Load<Sprite>(GameManager.BACKGROUND_TOP_SPRITE);
        if (sprite == null) return;

        float mapWidth = GameManager.MAP_WIDTH * GameManager.CELL_SIZE;
        float mapHeight = GameManager.MAP_HEIGHT * GameManager.CELL_SIZE;
        Vector2 spriteSize = sprite.bounds.size;
        float scaledHeight = mapWidth * (spriteSize.y / spriteSize.x);

        var sr = GetComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = GameManager.BACKGROUND_TOP_SORTING_ORDER;

        float mapCenterX = GameManager.MAP_WIDTH * GameManager.CELL_SIZE * 0.5f;
        transform.position = new Vector3(mapCenterX, mapHeight + scaledHeight * 0.5f, GameManager.BACKGROUND_Z);
        transform.localScale = new Vector3(mapWidth / spriteSize.x, scaledHeight / spriteSize.y, 1f);
    }
}
