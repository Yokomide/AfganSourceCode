using Zenject;

public class LinksPresenter : BasePresenter<LinksView>
{
    public LinksPresenter(LinksView view, SignalBus signalBus) : base(view, signalBus)
    {
        view.SetPresenter(this);
    }

    public override void Initialize()
    {

    }
}