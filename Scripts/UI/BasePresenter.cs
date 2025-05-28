using System;
using UniRx;
using Zenject;

public abstract class BasePresenter<TView> : IInitializable, IDisposable where TView : class, IView
{
    protected readonly TView View;
    protected readonly SignalBus SignalBus;
    protected readonly CompositeDisposable Disposables = new();

    protected BasePresenter(TView view, SignalBus signalBus)
    {
        View = view;
        SignalBus = signalBus;
    }

    public abstract void Initialize();

    public virtual void Dispose()
    {
        Disposables.Dispose();
    }
}