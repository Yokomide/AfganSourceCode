using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using Zenject;

public class UnitPark : IInitializable, ITickable
{
    private readonly List<UnitModel> _availableUnits = new();
    private readonly List<UnitModel> _destroyedUnits = new();
    private readonly Dictionary<UnitModel, DragHandler> _unitButtons = new();
    
    private int _materials = 0;
    public int Materials => _materials;
    public IReadOnlyList<UnitModel> AvailableUnits => _availableUnits.AsReadOnly();
    public Dictionary<UnitModel, DragHandler> UnitButtons => _unitButtons;
    public UnitPark(UnitConfig[] unitConfigs)
    {
        _unitConfigs = unitConfigs.ToDictionary(c => c.Id.ToString(), c => c);
    }

    public void Initialize()
    {
        ReloadUnits();
        _materials = GamePersistence.SaveData.Materials;
    }

    public void ReloadUnits()
    {
        _availableUnits.Clear();
        GameSaveData saveData = GamePersistence.SaveData;
        var loadedUnits = GamePersistence.GetUnitsFromSave();

        if (loadedUnits.Count > 0)
        {
            _availableUnits.AddRange(loadedUnits);
        }
        else
        {
            foreach (var config in _unitConfigs.Values)
            {
                _availableUnits.Add(config.CreateUnitModel());
            }
            _materials = 0;
        }
    }
    public UnitModel TakeUnit(UnitModel model)
    {
        var unit = _availableUnits.FirstOrDefault(u => u == model);
        if (unit != null)
        {
            _availableUnits.Remove(unit);
        }
        Debug.Log("Return unit: " + unit);
        return unit;
    }

    public void AddDestroyed(UnitModel model)
    {
        _destroyedUnits.Add(model);
    }
    public void RegisterButton(UnitModel unitModel, DragHandler button)
    {
        _unitButtons[unitModel] = button;
    }

    public void UnregisterButtons()
    {
        _unitButtons.Clear();
    }
    public void ReturnUnit(UnitModel unit)
    {
        if (unit.Durability.Value < 0.5f)
        {
            unit.Durability.Value = 0.5f;
        }
        _availableUnits.Add(unit);
        for (int i = 0; i < _availableUnits.Count; i++)
        {
            Debug.Log("AvaiableUnitsInPark: " + _availableUnits[i].Name);
        }
        if (_unitButtons.TryGetValue(unit, out DragHandler button))
        {
            Debug.Log("ActivateButton");
            button.gameObject.SetActive(true); 
        }
    }

    public void AddMaterials(int amount)
    {
        _materials += amount;
        if (_materials < 0) _materials = 0;
    }

    public bool SpendMaterials(int amount)
    {
        if (_materials >= amount)
        {
            _materials -= amount;
            return true;
        }
        return false;
    }

    public void SaveData()
    {
        List<UnitModel> saveUnits = new List<UnitModel>();
        saveUnits.AddRange(_availableUnits);
        saveUnits.AddRange(_destroyedUnits);
        GamePersistence.SaveData.UnitPark = new UnitParkSaveData(saveUnits);
        GamePersistence.SaveData.Materials = _materials;

    }

    public void Tick()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            GamePersistence.ClearGameSave(); 
            PlayerPrefs.DeleteAll();
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            for (int i = 0; i < _availableUnits.Count; i++)
            {
                Debug.Log("ModelData: "  + _availableUnits[i].Durability.Value + " " + _availableUnits[i].Type);
            }
        }
    }

    private readonly Dictionary<string, UnitConfig> _unitConfigs;
}