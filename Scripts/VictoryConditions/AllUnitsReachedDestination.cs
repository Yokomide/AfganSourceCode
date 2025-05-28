using System.Linq;
using UnityEngine;

public class AllUnitsReachedDestination : IVictoryCondition
{
    private readonly ConvoySystem _convoySystem;
    private readonly Vector3 _destination;

    public AllUnitsReachedDestination(ConvoySystem convoySystem, Vector3 destination)
    {
        _convoySystem = convoySystem;
        _destination = destination;
    }

    public bool IsConditionMet()
    {
        var activeUnits = _convoySystem.Convoy.Where(u => u != null).ToList();
        return activeUnits.Count > 0 && activeUnits.All(unit => unit.HasReachedDestination(1));
    }
}