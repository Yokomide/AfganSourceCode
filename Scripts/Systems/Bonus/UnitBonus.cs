using UnityEngine;
using Zenject;

public class UnitBonus : Bonus
{
    [SerializeField] private UnitType _unitType;
    [SerializeField]private UnitController _unitController;
    [Inject] private UnitFactory _unitFactory;


    protected override void Start()
    {
        base.Start();

        Type = BonusType.Unit;
        var unitModel = _unitFactory.GetUnitModel(_unitType);
        unitModel.Durability.Value = Random.Range(0.5f, 1f);
        unitModel.Health.Value = (int)(unitModel.MaxHealth * unitModel.Durability.Value);

        _unitController.Initialize(unitModel);
    }

    public UnitController GetUnitController()
    {
        return _unitController;
    }
    protected override void Activate()
    {
        _signalBus.Fire(new BonusActivatedSignal(this));
        _effect.SetActive(false);
        _canActivate = false;
    }
}