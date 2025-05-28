using UniRx;

public class EnemiesDestroyed : IVictoryCondition
{
    private readonly int _requiredDestroyed;
    private readonly ReactiveProperty<int> _enemiesDestroyed;

    public EnemiesDestroyed(int requiredDestroyed, ReactiveProperty<int> enemiesDestroyed)
    {
        _requiredDestroyed = requiredDestroyed;
        _enemiesDestroyed = enemiesDestroyed;
    }

    public bool IsConditionMet()
    {
        return _enemiesDestroyed.Value >= _requiredDestroyed;
    }
}