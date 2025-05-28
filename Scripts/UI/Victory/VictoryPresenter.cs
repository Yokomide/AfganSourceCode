using Zenject;

public class VictoryPresenter : BasePresenter<VictoryView>
{
    public VictoryPresenter(VictoryView view, SignalBus signalBus) : base(view, signalBus) { }
    public override void Initialize()
    {
        SignalBus.Subscribe<MissionCompletedSignal>(OnMissionCompleted);
        View.Hide();
    }

    private void OnMissionCompleted(MissionCompletedSignal signal)
    {
        if (signal.Result.IsSuccess)
        {

            View.Show();
            SetInfo(signal.Result);
        }
    }
    public void SetInfo(MissionResult result)
    {
        View.SetInfo(result.TotalDistancePassed,result.SavedLives, result.LostLives, result.LostUnits,result.EnemiesDestroyed );
    }

    public override void Dispose()
    {
        SignalBus.Unsubscribe<MissionCompletedSignal>(OnMissionCompleted);
    }
}