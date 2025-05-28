using UnityEngine;
using Zenject;

public class GameInstaller : MonoInstaller
{
   // [SerializeField] private UnitConfig[] _unitConfigs;
    [SerializeField] private HitIndicator _hitIndicator;
    [SerializeField] private Crosshair _crosshair;
    [SerializeField] private DefeatView _defeatView;
    [SerializeField] private VictoryView _victoryView;
    [SerializeField] private PreparationRepairView _preparationRepairView;
    [SerializeField] private Notification _notification;
    public override void InstallBindings()
    {
        Debug.Log("InstallBindings");
        Container.DeclareSignal<StartMissionSignal>();
        Container.DeclareSignal<AddUnitToConvoySignal>();
        Container.DeclareSignal<FireCommandSignal>();
        Container.DeclareSignal<UnitDestroyedSignal>();
        Container.DeclareSignal<AllySavedSignal>();
        Container.DeclareSignal<CargoDeliveredSignal>();
        Container.DeclareSignal<MissionCompletedSignal>();
        Container.DeclareSignal<HitSignal>();
        Container.DeclareSignal<ConvoyDefeatedSignal>();
        Container.DeclareSignal<EnemyDestroyedSignal>();
        Container.DeclareSignal<AllUnitsParkedSignal>();
        Container.DeclareSignal<MissionEndedSignal>();
        Container.DeclareSignal<BonusActivatedSignal>();
        

        Container.BindInterfacesAndSelfTo<PreparationPresenter>().AsSingle().NonLazy();
        
        Container.BindInterfacesAndSelfTo<UnitPark>().AsSingle().NonLazy();

        Container.BindInterfacesAndSelfTo<UnitFactory>().AsSingle();
        Container.Bind<ProjectileFactory>().AsSingle();
        
        Container.Bind<DefeatView>().FromInstance(_defeatView).AsSingle();
        Container.Bind<VictoryView>().FromInstance(_victoryView).AsSingle();
        Container.Bind<PreparationRepairView>().FromInstance(_preparationRepairView).AsSingle();
        
        Container.BindInterfacesAndSelfTo<DefeatPresenter>().AsSingle();
        Container.BindInterfacesAndSelfTo<VictoryPresenter>().AsSingle();
        Container.BindInterfacesAndSelfTo<PreparationRepairPresenter>().AsSingle();
        
        Container.Bind<HitIndicator>().FromInstance(_hitIndicator).AsSingle();
        Container.Bind<Crosshair>().FromInstance(_crosshair).AsSingle();
        Container.BindInterfacesAndSelfTo<BonusSystem>().AsSingle().NonLazy();
        
        Container.Bind<ConvoyPlacer>().FromComponentInHierarchy().AsSingle();
        Container.Bind<PreparationView>().FromComponentInHierarchy().AsSingle();
        Container.Bind<DragHandler>().FromComponentsInHierarchy().AsCached();

        Container.Bind<Notification>().FromInstance(_notification).AsSingle();
    }
}