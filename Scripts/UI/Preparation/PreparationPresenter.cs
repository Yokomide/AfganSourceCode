using System;
using System.Linq;
using UniRx;
using UnityEngine;
using Zenject;

public class PreparationPresenter : BasePresenter<PreparationView>
{
    private readonly ConvoySystem _convoySystem;
    private readonly LevelConfig _levelConfig;

    public PreparationPresenter(PreparationView view, SignalBus signalBus, ConvoySystem convoySystem, LevelConfig levelConfig)
        : base(view, signalBus)
    {
        _convoySystem = convoySystem;
        _levelConfig = levelConfig;
    }

    public override void Initialize()
    {
        View.OnUnitDragged.Subscribe(OnUnitDragged).AddTo(Disposables);
        View.OnStartMissionClicked.Subscribe(_ => OnMissionSignal()).AddTo(Disposables);
        Observable.CombineLatest(
                _convoySystem.ConvoySpeed,
                _convoySystem.ConvoyDamage,
                _convoySystem.ConvoyCargo,
                (speed, damage, cargo) => (speed, damage, cargo)
            )
            .Subscribe(values => View.UpdateConvoyStats(values.speed, values.damage, values.cargo))
            .AddTo(Disposables);
        View.Show();
        View.SetMissionInfo(_levelConfig.Title, _levelConfig.TaskDescription);
    }

    private void OnUnitDragged(UnitDragData data)
    {
        SignalBus.Fire(new AddUnitToConvoySignal(data.DragUnitController, data.SlotIndex));
    }

    private void OnMissionSignal()
    {
        var convoyUnits = _convoySystem.Convoy.Where(u => u != null).ToList();
        if (!convoyUnits.Any())
        {
            MissionStartFailed("<b><color=red>Невозможно начать миссию</color></b>\n Конвой пуст!");
            return;
        }
        
        switch (_levelConfig.ConditionType)
        {
            case VictoryConditionType.SpecificUnitReachedDestination:
                // Проверяем, есть ли юнит с нужным Id (например, 2)
                if (!convoyUnits.Any(u => u.Model.Id == _levelConfig.SpecificUnitId))
                {
                    MissionStartFailed($"<b><color=red>Невозможно начать миссию</color></b>\n Отсутствует необходимый юнит!");
                    return;
                }
                break;

            case VictoryConditionType.AllUnitsReachedDestination:
                // Требуем хотя бы один юнит
                if (convoyUnits.Count == 0)
                {
                    MissionStartFailed("<b><color=red>Невозможно начать миссию</color></b>\n Добавьте хотя бы один юнит!");
                    return;
                }
                break;

            case VictoryConditionType.MinimumUnitsSurvived:
                // Проверяем минимальное количество юнитов
                if (convoyUnits.Count < _levelConfig.MinUnits)
                {
                    MissionStartFailed($"<b><color=red>Невозможно начать миссию</color></b>\n Нужно минимум {_levelConfig.MinUnits} юнитов!");
                    return;
                }
                break;

            case VictoryConditionType.EnemiesDestroyed:
                // Требуем хотя бы одну боевую единицу (UnitClass.Combat)
                if (!convoyUnits.Any(u => u.Model.Class == UnitClass.Combat))
                {
                    MissionStartFailed("<b><color=red>Невозможно начать миссию</color></b>\n Добавьте хотя бы одну боевую единицу!");
                    return;
                }
                break;

            default:
                Debug.LogError("Unknown victory condition type");
                return;
        }
        
        SignalBus.Fire(new StartMissionSignal());
        View.Hide();
    }

    private void MissionStartFailed(string reason)
    {
        View.ShowErrorMessage(reason);
    }

    public override void Dispose()
    {
        base.Dispose();
    }
}