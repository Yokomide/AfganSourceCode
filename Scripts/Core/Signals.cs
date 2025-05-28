using UnityEngine;

public struct StartMissionSignal { }


public struct EnemyDestroyedSignal
{
    public EnemyType enemyType;
    public EnemyDestroyedSignal(EnemyType enemyType)
    {
        this.enemyType = enemyType;
    }
    
}
public struct AllUnitsParkedSignal { }

public struct MissionEndedSignal { }

public class LevelSelectedSignal
{
    public string LevelId { get; }
    public LevelSelectedSignal(string levelId) => LevelId = levelId;
}

public class LevelCompletedSignal
{
    public int LevelId { get; }
    public LevelCompletedSignal(int levelId) => LevelId = levelId;
}

public struct AddUnitToConvoySignal
{
    public UnitController AddedUnitController { get; }
    public int SlotIndex { get; }

    public AddUnitToConvoySignal(UnitController unitController, int slotIndex, UnitController existingUnit = null)
    {
        AddedUnitController = unitController;
        SlotIndex = slotIndex;
    }
}
public struct BonusActivatedSignal
{
    public Bonus Bonus { get; }
    public BonusActivatedSignal(Bonus bonus) => Bonus = bonus;
}
public struct FireCommandSignal
{
    public Vector3 TargetPosition { get; }
    public FireCommandSignal(Vector3 targetPosition) => TargetPosition = targetPosition;
}
public struct ConvoyDefeatedSignal
{
    public string DefeatReson { get; }
    public ConvoyDefeatedSignal(string defeatReson) => DefeatReson = defeatReson;


}
public struct HitSignal
{
    public Vector2 ScreenPosition { get; }

    public HitSignal(Vector2 screenPosition)
    {
        ScreenPosition = screenPosition;
    }
}
public struct UnitDestroyedSignal // Заменил VehicleDestroyedSignal
{
    public UnitModel Unit { get; }
    public UnitDestroyedSignal(UnitModel unit) => Unit = unit;
}

public struct AllySavedSignal { }

public struct CargoDeliveredSignal { }

public struct MissionCompletedSignal
{
    public MissionResult Result { get; }
    public MissionCompletedSignal(MissionResult result) => Result = result;
}