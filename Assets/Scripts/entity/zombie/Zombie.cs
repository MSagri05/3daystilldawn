using UnityEngine;

// Basic zombie enemy. Stands idle until the player enters its view cone (range +
// angle + unobstructed line of sight), then chases. Also listens on the Noise bus:
// a sound within its radius sends the zombie shambling to the noise point to
// investigate (sight/chase always takes priority). No navmesh: it walks straight
// toward its goal and slides along walls via the CharacterController. Keeps
// chasing for a short memory window after losing sight.
[RequireComponent(typeof(CharacterController))]
public class Zombie : MonoBehaviour
{
    static readonly int SpeedParam  = Animator.StringToHash("Speed");
    static readonly int AttackParam = Animator.StringToHash("Attack");

    CharacterController controller;
    Animator animator;   // on the model child; optional — AI runs fine without one
    Transform target;
    Health targetHealth;
    Vector3 velocity;
    float chaseMemory;
    float attackCooldown;

    // noise investigation
    bool hasNoise;
    Vector3 noisePosition;
    float lingerTimer;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator   = GetComponentInChildren<Animator>();
        lastPosition = transform.position;
    }

    void OnEnable()
    {
        Noise.onNoise += onNoise;
    }

    void OnDisable()
    {
        Noise.onNoise -= onNoise;   // static event — never leave a destroyed listener behind
    }

    void Start()
    {
        PlayerController player = FindAnyObjectByType<PlayerController>();
        if (player != null)
        {
            target = player.transform;
            targetHealth = player.GetComponent<Health>();
        }
    }

    void Update()
    {
        if (attackCooldown > 0f) attackCooldown -= Time.deltaTime;

        if (target != null)
        {
            if (canSeeTarget())
                chaseMemory = GameManager.ZOMBIE_SIGHT_MEMORY;
            else if (chaseMemory > 0f)
                chaseMemory -= Time.deltaTime;

            if (chaseMemory > 0f)
            {
                chase();
                tryAttack();
            }
            else
            {
                investigate();
            }
        }

        applyGravity();
        updateAnimator();
    }

    // Feed the state machine the zombie's real horizontal speed (so Idle/Walk/Chase
    // pick themselves from the same thresholds the AI moves at), lightly damped so
    // transitions don't flicker at the boundaries. Speed is measured from actual
    // frame displacement — controller.velocity only reflects the LAST Move() call,
    // and the trailing gravity Move zeroes its horizontal part every frame.
    Vector3 lastPosition;

    void updateAnimator()
    {
        if (animator == null) return;

        Vector3 delta = transform.position - lastPosition;
        delta.y = 0f;
        lastPosition = transform.position;

        float speed = Time.deltaTime > 0f ? delta.magnitude / Time.deltaTime : 0f;
        animator.SetFloat(SpeedParam, speed, 0.1f, Time.deltaTime);
    }

    // A sound reached us if we're inside its carry radius. The newest audible noise
    // wins; an active chase is never interrupted (checked in Update's priority).
    void onNoise(Vector3 position, float radius)
    {
        if ((position - transform.position).sqrMagnitude > radius * radius) return;

        hasNoise = true;
        noisePosition = position;
        lingerTimer = GameManager.ZOMBIE_INVESTIGATE_LINGER;
    }

    // Shamble to the last heard noise, look around briefly, then go back to idling.
    void investigate()
    {
        if (!hasNoise) return;

        Vector3 flat = noisePosition - transform.position;
        flat.y = 0f;

        if (flat.magnitude > 1f)
        {
            moveToward(noisePosition, GameManager.ZOMBIE_INVESTIGATE_SPEED);
            return;
        }

        lingerTimer -= Time.deltaTime;
        if (lingerTimer <= 0f) hasNoise = false;
    }

    // Bite the player whenever it stays within reach, throttled by a cooldown.
    void tryAttack()
    {
        if (targetHealth == null || targetHealth.IsDead || attackCooldown > 0f) return;

        Vector3 flat = target.position - transform.position;
        flat.y = 0f;
        if (flat.magnitude > GameManager.ZOMBIE_ATTACK_RANGE) return;

        targetHealth.damage(GameManager.ZOMBIE_ATTACK_DAMAGE);
        PlayerCondition.wound(GameManager.ZOMBIE_WOUND_MAX_HP);   // bites leave lasting damage
        attackCooldown = GameManager.ZOMBIE_ATTACK_COOLDOWN;
        animator?.SetTrigger(AttackParam);
    }

    bool canSeeTarget()
    {
        Vector3 eye         = transform.position + Vector3.up * GameManager.ZOMBIE_EYE_HEIGHT;
        Vector3 targetPoint = target.position + Vector3.up;          // aim at the player's torso
        Vector3 toTarget    = targetPoint - eye;
        float   distance    = toTarget.magnitude;

        if (distance > GameManager.ZOMBIE_SIGHT_RANGE) return false;

        // anything solid between us and the player blocks awareness (ignore our own body)
        if (Physics.Raycast(eye, toTarget.normalized, out RaycastHit hit, distance))
        {
            bool isTarget = hit.transform == target || hit.transform.IsChildOf(target);
            bool isSelf   = hit.transform == transform || hit.transform.IsChildOf(transform);
            if (!isTarget && !isSelf) return false;
        }

        // close by: hears/smells the player from any direction, so the view cone doesn't matter
        if (distance <= GameManager.ZOMBIE_HEARING_RANGE) return true;

        // farther away: must be inside the forward view cone
        return Vector3.Angle(transform.forward, toTarget) <= GameManager.ZOMBIE_FOV * 0.5f;
    }

    void chase()
    {
        moveToward(target.position, GameManager.ZOMBIE_MOVE_SPEED);
    }

    // Straight-line steering shared by chasing and investigating.
    void moveToward(Vector3 point, float speed)
    {
        Vector3 direction = point - transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.0001f) return;

        direction.Normalize();
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.LookRotation(direction),
            Time.deltaTime * GameManager.ZOMBIE_TURN_SPEED);

        controller.Move(direction * speed * Time.deltaTime);
    }

    void applyGravity()
    {
        if (controller.isGrounded && velocity.y < 0f)
            velocity.y = -2f;
        velocity.y += GameManager.PLAYER_GRAVITY * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}
