using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

public class DragHandler : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    [Inject] private UnitFactory _unitFactory;
    [Inject] private UnitPark _unitPark;
    [Inject] private PreparationView _preparationView;

    [SerializeField] private Slider _healthSlider;
    [SerializeField] private Image _unitImage;
    [SerializeField] private TextMeshProUGUI _unitName;

    private UnitModel _unitModel;
    private UnitController _draggedUnit;
    private Camera _mainCamera;
    private int _currentSlotIndex = -1;
    private LayerMask _groundLayer;

    public static DragHandler _activeDragHandler;

    [Inject] private ConvoyPlacer _convoyPlacer;

    private void Awake()
    {
        _mainCamera = Camera.main;
        _groundLayer = LayerMask.GetMask("Ground");
        _activeDragHandler = null;
        gameObject.SetActive(true);
    }

    public void SetUnitModel(UnitModel model)
    {
        _unitModel = model;
        
        string imagePath = $"Images/Units/{model.Type}";
        _unitImage.sprite = Resources.Load<Sprite>(imagePath);
        _unitName.text = model.Name;
        _healthSlider.maxValue = _unitModel.MaxHealth;
        _healthSlider.value = _unitModel.Health.Value;
    }

    public void UpdateHealthInfo()
    {
        _healthSlider.value = _unitModel.Health.Value;
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_unitModel == null)
        {
            Debug.LogWarning("No unit available in park!");
            return;
        }
        _draggedUnit = _unitFactory.Create(_unitPark.TakeUnit(_unitModel));
        if(_convoyPlacer.IsLeftSide)
            _draggedUnit.transform.eulerAngles = new Vector3(0, 180, 0);
        _draggedUnit.transform.localScale = Vector3.zero;
        _draggedUnit.transform.DOScale(Vector3.one, 0.3f);
        _currentSlotIndex = -1;
        _activeDragHandler = this;

        UpdatePosition(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_draggedUnit == null) return;

        UpdatePosition(eventData);
    }

    private void UpdatePosition(PointerEventData eventData)
    {
        Ray ray = _mainCamera.ScreenPointToRay(eventData.position);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, _groundLayer))
        {
            _draggedUnit.transform.position = hit.point + new Vector3(0, 2, 0);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (_draggedUnit == null) return;

        if (_currentSlotIndex >= 0)
        {
            var slotPosition = _convoyPlacer.Slots[_currentSlotIndex].transform.position;
            _preparationView.HandleUnitDropped(_draggedUnit, _currentSlotIndex);
            gameObject.SetActive(false);
        }
        else
        {
            _unitPark.ReturnUnit(_draggedUnit.Model);
            Destroy(_draggedUnit.gameObject);
        }
        _draggedUnit = null;
        _activeDragHandler = null;
    }

    public void SetSlotIndex(int slotIndex)
    {
        _currentSlotIndex = slotIndex;
    }
}