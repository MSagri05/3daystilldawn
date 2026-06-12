using UnityEngine;

public class HazardEntity : BaseEntity
{
    private static readonly Vector2Int[] DIRECTIONS = {
        Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
    };

    private const float SPIN_SPEED = 180f;

    private Player player;
    private Vector2Int spawnPos = new Vector2Int(-1, -1);
    private Vector2Int currentDir = Vector2Int.down;
    private float spinDirection = 0f;

    public void setSpawnPosition(Vector2Int pos)
    {
        spawnPos = pos;
    }

    protected override void Start()
    {
        spritePath = GameManager.HAZARD_SPRITE;
        base.Start();

        GetComponent<SpriteRenderer>().sortingOrder = 2;
        player = FindAnyObjectByType<Player>();
        currentDir = DIRECTIONS[Random.Range(0, DIRECTIONS.Length)];
        spinDirection = Random.value < 0.5f ? -1f : 1f;
        updateFacing(currentDir);

        targetWorldPos = map.cellToWorld(gridPos);
    }

    protected override void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.isPlaying()) return;

        transform.Rotate(0f, 0f, spinDirection * SPIN_SPEED * Time.deltaTime);

        checkPlayerContact();
        moveConstant();
    }

    private void moveConstant()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetWorldPos, GameManager.HAZARD_MOVE_SPEED * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetWorldPos) < 0.001f) {
            transform.position = targetWorldPos;

            Vector2Int next = gridPos + currentDir;
            if (map.canMoveTo(next)) {
                gridPos = next;
                targetWorldPos = map.cellToWorld(gridPos);
            } else {
                pickNewDirection();
            }
        }
    }

    private void pickNewDirection()
    {
        Vector2Int[] others = System.Array.FindAll(DIRECTIONS, d => d != currentDir);
        shuffle(others);
        foreach (var dir in others) {
            Vector2Int next = gridPos + dir;
            if (map.canMoveTo(next)) {
                currentDir = dir;
                gridPos = next;
                targetWorldPos = map.cellToWorld(gridPos);
                updateFacing(dir);
                return;
            }
        }
    }

    private void shuffle(Vector2Int[] arr)
    {
        for (int i = arr.Length - 1; i > 0; i--) {
            int j = Random.Range(0, i + 1);
            (arr[i], arr[j]) = (arr[j], arr[i]);
        }
    }

    private void updateFacing(Vector2Int dir)
    {
        if (dir == Vector2Int.right)     spinDirection = -1f;
        else if (dir == Vector2Int.left) spinDirection = 1f;
    }

    public override void tick() { }

    private void checkPlayerContact()
    {
        if (player == null) {
            player = FindAnyObjectByType<Player>();
            if (player == null) return;
        }

        if (gridPos == player.getGridPos()) {
            player.applyStunEffect(GameManager.HAZARD_STUN_DURATION);
        }
    }

    protected override Vector2Int startPosition()
    {
        if (spawnPos.x >= 0) return spawnPos;
        if (map == null) return Vector2Int.zero;
        return new Vector2Int(Random.Range(1, map.width - 1), Random.Range(0, map.height));
    }
}
