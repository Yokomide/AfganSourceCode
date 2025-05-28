using System;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class PreparationRepairView : MonoBehaviour, IView
{
    [SerializeField] private TextMeshProUGUI _materialsText;
    [SerializeField] private TextMeshProUGUI _repairCostText;
    [SerializeField] private Button _repairButton;

    private PreparationRepairPresenter _repairPresenter;

    [Inject] private Notification _errorMessage;

    private void Start()
    {
        _repairPresenter.SetRepairCost();
        _repairButton.onClick.AddListener(_repairPresenter.OnRepairButtonClicked);
        _repairPresenter.UpdateMaterials();
        if(!_repairPresenter.CheckNeedOfRepair())
            Hide();
    }

    public void SetPresenter(PreparationRepairPresenter presenter)
    {
        _repairPresenter = presenter;
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

    public void SetRepairCost(int cost)
    {
        _repairCostText.text = cost.ToString();
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