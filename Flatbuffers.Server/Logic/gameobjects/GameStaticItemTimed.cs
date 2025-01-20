namespace Game.Logic;

public class GameStaticItemTimed : GameStaticItem
{
	protected uint m_removeDelay = 120000; //Currently 2 mins
	protected RemoveItemAction m_removeItemAction;

	public GameStaticItemTimed() : base()
	{
	}
	public GameStaticItemTimed(uint vanishTicks): this()
	{
		if(vanishTicks > 0)
			m_removeDelay = vanishTicks;
	}
	public uint RemoveDelay
	{
		get 
		{
			return m_removeDelay;
		}
		set
		{
			if(value>0)
				m_removeDelay=value;
			if(m_removeItemAction.IsAlive)
				m_removeItemAction.Start((int)m_removeDelay);
		}
	}
	public override void Delete()
	{
		if (m_removeItemAction != null)
		{
			m_removeItemAction.Stop();
			m_removeItemAction = null;
		}
		base.Delete ();
	}
	public override bool AddToWorld()
	{
		if(!base.AddToWorld()) return false;
		if (m_removeItemAction == null)
			m_removeItemAction = new RemoveItemAction(this);
		m_removeItemAction.Start((int)m_removeDelay);
		return true;
	}
	protected class RemoveItemAction : RegionAction
	{
		public RemoveItemAction(GameStaticItemTimed item) : base(item)
		{
		}
		protected override void OnTick()
		{
			GameStaticItem item = (GameStaticItem)m_actionSource;
			item.Delete();
		}
	}
}