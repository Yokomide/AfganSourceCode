using System;
using UniRx;
using Zenject;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using Object = UnityEngine.Object;

public class ConvoySystem : IInitializable, IDisposable
{
    private readonly UnitFactory _unitFactory;
    private readonly SignalBus _signalBus;
    private readonly List<UnitController> _convoy = new();
    private readonly CompositeDisposable _disposables = new();
    private readonly Dictionary<string, UnitConfig> _unitConfigs;

    [Inject] private HitIndicator _hitIndicator;
    [Inject] private Crosshair _crosshair;
    [SerializeField] private List<Vector3> _waypoints; // Список точек пути
    public List<Vector3> Waypoints => _waypoints;
    private int _currentWaypointIndex = 0;

    public IReadOnlyList<UnitController> Convoy => _convoy.AsReadOnly();
    public IReadOnlyReactiveProperty<float> ConvoySpeed => _convoySpeed;
    public IReadOnlyReactiveProperty<int> ConvoyDamage => _convoyDamage;
    public IReadOnlyReactiveProperty<int> ConvoyCargo => _convoyCargo;

    private readonly ReactiveProperty<float> _convoySpeed = new();
    private readonly ReactiveProperty<int> _convoyDamage = new();
    private readonly ReactiveProperty<int> _convoyCargo = new();

    private Transform _convoyRoot;
    private SlotTrigger[] _slots;
    private ConvoyPlacer _convoyPlacer;
    private Camera _mainCamera;
    private UnitPark _unitPark;

    private Vector3 _lastUnitPosition;
    private Dictionary<UnitController, Vector3> _parkingPositions = new();
    private List<UnitController> _parkedUnits = new();
    private List<UnitController> _parkingUnits = new();
    private List<GameObject> _parkingMarkers = new();

    private bool _isMissionStarted;
    private bool _isMissionEnded;

    public ConvoySystem(UnitFactory unitFactory, SignalBus signalBus, ConvoyPlacer convoyPlacer,
        UnitConfig[] unitConfigs, UnitPark unitPark, HitIndicator hitIndicator)
    {
        _unitFactory = unitFactory;
        _signalBus = signalBus;
        _convoyPlacer = convoyPlacer;
        _unitPark = unitPark;
        _hitIndicator = hitIndicator;
    }

    public void Initialize()
    {
        _signalBus.Subscribe<AddUnitToConvoySignal>(AddUnit);
        _signalBus.Subscribe<StartMissionSignal>(StartConvoy);
        _signalBus.Subscribe<MissionEndedSignal>(StopConvoy);

        _mainCamera = Camera.main;
        _convoyRoot = GameObject.Find("ConvoyRoot").transform;

        _slots = _convoyPlacer.GetSlots();
        _convoyPlacer.Slots.ObserveCountChanged().Subscribe(_ => UpdateSlotsCount()).AddTo(_disposables);

        for (int i = 0; i < _slots.Length; i++)
        {
            _convoy.Add(null);
        }
    }

    private void UpdateSlotsCount()
    {
        _slots = _convoyPlacer.GetSlots();
        while (_convoy.Count < _slots.Length)
        {
            _convoy.Add(null);
        }

        while (_convoy.Count > _slots.Length)
        {
            var unit = _convoy[_convoy.Count - 1];
            if (unit != null) RemoveUnit(_convoy.Count - 1);
            else _convoy.RemoveAt(_convoy.Count - 1);
        }
    }

    public void AddUnit(AddUnitToConvoySignal signal)
    {
        if (signal.SlotIndex < 0 || signal.SlotIndex >= _convoy.Count)
        {
            if (signal.AddedUnitController != null)
                Object.Destroy(signal.AddedUnitController.gameObject);
            return;
        }

        if (_convoy[signal.SlotIndex] != null)
        {
            _unitPark.ReturnUnit(_convoy[signal.SlotIndex].Model);
            Object.Destroy(_convoy[signal.SlotIndex].gameObject);
            _convoy[signal.SlotIndex] = null;
        }

        _convoy[signal.SlotIndex] = signal.AddedUnitController;
        signal.AddedUnitController.transform.DOMove(_slots[signal.SlotIndex].PlacePoint.transform.position, 0.2f)
            .OnComplete(() => _slots[signal.SlotIndex].SetSlotOccupied(true));
        signal.AddedUnitController.transform.SetParent(_convoyRoot.transform);
        signal.AddedUnitController.OnDestroyed += HandleUnitDestroyed;

        UpdateConvoyStats();
    }

    public void AddUnitToEnd(UnitController unit)
    {
        _convoy.Add(unit);
        unit.OnDestroyed += HandleUnitDestroyed;
        var activeUnits = _convoy.Where(u => u != null && !_parkedUnits.Contains(u) && !_parkingUnits.Contains(u))
            .ToList();
        if (activeUnits.Count == 0) return;
        for (int i = 0; i < activeUnits.Count; i++)
        {
            if (activeUnits[i].IsLeader)
            {
                UpdateConvoyStructure(activeUnits[i].GetCurrentWaypointIndex());
            }
        }
    }

