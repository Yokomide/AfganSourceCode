using System.ComponentModel.Composition.Primitives;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.AI;

public class PickupWithGun : EnemyController
{
    [SerializeField] private float _speedMultiplier = 0.8f;
    [SerializeField] private float _sideOffsetDistance = 5f;  
    [SerializeField] private float _minSafeDistance = 3f;     
    [SerializeField] private Transform _turret;              

    private PickupState _currentState = PickupState.Idle;      
    private Vector3 _initialPosition;                       
    private Vector3 _sideOffset;                              
    private int _targetWaypointIndex = 0;                   
    private bool _sideChosen = false;                   
    private float _minSafeDistanceSqr;                    

    #region Инициализация

    protected override void Awake()
    {
        base.Awake();
        _initialPosition = transform.position;                 
        _navMeshAgent.isStopped = true;                     
        _minSafeDistanceSqr = _minSafeDistance * _minSafeDistance;
    }

    protected override void Activate()
    {
        base.Activate();                                
        
        Observable.EveryUpdate()
            .Where(_ => _currentState == PickupState.Moving && _selectedTarget != null)
            .Subscribe(_ =>
            {
                MoveParallelToConvoy();
                RotateToTarget();                             
            })
            .AddTo(_disposables);
        
        Observable.Interval(System.TimeSpan.FromSeconds(_model.AttackCooldown))
            .Where(_ => _currentState == PickupState.Moving && _selectedTarget != null)
            .Subscribe(_ => AttackTarget())
            .AddTo(_disposables);
    }

    #endregion

    #region Логика поведения

    protected override void CheckConvoyTargets()
    {
        var activeUnits = _convoySystem.Convoy.Where(u => u != null).ToList();
        var targetsInRange = activeUnits
            .Where(unit => Vector3.Distance(transform.position, unit.transform.position) <= _model.AttackRadius)
            .ToList();

        if (targetsInRange.Count > 0)
        {
            if (_currentState == PickupState.Idle)
            {
                _currentState = PickupState.Moving;
                _navMeshAgent.isStopped = false;             
                StartMoving(activeUnits.First());            
            }

            if (_selectedTarget == null || !targetsInRange.Contains(_selectedTarget))
            {
                SelectTarget(targetsInRange);                 
            }
        }
        else if (_currentState == PickupState.Moving)
        {
            ResetToIdle();                                 
        }
    }

    private void StartMoving(UnitController leader)
    {
        if (leader == null) return;

    
        Vector3 leaderDirection = leader.transform.forward;
        Vector3 toPickup = transform.position - leader.transform.position;
        float side = Vector3.Dot(Vector3.Cross(leaderDirection, toPickup), Vector3.up);
        _sideOffset = (side > 0
            ? -Vector3.Cross(leaderDirection, Vector3.up)
            : Vector3.Cross(leaderDirection, Vector3.up)).normalized * _sideOffsetDistance;
        _sideChosen = true;
        
        _targetWaypointIndex = Mathf.Min(leader.GetCurrentWaypointIndex() + 1, _convoySystem.Waypoints.Count - 1);
        _navMeshAgent.speed = _convoySystem.ConvoySpeed.Value * _speedMultiplier;
        MoveToOffsetWaypoint(_targetWaypointIndex);
    }

    private void MoveParallelToConvoy()
    {
        if (_selectedTarget == null) return;

        var activeUnits = _convoySystem.Convoy.Where(u => u != null).ToList();
        if (activeUnits.Count == 0) return;

        var leader = activeUnits.First();
        int leaderWaypointIndex = leader.GetCurrentWaypointIndex();
        
        if (_targetWaypointIndex <= leaderWaypointIndex)
        {
            _targetWaypointIndex = Mathf.Min(leaderWaypointIndex + 1, _convoySystem.Waypoints.Count - 1);
            MoveToOffsetWaypoint(_targetWaypointIndex);
        }
        
        if (_navMeshAgent.remainingDistance <= _navMeshAgent.stoppingDistance && !_navMeshAgent.pathPending)
        {
            if (_targetWaypointIndex < _convoySystem.Waypoints.Count - 1)
            {
                _targetWaypointIndex++;
                MoveToOffsetWaypoint(_targetWaypointIndex);
            }
            else
            {
                _navMeshAgent.isStopped = true;
            }
        }
        
        if ((transform.position - _selectedTarget.transform.position).sqrMagnitude < _minSafeDistanceSqr)
        {
            Vector3 adjustedPosition = _navMeshAgent.destination + _sideOffset;
            _navMeshAgent.SetDestination(adjustedPosition);
        }
    }

    private void MoveToOffsetWaypoint(int waypointIndex)
    {
        if (waypointIndex >= _convoySystem.Waypoints.Count) return;

        Vector3 waypoint = _convoySystem.Waypoints[waypointIndex];
        Vector3 offsetWaypoint = waypoint + _sideOffset;
        _navMeshAgent.SetDestination(offsetWaypoint);
    }

    private void ResetToIdle()
    {
        _selectedTarget = null;
        _selectedTargetHitTransform = null;
        _currentState = PickupState.Idle;
        _navMeshAgent.isStopped = true;
        _sideChosen = false;
        _targetWaypointIndex = 0;
        transform.position = Vector3.MoveTowards(transform.position, _initialPosition, _navMeshAgent.speed * Time.deltaTime);
    }

    protected override void RotateToTarget()
    {
        if (_selectedTarget == null || _selectedTargetHitTransform == null || _turret == null) return;

        Vector3 direction = (_selectedTargetHitTransform.position - _turret.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        
        if (Quaternion.Angle(_turret.rotation, targetRotation) > 1f)
        {
            _turret.rotation = Quaternion.Slerp(_turret.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
        }
    }

    #endregion

    #region Очистка

    protected override void Die()
    {
        _signalBus.Fire(new EnemyDestroyedSignal(EnemyType.PickupWithGun));
        base.Die();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }

    #endregion
}

public enum PickupState
{
    Idle,
    Moving,
    Shooting
}