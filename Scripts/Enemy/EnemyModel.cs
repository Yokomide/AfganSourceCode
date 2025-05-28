public class EnemyModel
{
    public EnemyType EnemyType { get; private set; }
    public int MaxHealth { get; private set; }
    public float Speed { get; private set; }
    public int Damage{ get; private set; }
    public float AttackRadius { get; private set; }
    public float AttackCooldown { get; private set; }
    public ProjectileType ProjectileType { get; private set; }

    public EnemyModel(EnemyConfig config)
    {
        EnemyType = config.EnemyType;
        MaxHealth = config.MaxHealth;
        Speed = config.Speed;
        Damage = config.Damage;
        AttackRadius = config.AttackRadius;
        AttackCooldown = config.AttackCooldown;
        ProjectileType = config.ProjectileType;
    }
}