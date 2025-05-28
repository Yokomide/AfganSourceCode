using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public class GamePersistence
{
    private static readonly Dictionary<string, UnitConfig> _unitConfigs;
    private static readonly string SavePath = Path.Combine(Application.persistentDataPath, "gameSave.json");

    public static GameSaveData SaveData;
    static GamePersistence()
    {
        UnitConfig[] configs = Resources.LoadAll<UnitConfig>("Configs/Units");
        _unitConfigs = configs.ToDictionary(c => c.Id.ToString(), c => c);
        Debug.Log("Loaded " + _unitConfigs.Count + " unit configs from Resources.");
        SaveData = LoadGame();
    }

    public static void SaveGame()
    {
        Debug.Log("SaveGameDataWasSaved");
        string json = JsonUtility.ToJson(SaveData, true);
        File.WriteAllText(SavePath, json);
        Debug.Log($"Game saved to {SavePath}");
    }

    public static GameSaveData LoadGame()
    {
        Debug.Log("LoadGame");
        if (!File.Exists(SavePath))
        {
            Debug.Log("No game save file found.");
            return new GameSaveData();
        }

        string json = File.ReadAllText(SavePath);
        var saveData = JsonUtility.FromJson<GameSaveData>(json);

        if (saveData == null)
        {
            Debug.Log("Invalid save data, returning default.");
            return new GameSaveData();
        }

        return saveData;
    }

    public static void ClearGameSave()
    {
        if (File.Exists(SavePath))
        {
            File.Delete(SavePath);
            Debug.Log("Game save file deleted.");
        }
    }

    public static List<UnitModel> GetUnitsFromSave()
    {
        var units = new List<UnitModel>();
        if (SaveData.UnitPark?.Units != null)
        {
            foreach (var unitData in SaveData.UnitPark.Units)
            {
                if (unitData != null && _unitConfigs.TryGetValue(unitData.Id.ToString(), out UnitConfig config))
                {
                    units.Add(unitData.ToUnitModel(config));
                }
            }
        }
        Debug.Log("Loaded Units count: " + units.Count);
        return units;
    }
}