    public void RemoveUnit(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < _convoy.Count && _convoy[slotIndex] != null)
        {
            _convoy[slotIndex].OnDestroyed -= HandleUnitDestroyed;
            Object.Destroy(_convoy[slotIndex].gameObject);
            _convoy[slotIndex] = null;
            _slots[slotIndex].SetSlotOccupied(false);
            UpdateConvoyStats();
        }
    }

    private void HandleUnitDestroyed(UnitController destroyedUnit)
    {
        int destroyedIndex = _convoy.IndexOf(destroyedUnit);
        if (destroyedIndex != -1)
        {
            _convoy[destroyedIndex] = null;
            UpdateConvoyStats();
            if (destroyedUnit.IsLeader)
            {
                int inheritedWaypointIndex = destroyedUnit.GetCurrentWaypointIndex();
                UpdateConvoyStructure(inheritedWaypointIndex);
            }

            _unitPark.AddDestroyed(destroyedUnit.Model);
            Debug.Log("UnitDestroyed");
            if (_isMissionStarted && _convoy.All(u => u == null))
            {
                Debug.Log("No units");
                _lastUnitPosition = destroyedUnit.transform.position;
                _signalBus.Fire(new ConvoyDefeatedSignal("Все союзники погибли"));
            }
        }
    }

    private void UpdateConvoyStructure(int inheritedWaypointIndex)
    {
        var activeUnits = _convoy.Where(u => u != null && !_parkedUnits.Contains(u) && !_parkingUnits.Contains(u))
            .ToList();
        if (activeUnits.Count == 0) return;
        for (int i = 0; i < activeUnits.Count; i++)
        {
            UnitController leader = i > 0 ? activeUnits[i - 1] : null;
            int waypointIndex = i == 0 ? inheritedWaypointIndex : 0;
            activeUnits[i].SetConvoyParameters(_waypoints, _convoySpeed.Value, leader, i, waypointIndex);
        }
    }

    public void StartConvoy()
    {
        _isMissionStarted = true;

        for (int i = 0; i < _slots.Length; i++)
        {
            _slots[i].Deactivate();
        }

        for (int i = 0; i < _convoy.Count; i++)
        {
            if (_convoy[i] == null) continue;
            _convoy[i].SetColliderStatus(true);
        }

        UpdateConvoyStructure(0);

        Observable.EveryUpdate()
            .Where(_ => Input.GetMouseButton(0))
            .Subscribe(_ => ShootAtCursor())
            .AddTo(_disposables);

        Observable.EveryUpdate()
            .Where(_ => _isMissionStarted)
            .Subscribe(_ => HandleParking())
            .AddTo(_disposables);
    }

    private void StopConvoy()
    {
        _isMissionEnded = true;
        foreach (var unit in _convoy.Where(u => u != null))
        {
            unit.Stop(); // Предполагаемый метод остановки движения
            unit.transform.DOKill(); // Останавливаем все анимации DOTween
        }

        _disposables.Clear(); // Останавливаем все подписки на обновления
        Debug.Log("ConvoySystem: All units stopped.");
    }

    private void ShootAtCursor()
    {
        Vector2 mousePosition = Input.mousePosition;
        mousePosition.y += 120;
        mousePosition.x -= 120;
        // Смещаем позицию на 2 единицы вверх по Y
        //Vector2 adjustedPosition = new Vector2(mousePosition.x, mousePosition.y + 100f);

        // Используем скорректированную позицию для рейкаста
        Ray ray = _mainCamera.ScreenPointToRay(mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector3 targetPosition = hit.point;
            targetPosition.y -= 0.2f;
            foreach (var unit in _convoy.Where(u => u != null))
            {
                unit.TryShoot(targetPosition);
            }
        }

        _crosshair.transform.position = mousePosition;
    }

    private void HandleParking()
    {
        var activeUnits = _convoy.Where(u => u != null).ToList();
        if (activeUnits.Count == 0) return;

        foreach (var unit in activeUnits)
        {
            if (unit.HasReachedFinalWaypoint() && !_parkedUnits.Contains(unit) && !_parkingUnits.Contains(unit))
            {
                if (!_parkingPositions.ContainsKey(unit))
                {
                    AssignParkingPosition(unit);
                }

                MoveToParking(unit);

                // Если это лидер, обновляем структуру конвоя
                if (unit == activeUnits.First())
                {
                    UpdateConvoyStructure(unit.GetCurrentWaypointIndex());
                }
            }
        }
    }

    private void AssignParkingPosition(UnitController unit)
    {
        Vector3 finalWaypoint = _waypoints[_waypoints.Count - 1];
        int unitIndex = _convoy.IndexOf(unit);

        Vector3 convoyDirection = (_waypoints[_waypoints.Count - 1] - _waypoints[_waypoints.Count - 2]).normalized;
        Vector3 parkingLineDirection = Vector3.Cross(convoyDirection, Vector3.up).normalized;

        float spacing = 4f;
        int totalUnits = _convoy.Count(u => u != null);
        float lineLength = (totalUnits - 1) * spacing;
        Vector3 startOffset = parkingLineDirection * (-lineLength / 2f);

        Vector3 offset = startOffset + parkingLineDirection * (unitIndex * spacing);
        Vector3 parkingPosition =
            finalWaypoint + offset + convoyDirection * 4f; // Добавляем смещение на 2 единицы вперёд
        _parkingPositions[unit] = parkingPosition;
        // CreateParkingMarker(parkingPosition);
    }

    private void CreateParkingMarker(Vector3 position)
    {
        GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        marker.transform.position = position;
        marker.transform.localScale = Vector3.one * 0.5f;
        marker.GetComponent<Renderer>().material.color = Color.green;
        marker.name = "ParkingMarker";

        _parkingMarkers.Add(marker);
    }

    private void MoveToParking(UnitController unit)
    {
        Vector3 parkingPosition = _parkingPositions[unit];
        unit.MoveToParkPosition(parkingPosition);
        unit.OnParked += AddParkedUnit;
        _parkingUnits.Add(unit);
    }

    private void AddParkedUnit(UnitController unit)
    {
        _parkedUnits.Add(unit);
        _parkingUnits.Remove(unit);
        unit.OnParked -= AddParkedUnit;
        var activeUnits = _convoy.Where(u => u != null).ToList();
        if (activeUnits.All(unit => _parkedUnits.Contains(unit)))
        {
            _signalBus.Fire(new AllUnitsParkedSignal());
            Debug.Log("ConvoySystem: All units parked");
        }

        Debug.Log($"{unit.name} parked at {_parkingPositions[unit]}");
    }

    public float CalculatePassedDistance()
    {
        float passedDistance = 0f;
        for (int i = 0; i < _currentWaypointIndex; i++)
        {
            passedDistance += Vector3.Distance(Waypoints[i], Waypoints[i + 1]);
        }

        if (_currentWaypointIndex < Waypoints.Count)
        {
            passedDistance += Vector3.Distance(Waypoints[_currentWaypointIndex], GetLastUnitPosition());
        }

        return passedDistance / 100f;
    }

    public float CalculateTotalDistance()
    {
        float totalDistance = 0f;
        for (int i = 0; i < Waypoints.Count - 1; i++)
        {
            totalDistance += Vector3.Distance(Waypoints[i], Waypoints[i + 1]);
        }

        return totalDistance / 100f;
    }

    private void UpdateConvoyStats()
    {
        var activeUnits = _convoy.Where(u => u != null).ToList();
        _convoySpeed.Value = activeUnits.Any() ? activeUnits.Average(u => u.Model.Speed) : 0f;

        _convoyDamage.Value = activeUnits.Any() ? activeUnits.Sum(u => u.Model.Damage) : 0;

        _convoyCargo.Value = activeUnits.Any() ? activeUnits.Sum(u => u.Model.Crew) : 0;
    }

    public int GetCurrentWaypointIndex()
    {
        return _currentWaypointIndex;
    }

    public Vector3 GetLastUnitPosition()
    {
        return _lastUnitPosition;
    }

    private bool AllUnitsReachedFinalWaypoint()
    {
        var activeUnits = _convoy.Where(u => u != null).ToList();
        return activeUnits.Count > 0 && activeUnits.All(unit => unit.HasReachedFinalWaypoint());
    }

    public Vector3 GetConvoyDirection()
    {
        var activeUnits = Convoy.Where(u => u != null).ToList();
        if (activeUnits.Count == 0) return Vector3.forward;
        var leader = activeUnits.First();
        return (leader.GetTargetTransform().position - leader.transform.position).normalized;
    }

    public UnitClass[] GetUnitClasses()
    {
        UnitClass[] types = new UnitClass[_convoy.Count];
        for (int i = 0; i < _convoy.Count; i++)
        {
            types[i] = _convoy[i] != null ? _convoy[i].Model.Class : UnitClass.None;
        }

        return types;
    }

    public void SetWaypoints(List<Vector3> waypoints)
    {
        _waypoints = waypoints;
    }

    public void Dispose()
    {
        foreach (var unit in _convoy.Where(u => u != null))
        {
            unit.StopShooting();
            unit.OnDestroyed -= HandleUnitDestroyed;
            unit.OnParked -= AddParkedUnit; // Очистка подписки на парковку
        }

        _signalBus.Unsubscribe<AddUnitToConvoySignal>(AddUnit);
        _signalBus.Unsubscribe<StartMissionSignal>(StartConvoy);

        foreach (var marker in _parkingMarkers)
        {
            if (marker != null) Object.Destroy(marker);
        }

        _parkingMarkers.Clear();
        _disposables.Dispose();
    }
}