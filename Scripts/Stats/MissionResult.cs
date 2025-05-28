public struct MissionResult
{
    public bool IsSuccess { get; }
    public int SavedLives { get; }
    public int LostLives { get; }
    public int LostUnits { get; }
    public bool CargoDelivered { get; }
    public int EnemiesDestroyed { get; }
    public int EnemyVehiclesDestroyed { get; }
    public string Reason { get; }
    public float TotalDistancePassed { get; }

    public MissionResult(bool isSuccess, int savedLives, int lostLives, int lostUnits, bool cargoDelivered, int enemiesDestroyed, int enemyVehiclesDestroyed, float totalDistancePassed, string reason)
    {
        IsSuccess = isSuccess;
        SavedLives = savedLives;
        LostLives = lostLives;
        LostUnits = lostUnits;
        CargoDelivered = cargoDelivered;
        EnemiesDestroyed = enemiesDestroyed;
        EnemyVehiclesDestroyed = enemyVehiclesDestroyed;
        TotalDistancePassed = totalDistancePassed;
        Reason = reason;
    }
}