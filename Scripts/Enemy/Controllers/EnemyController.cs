using System.Collections.Generic;
using UnityEngine;
using Zenject;
using UniRx;
using System.Linq;
using UnityEngine.AI;

public abstract class EnemyController : MonoBehaviour, IDamageable
{
    [SerializeField] protected EnemyView _view;
    [SerializeField] protected Transform _firePoint;
    [SerializeField] protected EnemyConfig _config;
    [SerializeField] protected Animator _animator;
    [SerializeField] protected NavMeshAgent _navMeshAgent;
    [SerializeField] protected float _rotationSpeed = 5f; 
    
    protected EnemyModel _model;
    protected UnitController _selectedTarget;
    protected Transform _selectedTargetHitTransform;
    protected CompositeDisposable _disposables = new();
    
    [Inject] protected ConvoySystem _convoySystem;
    [Inject] protected ProjectileFactory _projectileFactory;
    [Inject] protected SignalBus _signalBus;
    
    protected int _currentHealth;
    protected float _lastAttackTime;

    protected virtual void Awake()
    {
        if (_config == null)
        {
            Debug.LogError($"{gameObject.name} Конфиг не назначен.");
            return;
        }

        _model = new EnemyModel(_config);
        _navMeshAgent.speed = _model.Speed;
        _currentHealth = _model.MaxHealth;
        _signalBus.Subscribe<StartMissionSignal>(Activate);
        _signalBus.Subscribe<MissionEndedSignal>(Deactivate);
    }

    protected virtual void Activate()
    {
        Observable.Interval(System.TimeSpan.FromSeconds(0.5f))
            .Subscribe(_ => CheckConvoyTargets())
            .AddTo(_disposables);

        Observable.EveryUpdate()
            .Where(_ => _selectedTargetHitTransform != null)
            .Subscribe(_ => RotateToTarget())
            .AddTo(_disposables);
    }
    protected virtual void Deactivate()
    {
        _disposables?.Dispose();
        if(_animator)
            _animator.StopPlayback();
        
    }
    protected virtual void CheckConvoyTargets()
    {
        var activeUnits = _convoySystem.Convoy.Where(u => u != null).ToList();
        var targetsInRange = activeUnits.Where(unit =>
            Vector3.Distance(transform.position, unit.transform.position) <= _model.AttackRadius).ToList();

        if (targetsInRange.Count > 0 && (_selectedTarget == null || !targetsInRange.Contains(_selectedTarget)))
        {
            SelectTarget(targetsInRange);
        }
        else if (_selectedTarget != null && !targetsInRange.Contains(_selectedTarget))
        {
            _selectedTarget = null;
            _selectedTargetHitTransform = null;
            if (targetsInRange.Count > 0)
            {
                SelectTarget(targetsInRange);
            }
        }
    }

    protected void SelectTarget(List<UnitController> targetsInRange)
    {
        float randomValue = Random.value;
        if ((randomValue <= 0.7f && targetsInRange[0] != null) || targetsInRange.Count == 1)
        {
            _selectedTarget = targetsInRange[0];
        }
        else
        {
            int randomIndex = Random.Range(1, targetsInRange.Count);
            _selectedTarget = targetsInRange[randomIndex];
        }

        _selectedTargetHitTransform = _selectedTarget.GetComponent<ITarget>().GetTargetTransform();
    }

    protected virtual void RotateToTarget()
    {
        Vector3 direction = (_selectedTargetHitTransform.position - transform.position).normalized;
        
        direction.y = 0;

        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
        }
    }

    protected virtual void AttackTarget()
    {
        if (_selectedTarget == null || _selectedTargetHitTransform == null) return;

        Vector3 direction = (_selectedTargetHitTransform.position - _firePoint.position).normalized;
        var projectile = _projectileFactory.Create(_model.ProjectileType, direction, _model.Damage);
        projectile.transform.position = _firePoint.position;

        _lastAttackTime = Time.time;
    }

    public void TakeDamage(int value)
    {
        if (_currentHealth <= 0) return;

        _currentHealth -= value;


        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        _view.PlayDieVFX();
        Destroy(gameObject);
    }

    protected virtual void OnDestroy()
    {
        _disposables.Dispose();
        _signalBus.Unsubscribe<StartMissionSignal>(Activate);
    }

    protected void OnDrawGizmosSelected()
    {
        if (_model != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _model.AttackRadius);
        }
    }
}