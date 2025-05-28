using System;
using TMPro;
using UniRx;
using UnityEngine;
using Zenject;

public class PreparationView : MonoBehaviour, IView
{
    public IObservable<UnitDragData> OnUnitDragged => _onUnitDragged;
    public IObservable<Unit> OnStartMissionClicked => _onStartMissionClicked;

    private readonly Subject<UnitDragData> _onUnitDragged = new();
    private readonly Subject<Unit> _onStartMissionClicked = new();

    [Inject] private UnitPark _unitPark;
    [Inject] private PreparationPresenter _preparationPresenter;

    [SerializeField] private Transform _inventoryPanel;
    [SerializeField] private DragHandler _dragPrefab; 
    
    [SerializeField] private TextMeshProUGUI _convoySpeedTextValue;
    [SerializeField] private TextMeshProUGUI _convoyDamageTextValue;

    [SerializeField] private MissionInfoHandler _missionInfoHandler;
    
    [Inject] private Notification _errorMessage;

    [Inject] private DiContainer _diContainer;

    private void Start()
    {
        PopulateInventory();
    }

    public void PopulateInventory()
    {
        _unitPark.ReloadUnits();
        foreach (Transform child in _inventoryPanel)
        {
            Destroy(child.gameObject);
        }
        foreach (var unit in _unitPark.AvailableUnits)
        {
            Debug.Log("Durability populate: " + unit.Durability.Value);

            var dragButton = _diContainer.InstantiatePrefab(_dragPrefab, _inventoryPanel);
            var dragHandler = dragButton.GetComponent<DragHandler>();
            dragHandler.SetUnitModel(unit);
            _unitPark.RegisterButton(unit, dragHandler);
        }
    }

    public void HandleUnitDropped(UnitController unitController, int slotIndex)
    {
        Debug.Log("HandleUnitDropped");
        _onUnitDragged.OnNext(new UnitDragData(unitController, slotIndex));
    }

    public void ShowErrorMessage(string message)
    {
        if (_errorMessage != null)
        {
            _errorMessage.ShowNotification(Notification.NotificationType.Error, message);
        }
    }

    public void SetMissionInfo(string title, string info)
    {
        _missionInfoHandler.SetInfo(title, info);
    }
    public void HandleStartMissionButton()
    {
        Debug.Log("PlayButtonPressed");
        _onStartMissionClicked.OnNext(Unit.Default);
    }

    public void UpdateConvoyStats(float speed, int damage, int cargo)
    {
        if (_convoySpeedTextValue != null)
        {
            _convoySpeedTextValue.text = $"{speed:F1}";
        }
        if (_convoyDamageTextValue != null)
        {
            _convoyDamageTextValue.text = $"{damage}";
        }

    }
    
    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}