using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[RequireComponent(typeof(Slider))]
public class HealthBarColorUpdater : MonoBehaviour
{
    [SerializeField] private Image _fillImage;       
    [SerializeField] private float _colorTransitionDuration = 0.1f; 

    private Slider _slider;
    
    private readonly Color _maxValueColor = Color.green;   // 100% - Зелёный
    private readonly Color _highValueColor = Color.yellow; // 75% - Жёлтый
    private readonly Color _midValueColor = new Color(1f, 0.5f, 0f); // 50% - Оранжевый
    private readonly Color _minValueColor = Color.red;     // 0% - Красный

    private void Awake()
    {
        _slider = GetComponent<Slider>();
        if (_fillImage == null)
        {
            Debug.LogError("Fill Image not assigned in HealthBarColorUpdater");
            enabled = false;
            return;
        }
        UpdateColor(_slider.value);
    }

    private void OnEnable()
    {
        _slider.onValueChanged.AddListener(UpdateColor);
    }

    private void OnDisable()
    {
        _slider.onValueChanged.RemoveListener(UpdateColor);
    }

    private void UpdateColor(float value)
    {
        if (_fillImage == null) return;
        
        float percentage = Mathf.InverseLerp(_slider.minValue, _slider.maxValue, value);
        
        Color targetColor;
        if (percentage >= 0.75f)
        {
            // От 75% до 100%: Зелёный → Жёлтый
            targetColor = Color.Lerp(_highValueColor, _maxValueColor, (percentage - 0.75f) / 0.25f);
        }
        else if (percentage >= 0.5f)
        {
            // От 50% до 75%: Жёлтый → Оранжевый
            targetColor = Color.Lerp(_midValueColor, _highValueColor, (percentage - 0.5f) / 0.25f);
        }
        else if (percentage >= 0.25f)
        {
            // От 25% до 50%: Оранжевый → Красный
            targetColor = Color.Lerp(_minValueColor, _midValueColor, (percentage - 0.25f) / 0.25f);
        }
        else
        {
            // От 0% до 25%: Красный
            targetColor = Color.Lerp(_minValueColor, _minValueColor, percentage / 0.25f); 
        }
        
        _fillImage.DOColor(targetColor, _colorTransitionDuration);
    }
}