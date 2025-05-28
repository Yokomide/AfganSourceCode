public struct UnitDragData
{
    public UnitController DragUnitController { get; }
    public int SlotIndex { get; }

    public UnitDragData(UnitController dragUnit, int slotIndex)
    {
        DragUnitController = dragUnit;
        SlotIndex = slotIndex;
    }
}