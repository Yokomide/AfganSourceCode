using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using DG.Tweening;
using UniRx;

public class RepairUnitCard : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _unitNameText; 
    [SerializeField] private TextMeshProUGUI _durabilityText;
    [SerializeField] private TextMeshProUGUI _crewText;
    [SerializeField] private TextMeshProUGUI _speedText;
    [SerializeField] private TextMeshProUGUI _attackText;
    [SerializeField] private TextMeshProUGUI _repairCostText; 
    [SerializeField] private Button _repairButton;
    [SerializeField] private Slider _durabilitySlider;
    [SerializeField] private Image _unitImage; 

    private UnitModel _unit;
    private Action<UnitModel> _onRepairClicked; 
    private Func<float, int> _calculateRepairCost;

    private float _colorTransitionDuration = 0.1f;

    public UnitModel Unit => _unit;

    public void Initialize(UnitModel unit, Action<UnitModel> onRepairClicked, Func<float, int> calculateRepairCost)
    {
        _unit = unit;
        _onRepairClicked = onRepairClicked;
        _calculateRepairCost = calculateRepairCost;

        _unitNameText.text = unit.Name.ToString();
        Refresh();

        string imagePath = $"Images/Units/{unit.Type}";
        Sprite unitSprite = Resources.Load<Sprite>(imagePath);
        if (unitSprite != null)
        {
            _unitImage.sprite = unitSprite;
        }
        else
        {
            Debug.LogWarning($"Sprite not found at path: {imagePath}");
            _unitImage.sprite = null;
        }

        _durabilitySlider.maxValue = unit.MaxHealth;
        _repairButton.onClick.RemoveAllListeners();
        _repairButton.onClick.AddListener(OnRepairButtonClicked);
        _unit.Durability.Subscribe(_ => Refresh()).AddTo(this);
    }

    private void OnRepairButtonClicked()
    {
        _onRepairClicked?.Invoke(_unit);
        Refresh();
    }

    public void Refresh()
    {
        int repairCost = _calculateRepairCost(_unit.Durability.Value);
        _durabilityText.text = $"{_unit.Durability.Value * 100:F0}/100";
        _crewText.text = $"{_unit.Crew} чел.";
        _speedText.text = $"{_unit.Speed} км/ч";
        _attackText.text = $"{_unit.Damage} ед.";
        _durabilitySlider.DOValue(_unit.Health.Value, 0.1f);
        _repairCostText.text = repairCost.ToString();
        _repairButton.gameObject.SetActive(repairCost > 0);
    }
}