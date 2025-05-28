using System;
using System.Collections.Generic;
using DG.Tweening;
using UniRx;
using UnityEngine;
using UnityEngine.AI;
using Zenject;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public class UnitController : MonoBehaviour, IDamageable, ITarget
{
    [SerializeField] private UnitView _unitView;
    [SerializeField] private ProjectileType _projectileType;
    [SerializeField] private Transform _aimPoint;
    [SerializeField] private Transform _firePoint;
    [SerializeField] private Transform _firePointHolder;
    [SerializeField] private float spreadAngle = 5f;
    [Inject] private ProjectileFactory _projectileFactory;

    public UnitModel _model;
    public UnitModel Model => _model;
    public event Action<UnitController> OnDestroyed;
    public event Action<UnitController> OnParked;

    private Collider _collider;
    private NavMeshAgent _navMeshAgent;

    [Inject] private SignalBus _signalBus;
    private Subject<Vector3> _shootSubject = new Subject<Vector3>();
    private IDisposable _shootingDisposable;
    private readonly CompositeDisposable _disposables = new();
    private readonly CompositeDisposable _movementDisposables = new();
    private Tweener _rotationTweener;

    private UnitController _leader;
    private int _currentWaypointIndex = 0;
    private List<Vector3> _waypoints;
    private float _followDistance = 5f;
    private float _convoySpeed;
    private float _baseSpeed;
    private bool _isLeader;
    private bool _hasStartedMoving;
    private bool _moveToPark;
    private bool _parked;
    private float _startDelay;

    public bool IsLeader => _isLeader;
    private void Awake()
    {
        _collider = GetComponent<Collider>();
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _navMeshAgent.enabled = false;
    }

    public void Initialize(UnitModel model)
    {
        _model = model;
        _navMeshAgent.speed = _model.Speed;

        _unitView.InitializeHealthBar(_model.MaxHealth);
        
        _unitView.UpdateHealthBar(_model.Health.Value);
        transform.SetParent(GameObject.Find("ConvoyRoot").transform);
        if (_firePoint == null)
            _firePoint = transform;

        _shootSubject
            .ThrottleFirst(System.TimeSpan.FromSeconds(_model.FireRate))
            .Subscribe(Shoot)
            .AddTo(_disposables);

        Model.Health.Subscribe(hp =>
        {
            _unitView.UpdateHealthBar(hp);
            if (hp <= 0) Die();
        }).AddTo(_disposables);
    }

    public void SetColliderStatus(bool value)
    {
        _collider.enabled = value;
        _navMeshAgent.enabled = value;
    }

    public void SetConvoyParameters(List<Vector3> waypoints, float convoySpeed, UnitController leader = null,
        int convoyIndex = 0, int inheritedWaypointIndex = 0)
    {
        if (_navMeshAgent.enabled == false)
            _navMeshAgent.enabled = true;
        _waypoints = waypoints;
        _currentWaypointIndex = inheritedWaypointIndex; 
        _baseSpeed = convoySpeed;
        _navMeshAgent.speed = _baseSpeed;
        _leader = leader;
        _isLeader = _leader == null;
        _hasStartedMoving = false;
        _startDelay = convoyIndex * 0.2f;

        if (_waypoints == null || _waypoints.Count == 0)
        {
            Debug.LogWarning($"{name} has no waypoints assigned!");
            return;
        }
        
        _movementDisposables.Clear();
        
        Observable.EveryUpdate()
            .Where(_=> !_moveToPark && !_parked)
            .Subscribe(_ => UpdateMovement())
            .AddTo(_movementDisposables);

        if (_isLeader)
        {
            Observable.Timer(System.TimeSpan.FromSeconds(_startDelay))
                .Subscribe(_ =>
                {
                    _hasStartedMoving = true;
                    MoveToWaypoint(_currentWaypointIndex);
                })
                .AddTo(_movementDisposables);
        }
        else
        {
            Observable.Timer(System.TimeSpan.FromSeconds(_startDelay))
                .Subscribe(_ =>
                {
                    _hasStartedMoving = true;
                    AdjustPositionToLeader();
                })
                .AddTo(_movementDisposables);
        }
    }

    private void UpdateMovement()
    {
        if (!_hasStartedMoving || _currentWaypointIndex >= _waypoints.Count && !_moveToPark) return;

        if (_isLeader)
        {
            if (HasReachedDestination())
            {
                _currentWaypointIndex++;
                if (_currentWaypointIndex < _waypoints.Count)
                {
                    MoveToWaypoint(_currentWaypointIndex);
                }
                else
                {
                    _navMeshAgent.isStopped = true;
                }
            }
        }
        else if (_leader != null)
        {
            AdjustPositionToLeader();
        }
    }

    private void AdjustPositionToLeader()
    {
        if (_leader == null) return;

        float distanceToLeader = Vector3.Distance(transform.position, _leader.transform.position);
        Vector3 targetPosition;

        // Если лидер далеко, догоняем его напрямую
        if (distanceToLeader > _followDistance + 2f)
        {
            _navMeshAgent.speed = _baseSpeed * 1.5f;
            targetPosition = _leader.transform.position - (_leader.transform.forward * _followDistance);
        }
        // Если слишком близко, замедляемся и держим дистанцию
        else if (distanceToLeader < _followDistance - 1f)
        {
            _navMeshAgent.speed = _baseSpeed * 0.5f;
            targetPosition = _leader.transform.position - (_leader.transform.forward * _followDistance);
        }
        // Нормальное следование
        else
        {
            _navMeshAgent.speed = _baseSpeed;
            targetPosition = _leader.transform.position - (_leader.transform.forward * _followDistance);
        }

        _navMeshAgent.SetDestination(targetPosition);
        // Синхронизируем waypoint с лидером
        if (_leader.HasReachedDestination() && _currentWaypointIndex < _leader._currentWaypointIndex)
        {
            _currentWaypointIndex = _leader._currentWaypointIndex;
        }
    }

    private void MoveToWaypoint(int waypointIndex)
    {
        if (waypointIndex < _waypoints.Count)
        {
            _navMeshAgent.isStopped = false;
            _navMeshAgent.SetDestination(_waypoints[waypointIndex]);
        }
    }

    public void MoveToParkPosition(Vector3 parkPosition)
    {
        _isLeader = false;
        _moveToPark = true;
        _navMeshAgent.isStopped = false;
        _navMeshAgent.SetDestination(parkPosition);
        Debug.Log("Unit set park: " + this.name);
        Observable.EveryUpdate()
            .Where(_ => _moveToPark && HasReachedDestination() && !_parked)
            .Subscribe(_ => SetParked(true))
            .AddTo(_disposables);
    }

    private void SetParked(bool value)
    {
        _navMeshAgent.isStopped = true;
        _parked = value;
        if(value)
            OnParked?.Invoke(this);
    }
    public bool HasReachedDestination(float stoppingDistance = 1f)
    {
        return !_navMeshAgent.pathPending && _navMeshAgent.remainingDistance <= stoppingDistance;
    }

    public bool HasReachedFinalWaypoint()
    {
        return _currentWaypointIndex >= _waypoints.Count;
    }

    public int GetCurrentWaypointIndex()
    {
        return _currentWaypointIndex;
    }

    public void TryShoot(Vector3 targetPosition)
    {
        if (Model.Class != UnitClass.Combat) return;

        Vector3 direction = (targetPosition - _firePointHolder.position).normalized;
        RotateTurret(direction);
        _shootSubject.OnNext(targetPosition);
    }

    private void RotateTurret(Vector3 direction)
    {
        _rotationTweener?.Kill();

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        Vector3 eulerRotation = targetRotation.eulerAngles;
        Vector3 currentEuler = _firePointHolder.rotation.eulerAngles;
        eulerRotation.x = currentEuler.x;
        eulerRotation.z = currentEuler.z;

        _rotationTweener = _firePointHolder.DORotate(eulerRotation, 0.1f)
            .SetEase(Ease.Linear);
    }

    public void Shoot(Vector3 targetPosition)
    {
        Vector3 baseDirection = (targetPosition - _firePoint.position).normalized;
        
        Vector3 spread = Random.insideUnitSphere * Mathf.Tan(spreadAngle * Mathf.Deg2Rad); 
        Vector3 directionWithSpread = (baseDirection + spread).normalized;
        
        var projectile = _projectileFactory.Create(_projectileType, directionWithSpread, Model.Damage);
        projectile.transform.position = _firePoint.position;
        
        _unitView.PlayShootEffect();
    }

    public void Stop()
    {
        StopShooting();
        StopMovement();
        _disposables?.Dispose();

    }
    public void StopShooting()
    {
        _shootingDisposable?.Dispose();
    }

    public void StopMovement()
    {
        _navMeshAgent.isStopped = true;
        _movementDisposables?.Dispose();
    }
    private void OnDestroy()
    {
        _disposables.Dispose();
        _movementDisposables.Dispose();
    }

    public void TakeDamage(int value)
    {
        _model.TakeDamage(value);
    }

    public void Die()
    {
        OnDestroyed?.Invoke(this);
        if (_model.Durability.Value < 0.5f)
            _model.Durability.Value = 0.5f;
        _signalBus.Fire(new UnitDestroyedSignal(_model));
        Destroy(gameObject);
    }

    public Transform GetTargetTransform()
    {
        return _aimPoint;
    }
}