using UnityEngine;

// Abstract base class for all enemies — extend this for each enemy type
public abstract class BaseEnemy : BaseEntity
{
    // Override to implement enemy-specific AI logic
    protected override void OnTick() { }
}
