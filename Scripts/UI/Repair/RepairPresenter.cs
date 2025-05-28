using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using Zenject;

public class RepairPresenter : BasePresenter<RepairView>, IInitializable, IDisposable
{
    [Inject] private UnitPark _unitPark;

    private List<RepairUnitCard> unitCards = new();

    public RepairPresenter(RepairView view, SignalBus signalBus) : base(view, signalBus)
    {
        view.SetPresenter(this);
    }

    public override void Initialize()
    {
    }

    public void ShowPanel()
    {
        if (unitCards.Count == 0)
        {
            LoadUnits(View.UnitCardsContainer, View.UnitCardPrefab);
        }
        View.UpdateMaterialsText(_unitPark.Materials);
    }

    public void LoadUnits(Transform container, GameObject cardPrefab)
    {
        foreach (var card in unitCards)
        {
            UnityEngine.Object.Destroy(card.gameObject);
        }
        unitCards.Clear();

        foreach (var unit in _unitPark.AvailableUnits)
        {
            GameObject cardObject = UnityEngine.Object.Instantiate(cardPrefab, container);
            RepairUnitCard card = cardObject.GetComponent<RepairUnitCard>();
            card.Initialize(unit, OnRepairButtonClicked, CalculateRepairCost);
            unitCards.Add(card);
        }
    }

    private void OnRepairButtonClicked(UnitModel unit)
    {
        int repairCost = CalculateRepairCost(unit.Durability.Value);
        if (_unitPark.SpendMaterials(repairCost))
        {
            unit.Durability.Value = 1f;
            unit.UpdateHealth();
            View.UpdateMaterialsText(_unitPark.Materials);
            _unitPark.SaveData();
            GamePersistence.SaveGame();
            Debug.Log($"Техника {unit.Id} починена за {repairCost} материалов.");
        }
        else
        {
            Debug.Log("Недостаточно материалов для починки!");
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

    public override void Dispose()
    {
        base.Dispose();
    }
    
    public Transform UnitCardsContainer => View.UnitCardsContainer;
    public GameObject UnitCardPrefab => View.UnitCardPrefab;
}