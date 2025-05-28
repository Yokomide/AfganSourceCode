using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.AI;

public class InfantryEnemy : EnemyController
{
    [SerializeField] private float _coverSearchRadius = 10f;
    [SerializeField] private float _shootInterval = 5f;   
    [SerializeField] private float _shootProbability = 0.7f;
    [SerializeField] private float _shootDuration = 2f;    
    [SerializeField] private float _moveProbability = 0.3f; 
    [SerializeField] private float _moveInterval = 10f; 
    [SerializeField] private float _moveWithoutTargetProbability = 0.5f; 
    [SerializeField] private float _hidingHeightMultiplier = 0.5f; 


    [SerializeField] private bool _turnOffNavMesh;
    private List<CoverPoint> _coverPoints;
    private CoverPoint _currentCover;
    private InfantryState _currentState = InfantryState.Hiding;
    private bool _hasArrived = false; 
    
    private CapsuleCollider _collider;
    private float _originalHeight;
    private Vector3 _originalCenter;

    #region Инициализация

    protected override void Awake()
    {
        base.Awake();
        if (_turnOffNavMesh)
            _navMeshAgent.enabled = false;
        _coverPoints = GameObject.FindGameObjectsWithTag("Cover")
            .Select(go => go.GetComponent<CoverPoint>())
            .Where(cp => cp != null)
            .ToList();
        
        _collider = GetComponent<CapsuleCollider>();
        if (_collider == null)
        {
            Debug.LogError($"{gameObject.name} has no CapsuleCollider attached!");
            return;
        }
        
        _originalHeight = _collider.height;
        _originalCenter = _collider.center;
    }

    protected override void Activate()
    {
        base.Activate();
        MoveToCover();

        Observable.Interval(System.TimeSpan.FromSeconds(_moveInterval))
            .Where(_ => _currentState == InfantryState.Hiding && _selectedTarget == null && Random.value <= _moveWithoutTargetProbability)
            .Subscribe(_ => MoveToCover())
            .AddTo(_disposables);

        Observable.EveryUpdate()
            .Where(_ => _currentState == InfantryState.Running && 
                        !_hasArrived && 
                        _navMeshAgent.pathPending == false && 
                        !_turnOffNavMesh &&
                        _navMeshAgent.remainingDistance <= _navMeshAgent.stoppingDistance)
            .Subscribe(_ => ArriveAtCover())
            .AddTo(_disposables);

        Observable.Interval(System.TimeSpan.FromSeconds(_shootInterval))
            .Where(_ => _currentState == InfantryState.Hiding && Random.value <= _shootProbability)
            .Subscribe(_ => StartShooting())
            .AddTo(_disposables);
    }

    #endregion

    #region Логика поведения

    private void MoveToCover()
    {
        FreeCurrentCover();
        var cover = FindRandomFreeCover(_coverSearchRadius);
        if (cover != null)
        {
            cover.Occupy();
            _currentCover = cover;
            _navMeshAgent.SetDestination(cover.transform.position);
            SetAnimation(true, false, false);
            _hasArrived = false; 
            _currentState = InfantryState.Running;
        }
        else
        {
            SetAnimation(false, true, false);
            _currentState = InfantryState.Hiding;
        }
    }

    private void ArriveAtCover()
    {
        _hasArrived = true; 
        _currentState = InfantryState.Hiding;
        SetAnimation(false, true, false);
    }

    private void StartShooting()
    {
        if (_selectedTarget == null)
        {
            _currentState = InfantryState.Hiding;
            SetAnimation(false, true, false);
            return;
        }

        _currentState = InfantryState.Shooting;
        SetAnimation(false, false, true);

        var shootSubscription = Observable.Interval(System.TimeSpan.FromSeconds(_model.AttackCooldown))
            .TakeUntil(Observable.Timer(System.TimeSpan.FromSeconds(_shootDuration)))
            .Where(_ => _selectedTarget != null)
            .Subscribe(_ => AttackTarget())
            .AddTo(_disposables);

        Observable.Timer(System.TimeSpan.FromSeconds(_shootDuration))
            .Subscribe(_ =>
            {
                shootSubscription.Dispose();
                DecideNextAction();
            })
            .AddTo(_disposables);
    }

    private void DecideNextAction()
    {
        if (Random.value <= _moveProbability)
        {
            MoveToCover();
        }
        else
        {
            _currentState = InfantryState.Hiding;
            SetAnimation(false, true, false);
        }
    }

    #endregion

    #region Вспомогательные методы

    private CoverPoint FindRandomFreeCover(float searchRadius)
    {
        var availableCovers = _coverPoints
            .Where(cp => !cp.IsOccupied && 
                         cp != _currentCover && 
                         Vector3.Distance(transform.position, cp.transform.position) <= searchRadius)
            .ToList();

        if (availableCovers.Count > 0)
        {
            int randomIndex = Random.Range(0, availableCovers.Count);
            return availableCovers[randomIndex];
        }
        return null;
    }

    private void FreeCurrentCover()
    {
        if (_currentCover != null)
        {
            _currentCover.Free();
            _currentCover = null;
        }
    }

    private void SetAnimation(bool isRunning, bool isHiding, bool isShooting)
    {
        _animator.SetBool("IsRunning", isRunning);
        _animator.SetBool("IsHiding", isHiding);
        _animator.SetBool("IsShooting", isShooting);
        
        if (isHiding)
        {
            _collider.height = _originalHeight * _hidingHeightMultiplier;
            _collider.center = new Vector3(_originalCenter.x, _originalCenter.y * _hidingHeightMultiplier, _originalCenter.z);
        }
        else
        {
            _collider.height = _originalHeight;
            _collider.center = _originalCenter;
        }
    }

    #endregion

    #region Очистка

    protected override void Die()
    {
        FreeCurrentCover();
        _signalBus.Fire(new EnemyDestroyedSignal(EnemyType.Infantry));

        base.Die();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        FreeCurrentCover();
    }

    #endregion
}

public enum InfantryState
{
    Hiding,
    Running, 
    Shooting 
}