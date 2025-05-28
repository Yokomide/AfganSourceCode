using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using Zenject;

public class PreparationRepairPresenter : BasePresenter<PreparationRepairView>, IInitializable, IDisposable
{
    [Inject] private UnitPark _unitPark;
    [Inject] private PreparationView _preparationView;
    [Inject] private ConvoySystem _convoySystem;
    private int _totalRepairCost;

    public PreparationRepairPresenter(PreparationRepairView view, SignalBus signalBus) : base(view, signalBus)
    {
        view.SetPresenter(this);
    }

    public override void Initialize()
    {
    }

    public void SetRepairCost()
    {
        _totalRepairCost = 0;
        foreach (UnitModel unit in _unitPark.AvailableUnits)
        {
            _totalRepairCost += CalculateRepairCost(unit.Durability.Value);
        }

        View.SetRepairCost(_totalRepairCost);
    }

    public void UpdateMaterials()
    {
        View.UpdateMaterialsText(_unitPark.Materials);
    }

    public bool CheckNeedOfRepair()
    {
        return _totalRepairCost > 0;
    }

    private bool IsAllConvoyNull()
    {
        for (int i = 0; i < _convoySystem.Convoy.Count; i++)
        {
            if (_convoySystem.Convoy[i] != null)
                return false;
        }

        return true;
    }

    public void OnRepairButtonClicked()
    {
        if (!IsAllConvoyNull())
        {
            View.ShowErrorMessage("Вся техника должна быть в парке");
            return;
        }

        if (_unitPark.SpendMaterials(_totalRepairCost))
        {
            foreach (UnitModel unit in _unitPark.AvailableUnits)
            {
                unit.Durability.Value = 1f;
                Debug.Log($"Техника {unit.Id} починена за {CalculateRepairCost(unit.Durability.Value)} материалов.");
            }
            
            View.UpdateMaterialsText(_unitPark.Materials);
            foreach (var drag in _unitPark.UnitButtons.Values)
            {
                drag.UpdateHealthInfo();
            }

            _unitPark.SaveData();
            GamePersistence.SaveGame();
            _preparationView.PopulateInventory();

            View.Hide();
            Debug.Log($"Все юниты починены за {_totalRepairCost} материалов.");
        }
        else
        {
            Debug.Log("Недостаточно материалов для починки всех юнитов!");
            View.ShowErrorMessage("Недостаточно материалов");
        }
    }

    private int CalculateRepairCost(float currentDurability)
    {
        if (Math.Abs(currentDurability - 1) < 0.01f)
            return 0;
        const float baseCost = 1f;
        const float maxCost = 100f;
        float damageFactor = 1f - currentDurability;
        return Mathf.RoundToInt(Mathf.Lerp(baseCost, maxCost, damageFactor));
    }
}