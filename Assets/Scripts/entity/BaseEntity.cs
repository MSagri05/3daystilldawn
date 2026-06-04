using UnityEngine;

public abstract class BaseEntity : MonoBehaviour
{
    [Header("Entity")]
    public float moveSpeed = 12f;

    protected BaseMap map;
    protected Vector2Int gridPos;
    protected Vector3 targetWorldPos;
    protected bool isMoving;

    protected virtual void Start()
    {
        map = FindFirstObjectByType<BaseMap>();
        gridPos = StartPosition();
        targetWorldPos = map.CellToWorld(gridPos);
        transform.position = targetWorldPos;
    }

    protected virtual void Update()
    {
        OnTick();
        SmoothMove();
    }

    // Called every frame — override per entity (equivalent to Bukkit's tick())
    protected abstract void OnTick();

    // Override to set a custom spawn position
    protected virtual Vector2Int StartPosition()
    {
        return new Vector2Int(map.width / 2, map.height / 2);
    }

    protected bool TryMove(Vector2Int dir)
    {
        if (isMoving) return false;

        Vector2Int next = gridPos + dir;
        if (!map.IsInBounds(next)) return false;

        gridPos = next;
        targetWorldPos = map.CellToWorld(gridPos);
        isMoving = true;
        return true;
    }

    void SmoothMove()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetWorldPos, moveSpeed * Time.deltaTime);
        if (Vector3.Distance(transform.position, targetWorldPos) < 0.001f)
        {
            transform.position = targetWorldPos;
            isMoving = false;
        }
    }

    public Vector2Int GetGridPos() => gridPos;
}
