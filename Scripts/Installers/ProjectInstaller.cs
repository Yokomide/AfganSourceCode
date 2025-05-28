using UnityEngine;
using Zenject;

public class ProjectInstaller : MonoInstaller
{
    [SerializeField] private UnitConfig[] _unitConfigs;
    public override void InstallBindings()
    {
        SignalBusInstaller.Install(Container);
        Container.Bind<UnitConfig[]>().FromInstance(_unitConfigs).AsSingle();
    }
}