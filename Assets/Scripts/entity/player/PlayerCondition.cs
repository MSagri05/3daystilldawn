using System;
using UnityEngine;

// The player's lasting physical condition: maximum HP (worn down by zombie wounds,
// restored with medicine) and stamina capacity (worn down by overnight hunger,
// restored by eating food). Static so it survives scene loads — Health/Stamina are
// per-scene components and would otherwise forget wounds at every door. The player
// binds these values to its components (PlayerController.Start / onChanged).
public static class PlayerCondition
{
    public static float MaxHealth  { get; private set; } = GameManager.PLAYER_MAX_HEALTH;
    public static float MaxStamina { get; private set; } = GameManager.STAMINA_START_MAX;

    public static event Action onChanged;

    // New game: unhurt, but already a little hungry (capacity below the full bar).
    public static void reset()
    {
        MaxHealth  = GameManager.PLAYER_MAX_HEALTH;
        MaxStamina = GameManager.STAMINA_START_MAX;
        onChanged?.Invoke();
    }

    // A zombie hit leaves a lasting wound.
    public static void wound(float amount) =>
        setMaxHealth(MaxHealth - amount);

    // Medicine mends wounds.
    public static void treat(float amount) =>
        setMaxHealth(MaxHealth + amount);

    // A night without enough food shrinks stamina capacity.
    public static void starve(float amount) =>
        setMaxStamina(MaxStamina - amount);

    // Eating restores stamina capacity.
    public static void eat(float amount) =>
        setMaxStamina(MaxStamina + amount);

    static void setMaxHealth(float value)
    {
        float clamped = Mathf.Clamp(value, GameManager.PLAYER_MIN_MAX_HEALTH, GameManager.PLAYER_MAX_HEALTH);
        if (Mathf.Approximately(clamped, MaxHealth)) return;
        MaxHealth = clamped;
        onChanged?.Invoke();
    }

    static void setMaxStamina(float value)
    {
        float clamped = Mathf.Clamp(value, GameManager.STAMINA_MIN_MAX, GameManager.STAMINA_MAX);
        if (Mathf.Approximately(clamped, MaxStamina)) return;
        MaxStamina = clamped;
        onChanged?.Invoke();
    }
}
