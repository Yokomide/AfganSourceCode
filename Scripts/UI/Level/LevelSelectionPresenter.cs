using UnityEngine;
using Zenject;

public class LevelSelectionPresenter : BasePresenter<LevelSelectionView>
{
    private SignalBus _signalBus;
    public LevelSelectionPresenter(LevelSelectionView view, SignalBus signalBus) 
        : base(view, signalBus)
    {
        _signalBus = signalBus;
        view.SetPresenter(this);
    }

    public override void Initialize()
    {
        View.Show();
    }

    public override void Dispose()
    {
        base.Dispose();
        View.Hide();
    }
}