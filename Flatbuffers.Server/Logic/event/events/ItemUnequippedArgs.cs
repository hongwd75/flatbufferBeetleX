using Game.Logic.Inventory;
using Logic.database.table;

namespace Game.Logic.Events;

public class ItemUnequippedArgs : EventArgs
{
    private InventoryItem m_item;
    private eInventorySlot m_previousSlotPos;

    public ItemUnequippedArgs(InventoryItem item, eInventorySlot previousSlotPos)
    {
        m_item = item;
        m_previousSlotPos = previousSlotPos;
    }

    public ItemUnequippedArgs(InventoryItem item, int previousSlotPos)
    {
        m_item = item;
        m_previousSlotPos = (eInventorySlot)previousSlotPos;
    }

    public InventoryItem Item
    {
        get { return m_item; }
    }

    public eInventorySlot PreviousSlotPosition
    {
        get { return m_previousSlotPos; }
    }
}