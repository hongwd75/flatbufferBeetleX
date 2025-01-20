using Logic.database.table;

namespace Game.Logic.Events;

public class ItemDroppedEventArgs : EventArgs
{
    private InventoryItem m_sourceItem;
    private WorldInventoryItem m_groundItem;

    public ItemDroppedEventArgs(InventoryItem sourceItem, WorldInventoryItem groundItem)
    {
        m_sourceItem = sourceItem;
        m_groundItem = groundItem;
    }
    
    public InventoryItem SourceItem
    {
        get { return m_sourceItem; }
    }
    
    public WorldInventoryItem GroundItem
    {
        get { return m_groundItem; }
    }
}