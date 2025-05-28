using System;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class RepairView : MonoBehaviour, IView
{
    [SerializeField] private TextMeshProUGUI _materialsText;
    [SerializeField] private Button _backButton;
    [SerializeField] private Transform _unitCardsContainer;
    [SerializeField] private GameObject _unitCardPrefab;
    [SerializeField] private Notification _errorMessage;

    private RepairPresenter _repairPresenter;
    
    public Transform UnitCardsContainer => _unitCardsContainer;
    public GameObject UnitCardPrefab => _unitCardPrefab;

    public void SetPresenter(RepairPresenter presenter)
    {
        _repairPresenter = presenter;
    }

    private void Start()
    {
        Hide();
        _backButton.OnClickAsObservable()
            .Subscribe(_ => Hide())
            .AddTo(this);
    }

    public void UpdateMaterialsText(int materials)
    {
        _materialsText.text = $"{materials}";
    }

    public void ShowErrorMessage(string message)
    {
        if (_errorMessage != null)
        {
            _errorMessage.ShowNotification(Notification.NotificationType.Error, message);
        }
    }

    
    public void Show()
    {
        gameObject.SetActive(true);
        _repairPresenter.ShowPanel();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}