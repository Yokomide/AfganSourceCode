using System.Linq;
using UnityEngine;

public class SpecificUnitReachedDestination : IVictoryCondition
{
    private readonly int _specificUnitID;
    private readonly Vector3 _destination;
    private readonly ConvoySystem _convoySystem;

    public SpecificUnitReachedDestination(ConvoySystem convoySystem, int specificUnitID, Vector3 destination)
    {
        _convoySystem = convoySystem;
        _specificUnitID = specificUnitID;
        _destination = destination;
    }

    public bool IsConditionMet()
    {
        var specificUnit = _convoySystem.Convoy.FirstOrDefault(u => u?.Model.Id == _specificUnitID);
        if (specificUnit != null)
            return true;
        return false;
    }
}