using System;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class UnitSaveData
{
    public int Id;
    public int Crew;
    public int Health;
    public float Durability;
    public float Speed;
    public float FireRate;
    
    public int Damage;


    public UnitSaveData(UnitModel model)
    {
        Id = model.Id;
        Crew = model.Crew;
        Durability = model.Durability.Value;
        Speed = model.Speed;
        Damage = model.Damage;
        FireRate = model.FireRate;
    }

    public UnitModel ToUnitModel(UnitConfig config)
    {
        const float MinDurabilityThreshold = 0.5f;
        const float DefaultDurability = 1f;

        float GetDurability() => Durability switch
        {
            < 0 => DefaultDurability,
            < MinDurabilityThreshold => MinDurabilityThreshold,
            _ => Durability
        };

        float durabilityValue = GetDurability();
        
        var model = new UnitModel(
            config.Name,
            config.Id,
            config.Crew,
            config.BaseHealth,
            durabilityValue,
            Speed > 0 ? Speed : config.BaseSpeed,
            Damage > 0 ? Damage : config.BaseDamage,
            config.Type,
            config.Class,
            FireRate > 0 ? FireRate : config.BaseFireRate
        );
        return model;
    }
}

[System.Serializable]
public class UnitParkSaveData
{
    public UnitSaveData[] Units;

    public UnitParkSaveData(List<UnitModel> units)
    {
        Units = units.Select(u => new UnitSaveData(u)).ToArray();
    }
}