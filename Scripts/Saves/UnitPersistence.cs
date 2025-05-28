using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class UnitPersistence
{
    private static readonly string SavePath = Path.Combine(Application.persistentDataPath, "park.json");

    public static void SaveUnitPark(List<UnitModel> unitPark)
    {
        var saveData = new UnitParkSaveData(unitPark);
        string json = JsonUtility.ToJson(saveData, true); // true для читаемого формата
        File.WriteAllText(SavePath, json);
        Debug.Log($"Convoy saved to {SavePath}");
    }

    public static List<UnitModel> LoadUnitPark(Dictionary<string, UnitConfig> configs)
    {
        if (!File.Exists(SavePath))
        {
            Debug.Log("No park save file found.");
            return new List<UnitModel>();
        }

        string json = File.ReadAllText(SavePath);
        var saveData = JsonUtility.FromJson<UnitParkSaveData>(json);
        var units = new List<UnitModel>();

        if (saveData?.Units != null)
        {
            foreach (var unitData in saveData.Units)
            {
                if (unitData != null && configs.TryGetValue(unitData.Id.ToString(), out UnitConfig config))
                {
                   // var model = unitData.ToUnitModel(config);
                   units.Add(unitData.ToUnitModel(config));
                }
            }
        }
        Debug.Log("Return Units count: " + units);
        return units;
    }

    public static void ClearUnitParkSave()
    {
        Debug.Log("UnitPark save file deleted 2.");

        if (File.Exists(SavePath))
        {
            File.Delete(SavePath);
            Debug.Log("UnitPark save file deleted.");
        }
    }
}