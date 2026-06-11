public class SlowTrapTile : TrapTile
{
    private static readonly System.Random rng = new System.Random();

    public SlowTrapTile() : base("slow_trap", rng.NextDouble() < 0.5 ? GameManager.SLOW_TILE_SPRITE : GameManager.SLOW_TILE_SPRITE_ALT)
    {
    }

    public override void onStep(BaseEntity entity)
    {
        if (entity is Player player)
            player.applySlowEffect(GameManager.SLOW_TILE_DURATION);
    }
}
