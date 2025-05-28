
    using Zenject;

    public class SettingsPresenter : BasePresenter<SettingsView>
    {
        public SettingsPresenter(SettingsView view, SignalBus signalBus) : base(view, signalBus)
        {
            view.SetPresenter(this);
        }

        public override void Initialize()
        {

        }
    }
