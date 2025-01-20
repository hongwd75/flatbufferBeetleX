using Game.Logic.datatable;
using Game.Logic.Inventory;
using Game.Logic.World.Timer;
using Logic.database.table;
using NetworkMessage;

namespace Game.Logic;

public class WorldInventoryItem : GameStaticItemTimed
	{
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		
		private InventoryItem m_item;

		private bool m_isRemoved = false;

        public override LanguageDataObject.eTranslationIdentifier TranslationIdentifier
        {
            get { return LanguageDataObject.eTranslationIdentifier.eItem; }
        }

		public WorldInventoryItem() : base(ServerProperties.Properties.WORLD_ITEM_DECAY_TIME)
		{
		}

		public WorldInventoryItem(InventoryItem item) : this()
		{
			m_item = item;
			m_item.SlotPosition = 0;
			m_item.OwnerID = null;
			m_item.AllowAdd = true;
			this.Level = (byte)item.Level;
			this.Model = (ushort)item.Model;
			this.Emblem = item.Emblem;
			this.Name = item.Name;

			if (item.Template is ItemUnique && item.Template.IsPersisted == false)
			{
				GameServer.Database.AddObject(item.Template as ItemUnique);
			}
		}

		public static WorldInventoryItem CreateFromTemplate(InventoryItem item)
		{
			if (item.Template is ItemUnique)
				return null;
			
			return CreateFromTemplate(item.Id_nb);
		}
		
		public static WorldInventoryItem CreateFromTemplate(string templateID)
		{
			ItemTemplate template = GameServer.Database.FindObjectByKey<ItemTemplate>(templateID);
			if (template == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Item Creation: Template not found!\n"+Environment.StackTrace);
				return null;
			}

			return CreateFromTemplate(template);
		}

		public static WorldInventoryItem CreateUniqueFromTemplate(string templateID)
		{
			ItemTemplate template = GameServer.Database.FindObjectByKey<ItemTemplate>(templateID);

			if (template == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Item Creation: Template not found!\n" + Environment.StackTrace);
				return null;
			}
			
			return CreateUniqueFromTemplate(template);
		}

		public static WorldInventoryItem CreateFromTemplate(ItemTemplate template)
		{
			if (template == null)
				return null;

			WorldInventoryItem invItem = new WorldInventoryItem();

			invItem.m_item = GameInventoryItem.Create(template);
			
			invItem.m_item.SlotPosition = 0;
			invItem.m_item.OwnerID = null;

			invItem.Level = (byte)template.Level;
			invItem.Model = (ushort)template.Model;
			invItem.Emblem = template.Emblem;
			invItem.Name = template.Name;

			return invItem;
		}

		public static WorldInventoryItem CreateUniqueFromTemplate(ItemTemplate template)
		{
			if (template == null)
				return null;

			WorldInventoryItem invItem = new WorldInventoryItem();
			ItemUnique item = new ItemUnique(template);
			GameServer.Database.AddObject(item);

			invItem.m_item = GameInventoryItem.Create(item);
			
			invItem.m_item.SlotPosition = 0;
			invItem.m_item.OwnerID = null;

			invItem.Level = (byte)template.Level;
			invItem.Model = (ushort)template.Model;
			invItem.Emblem = template.Emblem;
			invItem.Name = template.Name;

			return invItem;
		}

		public override bool RemoveFromWorld()
		{
			if (base.RemoveFromWorld())
			{
				if (m_item is IGameInventoryItem)
				{
					(m_item as IGameInventoryItem).OnRemoveFromWorld();
				}

				m_isRemoved = true;
				return true;
			}

			return false;
		}

		public override void Delete()
		{
			if (m_item != null && m_isRemoved == false && m_item.Template is ItemUnique)
			{
				// for world items that expire we need to delete the associated ItemUnique
				GameServer.Database.DeleteObject(m_item.Template as ItemUnique);
			}

			base.Delete();
		}

		#region PickUpTimer
		private RegionTimer m_pickup;
		
		public void StartPickupTimer(int time)
		{
			if (m_pickup != null)
			{
				m_pickup.Stop();
				m_pickup = null;
			}
			m_pickup = new RegionTimer(this, new RegionTimerCallback(CallBack), time * 1000);
		}

		private int CallBack(RegionTimer timer)
		{
			m_pickup.Stop();
			m_pickup = null;
			return 0;
		}

		public void StopPickupTimer()
		{
			foreach (GamePlayer player in Owners)
			{
				if (player.ObjectState == eObjectState.Active)
				{
					player.Out.SendMessage("You may now pick up " + Name + "!", eChatType.CT_Loot, eChatLoc.CL_SystemWindow);
				}
			}
			m_pickup.Stop();
			m_pickup = null;
		}

		public int GetPickupTime
		{
			get
			{
				if (m_pickup == null)
					return 0;
				return m_pickup.TimeUntilElapsed;
			}
		}
		#endregion

		public InventoryItem Item
		{
			get
			{
				return m_item;
			}
		}
	}