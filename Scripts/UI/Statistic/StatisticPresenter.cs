using System;
using Unity.VisualScripting;
using Zenject;
using IInitializable = Zenject.IInitializable;

public class StatisticPresenter : BasePresenter<StatisticView>, IInitializable, IDisposable
{
    public StatisticPresenter(StatisticView view, SignalBus signalBus) : base(view, signalBus)
    {
        view.SetPresenter(this);
    }

    public override void Initialize()
    {
    }

    public void ShowStatistic()
    {
        var saveData = GamePersistence.SaveData;
        
        float distance = saveData.Statistics.TotalDistancePassed;
        int saved = saveData.Statistics.TotalSaved;
        int livesLost = saveData.Statistics.TotalLivesLost;
        int unitsLost = saveData.Statistics.TotalUnitsLost;
        int enemiesDestroyed = saveData.Statistics.TotalEnemiesDestroyed;
        int enemyVehiclesDestroyed = saveData.Statistics.TotalEnemyVehiclesDestroyed;
        View.UpdateStatisticsText(distance, saved, livesLost, unitsLost, enemiesDestroyed, enemyVehiclesDestroyed);
    }
    
    
}