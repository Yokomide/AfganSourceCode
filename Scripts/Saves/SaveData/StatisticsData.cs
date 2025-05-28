[System.Serializable]
public class StatisticsData
{
    public float TotalDistancePassed;       
    public int TotalSaved;       
    public int TotalLivesLost;     
    public int TotalUnitsLost;      
    public int TotalEnemiesDestroyed;      
    public int TotalEnemyVehiclesDestroyed;       

    public StatisticsData()
    {
        TotalDistancePassed = 0;
        TotalSaved = 0;
        TotalLivesLost = 0;
        TotalUnitsLost = 0;
        TotalEnemiesDestroyed = 0;
        TotalEnemyVehiclesDestroyed = 0;
    }
}