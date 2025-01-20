using Game.Logic.Inventory;
using Logic.database.table;

namespace Game.Logic.Events;

public class ItemEquippedArgs : EventArgs
{
    private InventoryItem m_item;
    private eInventorySlot m_previousSlotPosition;

    public ItemEquippedArgs(InventoryItem item, eInventorySlot previousSlotPosition)
    {
        m_item = item;
        m_previousSlotPosition = previousSlotPosition;
    }

    public ItemEquippedArgs(InventoryItem item, int previousSlotPosition)
    {
        m_item = item;
        m_previousSlotPosition = (eInventorySlot)previousSlotPosition;
    }

    public InventoryItem Item
    {
        get { return m_item; }
    }

    public eInventorySlot PreviousSlotPosition
    {
        get { return m_previousSlotPosition; }
    }
}