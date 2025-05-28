using UnityEngine;

[CreateAssetMenu(fileName = "EnemyConfig", menuName = "Enemies/Enemy Config", order = 1)]
public class EnemyConfig : ScriptableObject
{
    [SerializeField] private EnemyType _enemyType;      
    [SerializeField] private int _maxHealth;           
    [SerializeField] private float _speed;               
    [SerializeField] private int _damage;     
    [SerializeField] private float _attackRadius;        
    [SerializeField] private float _attackCooldown;     
    [SerializeField] private ProjectileType _projectileType;

    public EnemyType EnemyType => _enemyType;
    public int MaxHealth => _maxHealth;
    public float Speed => _speed;
    public int Damage=> _damage;
    public float AttackRadius => _attackRadius;
    public float AttackCooldown => _attackCooldown;
    public ProjectileType ProjectileType => _projectileType;
}