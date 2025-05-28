using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class MainMenuInstaller : MonoInstaller
{
    [SerializeField] private RepairView _repairView;
    [SerializeField] private StatisticView _statisticView;
    [SerializeField] private LevelSelectionView _levelView;
    [SerializeField] private SettingsView _settingsView;
    [SerializeField] private LinksView _linksView;
    [SerializeField] private Notification _notification;

    public override void InstallBindings()
    {
        
        Container.DeclareSignal<LevelCompletedSignal>();
        Container.DeclareSignal<LevelSelectedSignal>();
        
        Container.BindInterfacesAndSelfTo<UnitPark>().AsSingle().NonLazy();

        
        Container.Bind<RepairView>().FromInstance(_repairView).AsSingle();
        Container.BindInterfacesAndSelfTo<RepairPresenter>().AsSingle();

        Container.Bind<StatisticView>().FromInstance(_statisticView).AsSingle();
        Container.BindInterfacesAndSelfTo<StatisticPresenter>().AsSingle();
        
        Container.Bind<LevelSelectionView>().FromInstance(_levelView).AsSingle();
        Container.BindInterfacesAndSelfTo<LevelSelectionPresenter>().AsSingle();
        
        Container.Bind<SettingsView>().FromInstance(_settingsView).AsSingle();
        Container.BindInterfacesAndSelfTo<SettingsPresenter>().AsSingle();
        
        Container.Bind<LinksView>().FromInstance(_linksView).AsSingle();
        Container.BindInterfacesAndSelfTo<LinksPresenter>().AsSingle();
        
        Container.Bind<Notification>().FromInstance(_notification).AsSingle();

    }
}
