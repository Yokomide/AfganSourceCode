using System.Linq;

public class MinimumUnitsSurvived : IVictoryCondition
{
    private readonly ConvoySystem _convoySystem;
    private readonly int _minUnits;

    public MinimumUnitsSurvived(ConvoySystem convoySystem, int minUnits)
    {
        _convoySystem = convoySystem;
        _minUnits = minUnits;
    }

    public bool IsConditionMet()
    {
        var activeUnits = _convoySystem.Convoy.Where(u => u != null).ToList();
        return activeUnits.Count >= _minUnits;
    }
}