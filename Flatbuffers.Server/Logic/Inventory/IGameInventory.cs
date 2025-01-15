using Logic.database.table;

namespace Game.Logic.Inventory;

public enum eUseType
{
    clic = 0,
    use1 = 1,
    use2 = 2,
}

public interface IGameInventory
{
    bool LoadFromDatabase(string inventoryID);
    bool SaveIntoDatabase(string inventoryID);

    bool AddItem(eInventorySlot slot, InventoryItem item);
    bool AddTradeItem(eInventorySlot slot, InventoryItem item);

    bool AddCountToStack(InventoryItem item, int count);
    bool AddTemplate(InventoryItem template, int count, eInventorySlot minSlot, eInventorySlot maxSlot);
    bool RemoveItem(InventoryItem item);

    bool RemoveTradeItem(InventoryItem item);

    bool RemoveCountFromStack(InventoryItem item, int count);
    bool RemoveTemplate(string templateID, int count, eInventorySlot minSlot, eInventorySlot maxSlot);
    bool MoveItem(eInventorySlot fromSlot, eInventorySlot toSlot, int itemCount);
    InventoryItem GetItem(eInventorySlot slot);
    ICollection<InventoryItem> GetItemRange(eInventorySlot minSlot, eInventorySlot maxSlot);

    void BeginChanges();
    void CommitChanges();
    void ClearInventory();

    int CountSlots(bool countUsed, eInventorySlot minSlot, eInventorySlot maxSlot);
    int CountItemTemplate(string itemtemplateID, eInventorySlot minSlot, eInventorySlot maxSlot);
    bool IsSlotsFree(int count, eInventorySlot minSlot, eInventorySlot maxSlot);

    eInventorySlot FindFirstEmptySlot(eInventorySlot first, eInventorySlot last);
    eInventorySlot FindLastEmptySlot(eInventorySlot first, eInventorySlot last);
    eInventorySlot FindFirstFullSlot(eInventorySlot first, eInventorySlot last);
    eInventorySlot FindLastFullSlot(eInventorySlot first, eInventorySlot last);

    InventoryItem GetFirstItemByID(string uniqueID, eInventorySlot minSlot, eInventorySlot maxSlot);
    InventoryItem GetFirstItemByObjectType(int objectType, eInventorySlot minSlot, eInventorySlot maxSlot);
    InventoryItem GetFirstItemByName(string name, eInventorySlot minSlot, eInventorySlot maxSlot);

    ICollection<InventoryItem> VisibleItems { get; }
    ICollection<InventoryItem> EquippedItems { get; }
    ICollection<InventoryItem> AllItems { get; }
    int Count { get; }

    int InventoryWeight { get; }
}