using UnityEngine;
using UnityEngine.EventSystems;

public class SlotHandler : MonoBehaviour, IDropHandler
{
    public int SlotIndex { get; private set; }

    private void Awake()
    {
        SlotIndex = transform.GetSiblingIndex();
    }

    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log($"Dropped into slot {SlotIndex}");
    }
}