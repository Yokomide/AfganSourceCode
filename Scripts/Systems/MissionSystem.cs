using System;
using System.Linq;
using UniRx;
using Zenject;
using UnityEngine;

public class MissionSystem : IInitializable, IDisposable
{
    private readonly SignalBus _signalBus;
    private readonly ConvoySystem _convoySystem;
    private readonly CompositeDisposable _disposables = new();
    private readonly UnitPark _unitPark;
    public IReadOnlyReactiveProperty<bool> IsMissionActive => _isMissionActive;
    private readonly ReactiveProperty<bool> _isMissionActive = new(false);

    public IReadOnlyReactiveProperty<MissionResult> MissionResult => _missionResult;
    private readonly ReactiveProperty<MissionResult> _missionResult = new();

    private readonly ReactiveProperty<int> _savedLives = new(0);
    private readonly ReactiveProperty<int> _lostLives = new(0);
    private readonly ReactiveProperty<int> _lostUnits = new(0);
    private readonly ReactiveProperty<bool> _cargoDelivered = new(false);
    private readonly ReactiveProperty<int> _enemiesDestroyed = new(0);
    private readonly ReactiveProperty<int> _enemyVehiclesDestroyed = new(0);

    private IVictoryCondition _victoryCondition;
    private LevelConfig _currentLevelConfig;

    [Inject] private BonusSystem _bonusSystem; 
    
    public MissionSystem(SignalBus signalBus, ConvoySystem convoySystem, UnitPark unitPark)
    {
        _signalBus = signalBus;
        _convoySystem = convoySystem;
        _unitPark = unitPark;
    }

    public void Initialize()
    {
        _signalBus.Subscribe<StartMissionSignal>(StartMission);
        _signalBus.Subscribe<UnitDestroyedSignal>(OnUnitDestroyed);
        _signalBus.Subscribe<AllySavedSignal>(OnAllySaved);
        _signalBus.Subscribe<CargoDeliveredSignal>(OnCargoDelivered);
        _signalBus.Subscribe<ConvoyDefeatedSignal>(OnConvoyDefeated);
        _signalBus.Subscribe<EnemyDestroyedSignal>(OnEnemyDestroyed);
        _signalBus.Subscribe<AllUnitsParkedSignal>(CheckMissionCompletion);
    }

    public void SetLevelConfig(LevelConfig levelConfig)
    {
        _currentLevelConfig = levelConfig;
        SetupVictoryCondition();
    }

    private void SetupVictoryCondition()
    {
        Vector3 destination = _convoySystem.Waypoints.Last();
        switch (_currentLevelConfig.ConditionType)
        {
            case VictoryConditionType.AllUnitsReachedDestination:
                _victoryCondition = new AllUnitsReachedDestination(_convoySystem, destination);
                break;
            case VictoryConditionType.SpecificUnitReachedDestination:
                _victoryCondition =
                    new SpecificUnitReachedDestination(_convoySystem, _currentLevelConfig.SpecificUnitId, destination);
                break;
            case VictoryConditionType.MinimumUnitsSurvived:
                _victoryCondition = new MinimumUnitsSurvived(_convoySystem, _currentLevelConfig.MinUnits);
                break;
            case VictoryConditionType.EnemiesDestroyed:
                _victoryCondition =
                    new EnemiesDestroyed(_currentLevelConfig.RequiredEnemiesDestroyed, _enemiesDestroyed);
                break;
            default:
                Debug.LogError("Unknown victory condition type");
                break;
        }
    }

    private void StartMission(StartMissionSignal signal)
    {
        if (_currentLevelConfig == null)
        {
            Debug.LogError("LevelConfig not set. Call SetLevelConfig before starting mission.");
            return;
        }

        _isMissionActive.Value = true;
        _savedLives.Value = 0;
        _lostLives.Value = 0;
        _cargoDelivered.Value = false;
        _enemiesDestroyed.Value = 0;
        _enemyVehiclesDestroyed.Value = 0;

        Debug.Log("Mission Started");
    }

    private void CheckMissionCompletion()
    {
        if (_victoryCondition == null)
        {
            Debug.LogError("Victory condition not set.");
            return;
        }

        float totalDistancePassed = _convoySystem.CalculateTotalDistance();
        if (_victoryCondition.IsConditionMet())
        {
            EndMission(new MissionResult(true, _savedLives.Value, _lostLives.Value, _lostUnits.Value,
                _cargoDelivered.Value, _enemiesDestroyed.Value, _enemyVehiclesDestroyed.Value, totalDistancePassed, "Миссия выполнена успешно"));
        }
        else
        {
            EndMission(new MissionResult(false, _savedLives.Value, _lostLives.Value, _lostUnits.Value,
                _cargoDelivered.Value, _enemiesDestroyed.Value,_enemyVehiclesDestroyed.Value, totalDistancePassed, "Миссия провалена"));
            Debug.Log("Mission Failed: All units parked, but victory condition not met.");
        }
    }

