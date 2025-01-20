namespace Game.Logic.Events;

public class PlayerInventoryEvent : GameEvent
{
    public PlayerInventoryEvent(string name) : base(name)
    {
    }

    public static readonly PlayerInventoryEvent ItemEquipped = new PlayerInventoryEvent("PlayerInventory.ItemEquipped");
    public static readonly PlayerInventoryEvent ItemUnequipped = new PlayerInventoryEvent("PlayerInventory.ItemUnequipped");
    public static readonly PlayerInventoryEvent ItemDropped = new PlayerInventoryEvent("PlayerInventory.ItemDropped");
    public static readonly PlayerInventoryEvent ItemBonusChanged = new PlayerInventoryEvent("PlayerInventory.ItemBonusChanged");
}