using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEditor;
using UnityEngine;
using Zenject;

[ExecuteInEditMode]
public class ConvoyPlacer : MonoBehaviour
{
    [SerializeField, Range(1, 10)] private int _slotCount = 3; // Количество слотов
    [SerializeField] private float _slotSpacing = 5f; // Расстояние между слотами
    [SerializeField] private SlotTrigger _slotPrefab;
    [SerializeField] private Transform _cameraTarget;
    [SerializeField] private bool _isLeftSide;
    public bool IsLeftSide => _isLeftSide;
    public IReadOnlyReactiveCollection<SlotTrigger> Slots => _slots;
    private ReactiveCollection<SlotTrigger> _slots = new ReactiveCollection<SlotTrigger>();
    
    [Inject] private ConvoySystem _convoySystem;
    [Inject] private UnitPark _unitPark;
    private void OnValidate()
    {
        if (!Application.isPlaying)
        {
           // EditorApplication.delayCall += UpdateSlots;
        }
    }

    private void Start()
    {
        if (Application.isPlaying)
        {
            UpdateSlots();
        }
    }

    private void UpdateSlots()
    {
        if (_slotPrefab == null)
        {
            Debug.LogError("Slot Prefab is not assigned in ConvoyPlace!");
            return;
        }
        _slots.Clear();
        List<Transform> children = new List<Transform>();
        foreach (Transform child in transform)
        {
            children.Add(child);
        }

        foreach (var child in children)
        {
            if (Application.isPlaying)
            {
                Destroy(child.gameObject);
            }
            else
            {
                DestroyImmediate(child.gameObject, true);
            }
        }

        for (int i = 0; i < _slotCount; i++)
        {
            SlotTrigger slot = Instantiate(_slotPrefab, transform);

            slot.Initialize(i, _convoySystem, _unitPark);

            slot.transform.localPosition = Vector3.back * i * _slotSpacing;
            slot.name = $"Slot{i}";
            _slots.Add(slot);
        }
    }

    public Transform GetCameraTarget()
    {
        return _cameraTarget;
    }
    public SlotTrigger[] GetSlots()
    {
        return _slots.ToArray();
    }
}