    private void OnUnitDestroyed(UnitDestroyedSignal signal)
    {
        if (!_isMissionActive.Value) return;

        _lostUnits.Value++;
        _lostLives.Value += signal.Unit.Crew;
        
        switch (_currentLevelConfig.ConditionType)
        {
            case VictoryConditionType.SpecificUnitReachedDestination:
                var specificUnit =
                    _convoySystem.Convoy.FirstOrDefault(u => u?.Model.Id == _currentLevelConfig.SpecificUnitId);
                if (specificUnit == null) 
                {
                    float totalDistancePassed = _convoySystem.CalculateTotalDistance();
                    EndMission(new MissionResult(false, _savedLives.Value, _lostLives.Value, _lostUnits.Value,
                        _cargoDelivered.Value, _enemiesDestroyed.Value, _enemyVehiclesDestroyed.Value, totalDistancePassed,
                        "Необходимая единица уничтожена"));
                    return;
                }

                break;

            case VictoryConditionType.MinimumUnitsSurvived:
                int remainingUnits = _convoySystem.Convoy.Count(u => u != null);
                if (remainingUnits < _currentLevelConfig.MinUnits)
                {
                    float totalDistancePassed = _convoySystem.CalculateTotalDistance();
                    EndMission(new MissionResult(false, _savedLives.Value, _lostLives.Value, _lostUnits.Value,
                        _cargoDelivered.Value, _enemiesDestroyed.Value,_enemyVehiclesDestroyed.Value, totalDistancePassed, "Слишком большие потери"));
                    return;
                }

                break;
        }
    }

    private void OnAllySaved(AllySavedSignal signal)
    {
        _savedLives.Value += 1;
        Debug.Log("Человек спасён: +1 к счётчику, +10 деталей.");
    }

    private void OnCargoDelivered(CargoDeliveredSignal signal)
    {
        _cargoDelivered.Value = true;
    }

    private void OnEnemyDestroyed(EnemyDestroyedSignal signal)
    {
        switch (signal.enemyType)
        {
            case EnemyType.Infantry:
            case EnemyType.Grenadier:
                _enemiesDestroyed.Value += 1;
                break;
            case EnemyType.PickupWithGun:
                _enemiesDestroyed.Value += 3;
                _enemyVehiclesDestroyed.Value += 1;
                break;
        }
    }

    private void OnConvoyDefeated(ConvoyDefeatedSignal signal)
    {
        if (_isMissionActive.Value)
        {
            float totalDistancePassed = _convoySystem.CalculateTotalDistance();
            EndMission(new MissionResult(false, _savedLives.Value, _lostLives.Value, _lostUnits.Value,
                _cargoDelivered.Value, _enemiesDestroyed.Value, _enemyVehiclesDestroyed.Value, totalDistancePassed, "Все союзники погибли"));
        }
    }

    private void EndMission(MissionResult result)
    {
        _isMissionActive.Value = false;
        _missionResult.Value = result;
        _signalBus.Fire(new MissionCompletedSignal(result));
        _signalBus.Fire(new MissionEndedSignal());
        if (result.IsSuccess)
        {
            //SaveUnitParkState();
            _unitPark.AddMaterials(_currentLevelConfig.MaterialReward);
            SaveData(result);
        }
        GamePersistence.LoadGame();

        Debug.Log($"Mission Ended: Success={result.IsSuccess}");
    }

    private void SaveUnitParkState()
    {
        foreach (var unit in _convoySystem.Convoy.Where(u => u != null))
        {
            _unitPark.ReturnUnit(unit.Model);
        }

        //_unitPark.SaveData();
        Debug.Log("UnitPark state saved after successful mission.");
    }

    private void SaveData(MissionResult result)
    {
        foreach (var unit in _convoySystem.Convoy.Where(u => u != null))
        {
            _unitPark.ReturnUnit(unit.Model);
        }

        GameSaveData saveData = GamePersistence.SaveData;
        var level = saveData.Levels.FirstOrDefault(l => l.LevelId == _currentLevelConfig.LevelId);
        if (level == null)
        {
            saveData.Levels.Add(new LevelData(_currentLevelConfig.LevelId));
            level = saveData.Levels.FirstOrDefault(l => l.LevelId == _currentLevelConfig.LevelId);
        }

        level.CollectedBonusIds.AddRange(_bonusSystem.CollectedBonusIds);
        level.IsCompleted = true;

        saveData.Statistics.TotalDistancePassed += result.TotalDistancePassed;
        saveData.Statistics.TotalSaved += result.SavedLives;
        saveData.Statistics.TotalUnitsLost += result.LostUnits;
        saveData.Statistics.TotalLivesLost += result.LostLives;
        saveData.Statistics.TotalEnemiesDestroyed += result.EnemiesDestroyed;
        saveData.Statistics.TotalEnemyVehiclesDestroyed += result.EnemyVehiclesDestroyed;
        _unitPark.SaveData();
        GamePersistence.SaveGame();
    }

    public void Dispose()
    {
        _signalBus.Unsubscribe<StartMissionSignal>(StartMission);
        _signalBus.Unsubscribe<UnitDestroyedSignal>(OnUnitDestroyed);
        _signalBus.Unsubscribe<AllySavedSignal>(OnAllySaved);
        _signalBus.Unsubscribe<CargoDeliveredSignal>(OnCargoDelivered);
        _signalBus.Unsubscribe<ConvoyDefeatedSignal>(OnConvoyDefeated);
        _signalBus.Unsubscribe<EnemyDestroyedSignal>(OnEnemyDestroyed);
        _signalBus.Unsubscribe<AllUnitsParkedSignal>(CheckMissionCompletion);
        _disposables.Dispose();
    }
}