using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class DefeatPresenter : BasePresenter<DefeatView>
{
    //private readonly SceneLoader _sceneLoader;
    [Inject] private UnitPark _unitPark;
    public DefeatPresenter(DefeatView view, SignalBus signalBus/*, SceneLoader sceneLoader*/)
        : base(view, signalBus)
    {
    }

    public override void Initialize()
    {
        SignalBus.Subscribe<ConvoyDefeatedSignal>(OnConvoyDefeated);
        SignalBus.Subscribe<MissionCompletedSignal>(OnMissionCompleted);
        View.Hide();
    }
    private void OnMissionCompleted(MissionCompletedSignal signal)
    {
        if (!signal.Result.IsSuccess)
        {
            ShowDefeatScreen(signal.Result.Reason);
        }
    }
    private void OnConvoyDefeated(ConvoyDefeatedSignal signal)
    {
        ShowDefeatScreen(signal.DefeatReson);
    }

    private void ShowDefeatScreen(string defeatReason)
    {
        _unitPark.ReloadUnits();
        View.SetDefeatReasonMessage(defeatReason); 
        View.Show();
    }

    public override void Dispose()
    {
        SignalBus.Unsubscribe<ConvoyDefeatedSignal>(OnConvoyDefeated);
        SignalBus.Unsubscribe<MissionCompletedSignal>(OnMissionCompleted);
    }
}