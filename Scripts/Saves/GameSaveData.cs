using System.Collections.Generic;

[System.Serializable]
public class GameSaveData
{
    public int Version = 1;             
    public UnitParkSaveData UnitPark;   
    public List<LevelData> Levels;       
    public StatisticsData Statistics;   
    public int Materials;               
    
    public GameSaveData()
    {
        UnitPark = new UnitParkSaveData(new List<UnitModel>());
        Levels = new List<LevelData>();
        Statistics = new StatisticsData();
        Materials = 0;
    }
}