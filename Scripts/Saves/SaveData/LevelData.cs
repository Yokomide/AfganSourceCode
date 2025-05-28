using System.Collections.Generic;

[System.Serializable]
public class LevelData
{
    public int LevelId;    
    public bool IsCompleted;
    public List<int> CollectedBonusIds; 
    public LevelData(int levelId)
    {
        LevelId = levelId;
        IsCompleted = false;
        CollectedBonusIds = new List<int>();
    }
}