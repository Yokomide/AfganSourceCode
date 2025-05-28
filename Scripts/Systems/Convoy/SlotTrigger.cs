using System;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class SlotTrigger : MonoBehaviour
{
    [SerializeField] private int _slotIndex;
    [SerializeField] private Transform _placePoint;
    [SerializeField] private SpriteRenderer _sprite;
    [SerializeField] private Color _defaultColor;
    [SerializeField] private Color _selectedColor;
    [SerializeField] private Color _occupiedColor;
    [SerializeField] private ParticleSystem _placeVFX;
    [SerializeField] private ParticleSystem _removeVFX;
    [SerializeField] private AudioClip _placeSFX;
    [SerializeField] private AudioClip _removeSFX;
    [SerializeField] private AudioSource _audioSource;
    
    [Inject] private ConvoySystem _convoySystem;
    [Inject] private UnitPark _unitPark;
    private UnitController _triggeredUnit;
    private Collider _collider;
    private bool _occupied;

    public Transform PlacePoint => _placePoint;
    public int SlotIndex => _slotIndex;

    private void Awake()
    {
        _collider = GetComponent<Collider>();
        SetColor(SlotState.Free);
    }

    public void Initialize(int index, ConvoySystem convoySystem, UnitPark unitPark)
    {
        _slotIndex = index;
        _convoySystem = convoySystem;
        _unitPark = unitPark;
    }

    private void OnMouseOver()
    {
        SetColor(SlotState.Selected);
        if (DragHandler._activeDragHandler != null && _collider.enabled)
        {
            DragHandler._activeDragHandler.SetSlotIndex(SlotIndex);
        }

        if (Input.GetMouseButtonDown(0) && _collider.enabled && _convoySystem.Convoy[SlotIndex] != null )
        {
            Debug.Log("ConvoySystem");
            _unitPark.ReturnUnit(_convoySystem.Convoy[SlotIndex].Model);
            _convoySystem.RemoveUnit(SlotIndex);
        }
    }
    private void OnMouseExit()
    {
        if (_occupied)
        {
            SetColor(SlotState.Occupied);
            return;
        }
        SetColor(SlotState.Free);
        if (DragHandler._activeDragHandler != null && _collider.enabled)
        {
            DragHandler._activeDragHandler.SetSlotIndex(-1);
        }
    }
    public void SetSlotOccupied(bool value)
    {
        _occupied = value;
        if (_occupied)
        {
            _placeVFX.Play();
            _audioSource.PlayOneShot(_placeSFX);
            SetColor(SlotState.Occupied);
            return; 
        }
        _removeVFX.Play();
        _audioSource.PlayOneShot(_removeSFX);

    }



    private void SetColor(SlotState slotState)
    {
        switch (slotState)
        {
            case SlotState.Free:
                _sprite.color = _defaultColor;
                break;
            case SlotState.Selected:
                _sprite.color = _selectedColor;
                break;
            case SlotState.Occupied:
                _sprite.color = _occupiedColor;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(slotState), slotState, null);
        }
    }
    public void Deactivate()
    {
        _collider.enabled = false;
        gameObject.SetActive(false);
    }
    
    public enum SlotState
    {
        Free,
        Selected,
        Occupied
    }
}