using UnityEngine;

using UniRx;

public class GrenadierEnemy : EnemyController
{
    protected override void Awake()
    {
        base.Awake();
        Observable.EveryUpdate()
            .Where(_ => _selectedTarget != null && Time.time >= _lastAttackTime + _model.AttackCooldown)
            .Subscribe(_ => AttackTarget())
            .AddTo(_disposables);
    }

    protected override void Die()
    {
        base.Die();
        _signalBus.Fire(new EnemyDestroyedSignal(EnemyType.Grenadier));
    }
    protected override void Activate()
    {
        base.Activate();
    }

    protected override void AttackTarget()
    {
        base.AttackTarget();
    }

}