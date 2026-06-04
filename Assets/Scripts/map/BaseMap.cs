using UnityEngine;

// Abstract base class for all maps
public abstract class BaseMap : MonoBehaviour
{
    public int width = 10;
    public int height = 10;
    public float cellSize = 1f;

    public abstract void OnMapLoad();
    public abstract void OnMapUnload();

    public Vector3 CellToWorld(Vector2Int pos)
    {
        float half = cellSize * 0.5f;
        return new Vector3(pos.x * cellSize + half, pos.y * cellSize + half, 0f);
    }

    public bool IsInBounds(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height;
    }
}
