using UnityEngine;
using Zenject;

public class LevelInstaller : MonoInstaller
{
    [SerializeField] private LevelConfig _levelConfig;

    public override void InstallBindings()
    {
        Container.Bind<LevelConfig>().FromInstance(_levelConfig).AsSingle();

        Container.BindInterfacesAndSelfTo<ConvoySystem>().AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<MissionSystem>().AsSingle().NonLazy();
        
        Container.BindInterfacesAndSelfTo<LevelInitializer>().AsSingle().NonLazy();
    }
}

public class LevelInitializer : IInitializable
{
    private readonly ConvoySystem _convoySystem;
    private readonly MissionSystem _missionSystem;
    private readonly LevelConfig _levelConfig;
    private readonly SignalBus _signalBus;

    public LevelInitializer(ConvoySystem convoySystem, MissionSystem missionSystem, LevelConfig levelConfig, SignalBus signalBus)
    {
        _convoySystem = convoySystem;
        _missionSystem = missionSystem;
        _levelConfig = levelConfig;
        _signalBus = signalBus;
    }

    public void Initialize()
    {
        GamePersistence.LoadGame();
        _missionSystem.SetLevelConfig(_levelConfig);
    }
}