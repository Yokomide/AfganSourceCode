using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using Zenject;
using UnityEngine;

public class BonusSystem : IInitializable, IDisposable
{
    private readonly SignalBus _signalBus;
    private readonly ConvoySystem _convoySystem;
    private readonly CompositeDisposable _disposables = new();
    private readonly UnitPark _unitPark;

    [Inject] private Notification _notification;
    [Inject] private LevelConfig _levelConfig;
    
    public readonly List<int> CollectedBonusIds = new List<int>();
    public BonusSystem(SignalBus signalBus, ConvoySystem convoySystem, UnitPark unitPark)
    {
        _signalBus = signalBus;
        _convoySystem = convoySystem;
        _unitPark = unitPark;
    }

    public void Initialize()
    {
        _signalBus.Subscribe<BonusActivatedSignal>(OnBonusActivated);
    }

    private void OnBonusActivated(BonusActivatedSignal signal)
    {
        var bonus = signal.Bonus;
        string message;
        switch (bonus.Type)
        {
            case BonusType.Human:
                _signalBus.Fire(new AllySavedSignal());
                _unitPark.AddMaterials(10);
                message = "<b>Человек спасен</b>\n +10 материалов";
                _notification.ShowNotification(Notification.NotificationType.Notify, message);
                break;

            case BonusType.Cargo:
                _unitPark.AddMaterials(30);
                message = "<b>Груз собран</b>\n +30 материалов";
                _notification.ShowNotification(Notification.NotificationType.Notify, message);

                break;

            case BonusType.Unit:
                UnitBonus unitBonus = bonus as UnitBonus;

                _convoySystem.AddUnitToEnd(unitBonus.GetUnitController());
                message = "<b>Брошенная техника добавлена в колонну</b>";
                _notification.ShowNotification(Notification.NotificationType.Notify, message);
                break;

            default:
                Debug.LogError("Неизвестный тип бонуса");
                break;
        }
        SaveCollectedBonus(bonus.BonusId);
    }
    private void SaveCollectedBonus(int bonusId)
    {
        CollectedBonusIds.Add(bonusId);
    }


    public void Dispose()
    {
        _disposables.Dispose();
    }
}