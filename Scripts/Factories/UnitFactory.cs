using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

public class UnitFactory: IInitializable
{
    
    private readonly DiContainer _container;
    private readonly Dictionary<string, UnitConfig> _unitConfigs;
    private readonly List<UnitModel> _availableUnits = new();
    public UnitFactory(DiContainer container, UnitConfig[] unitConfigs)
    {
        _container = container;
        _unitConfigs = unitConfigs.ToDictionary(c => c.Id.ToString(), c => c);
    }
    public void Initialize()
    {
        foreach (var config in _unitConfigs.Values)
        {
            _availableUnits.Add(config.CreateUnitModel());
        }
    }
    public UnitController Create(UnitModel model)
    {
        var prefab = Resources.Load<GameObject>(GetPrefabPath(model.Type));
        var instance = _container.InstantiatePrefab(prefab);
        var controller = instance.GetComponent<UnitController>();
        controller.Initialize(model);
        return controller;
    }

    public UnitModel GetUnitModel(UnitType unitType)
    {  
        return _availableUnits.FirstOrDefault(u => u.Type == unitType);
    }
    private string GetPrefabPath(UnitType type) => $"Units/{type}";

}
