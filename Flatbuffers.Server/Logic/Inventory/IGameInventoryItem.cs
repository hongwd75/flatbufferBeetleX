﻿using Logic.database.table;

namespace Game.Logic.Inventory;

public interface IGameInventoryItem
{
	/// <summary>
	/// Is this item valid?
	/// </summary>
	/// <param name="player"></param>
	/// <returns></returns>
	bool CheckValid(GamePlayer player);
	/// <summary>
	/// Can this player equip this item
	/// </summary>
	/// <param name="player"></param>
	/// <returns></returns>
	bool CanEquip(GamePlayer player);
	/// <summary>
	/// This item is received by the player
	/// </summary>
	/// <param name="player"></param>
	void OnReceive(GamePlayer player);
	/// <summary>
	/// The player loses this item
	/// </summary>
	/// <param name="player"></param>
	void OnLose(GamePlayer player);
	/// <summary>
	/// The player is dropping this item into the world
	/// </summary>
	/// <param name="player"></param>
	/// <returns></returns>
	WorldInventoryItem Drop(GamePlayer player);
	/// <summary>
	/// This item is being removed from the world
	/// </summary>
	void OnRemoveFromWorld();
	/// <summary>
	/// This item is being equipped
	/// </summary>
	/// <param name="player"></param>
	void OnEquipped(GamePlayer player);
	/// <summary>
	/// This item is being un-equipped
	/// </summary>
	/// <param name="player"></param>
	void OnUnEquipped(GamePlayer player);
	/// <summary>
	/// This item has struck a target
	/// </summary>
	/// <param name="owner"></param>
	/// <param name="target"></param>
	void OnStrikeTarget(GameLiving owner, GameObject target);
	/// <summary>
	/// This item has been struck by an enemy
	/// </summary>
	/// <param name="owner"></param>
	/// <param name="enemy"></param>
	void OnStruckByEnemy(GameLiving owner, GameLiving enemy);
	/// <summary>
	/// The player is attempting to use this item
	/// </summary>
	/// <param name="player"></param>
	/// <returns>true if use is handled here</returns>
	bool Use(GamePlayer player);
	/// <summary>
	/// The player is attempting to combine this item with the target item
	/// </summary>
	/// <param name="player"></param>
	/// <param name="targetItem"></param>
	/// <returns>true if combine is handled here</returns>
	bool Combine(GamePlayer player, InventoryItem targetItem);
	/// <summary>
	/// Player is delving this item
	/// </summary>
	/// <param name="delve"></param>
	/// <param name="player"></param>
	void Delve(List<String> delve, GamePlayer player);
}