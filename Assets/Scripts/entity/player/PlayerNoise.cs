using UnityEngine;

// Emits footstep noise while the player moves on the ground — the first sound
// source on the Noise bus. Three tiers: sprinting is loud, walking is quiet,
// crouching (and standing still) is silent — so speed trades off directly against
// attention. Reads state off PlayerController like CameraEffects does.
[RequireComponent(typeof(PlayerController))]
public class PlayerNoise : MonoBehaviour
{
    PlayerController player;
    float footstepTimer;

    void Awake()
    {
        player = GetComponent<PlayerController>();
    }

    void Update()
    {
        float radius = footstepRadius();
        if (radius <= 0f) {
            footstepTimer = 0f;   // next audible movement starts with an immediate step
            return;
        }

        footstepTimer -= Time.deltaTime;
        if (footstepTimer <= 0f) {
            Noise.emit(transform.position, radius);
            footstepTimer = GameManager.NOISE_FOOTSTEP_INTERVAL;
        }
    }

    // How far the current footsteps carry: sprint loud, walk quiet, crouch silent.
    float footstepRadius()
    {
        bool moving = player.IsGrounded && player.HorizontalSpeed > GameManager.HEADBOB_MIN_SPEED;
        if (!moving || player.IsCrouching) return 0f;
        return player.IsSprinting ? GameManager.NOISE_SPRINT_RADIUS
                                  : GameManager.NOISE_WALK_RADIUS;
    }
}
