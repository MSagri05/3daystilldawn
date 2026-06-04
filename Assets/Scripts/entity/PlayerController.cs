using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 12f;

    private BaseMap map;
    private Vector2Int gridPos;
    private Vector3 targetWorldPos;
    private bool isMoving;

    void Start()
    {
        map = FindFirstObjectByType<BaseMap>();
        gridPos = new Vector2Int(map.width / 2, map.height / 2);
        targetWorldPos = map.CellToWorld(gridPos);
        transform.position = targetWorldPos;
    }

    void Update()
    {
        if (!isMoving)
            HandleInput();

        SmoothMove();
    }

    void HandleInput()
    {
        Vector2Int dir = Vector2Int.zero;

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            dir = Vector2Int.up;
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            dir = Vector2Int.down;
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            dir = Vector2Int.left;
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            dir = Vector2Int.right;

        if (dir != Vector2Int.zero)
            TryMove(dir);
    }

    void TryMove(Vector2Int dir)
    {
        Vector2Int next = gridPos + dir;
        if (!map.IsInBounds(next)) return;

        gridPos = next;
        targetWorldPos = map.CellToWorld(gridPos);
        isMoving = true;
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
