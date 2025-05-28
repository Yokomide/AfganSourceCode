using UnityEngine;

[CreateAssetMenu(fileName = "LevelConfig", menuName = "Game/LevelConfig")]
public class LevelConfig : ScriptableObject
{
    [Header("Level Settings")]
    public int LevelId;
    [TextArea]
    public string LevelDiscription;
    public int MaterialReward;
    
    [Header("Mission Settings")]
    public string Title; 
    public string TaskDescription; 
    public VictoryConditionType ConditionType;
    public int MinUnits;
    public int RequiredEnemiesDestroyed;
    public int SpecificUnitId;
}