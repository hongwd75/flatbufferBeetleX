using System.Reflection;
using System.Text;
using Game.Logic.Events;
using Game.Logic.World.Timer;
using log4net;

namespace Game.Logic.AI.Brain;

public abstract class ABrain
	{
		private readonly object m_LockObject = new object(); // dummy object for locking - Mannen. // use this object for locking, instead of locking on 'this'

		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		// /// <summary>
		// /// Action queue
		// /// </summary>
		// protected Stack m_actions = new Stack();

		/// <summary>
		/// The body of this brain
		/// </summary>
		protected GameNPC m_body;

		/// <summary>
		/// The timer used to check for player proximity
		/// </summary>
		private RegionTimer m_brainTimer;

		/// <summary>
		/// Constructs a new brain for a body
		/// </summary>
		public ABrain()
		{
		}

		/// <summary>
		/// Returns the string representation of the ABrain
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return new StringBuilder(32)
				.Append("body name='").Append(Body==null?"(null)":Body.Name)
				.Append("' (id=").Append(Body==null?"(null)":Body.ObjectID.ToString())
				.Append("), active=").Append(IsActive)
				.Append(", ThinkInterval=").Append(ThinkInterval)
				.ToString();
		}

		/// <summary>
		/// Gets/sets the body of this brain
		/// </summary>
		public GameNPC Body
		{
			get { return m_body; }
			set { m_body = value; }
		}

		/// <summary>
		/// Returns weather this brain is active or not
		/// </summary>
		public virtual bool IsActive
		{
			get { return m_brainTimer != null && m_brainTimer.IsAlive; }
		}

		/// <summary>
		/// The interval at which the brain will fire, in milliseconds
		/// </summary>
		public virtual int ThinkInterval
		{
			get { return 2500; }
			set {}
		}

		/// <summary>
		/// How fast can this brain cast, in milliseconds
		/// </summary>
		public virtual int CastInterval
		{
			get { return ThinkInterval; }
			set { }
		}

		/// <summary>
		/// Starts the brain thinking
		/// </summary>
		/// <returns>true if started</returns>
		public virtual bool Start()
		{
			//Do not start brain if we are dead or inactive
			if (!m_body.IsAlive || m_body.ObjectState != GameObject.eObjectState.Active)
				return false;
			
			lock (m_LockObject)
			{
				if (IsActive) return false;

				m_brainTimer = new RegionTimer(m_body);
				m_brainTimer.Callback = new RegionTimerCallback(BrainTimerCallback);
				m_brainTimer.Start(ThinkInterval);
			}
			return true;
		}

		/// <summary>
		/// Stops the brain thinking
		/// </summary>
		/// <returns>true if stopped</returns>
		public virtual bool Stop()
		{
			lock (m_LockObject)
			{
				if(!IsActive) return false;
				m_brainTimer.Stop();
				m_brainTimer = null;
			}
			return true;
		}

		/// <summary>
		/// The callback timer for the brain ticks
		/// </summary>
		/// <param name="callingTimer">the calling timer</param>
		/// <returns>the new tick intervall</returns>
		protected virtual int BrainTimerCallback(RegionTimer callingTimer)
		{
			if(!m_body.IsAlive || m_body.ObjectState!=GameObject.eObjectState.Active)
			{
				//Stop the brain for dead or inactive bodies
				Stop();
				return 0;
			}

			Think();
			GameEventManager.Notify(GameNPCEvent.OnAICallback, m_body);
			return ThinkInterval;
		}

		/// <summary>
		/// Receives all messages of the body
		/// </summary>
		/// <param name="e">The event received</param>
		/// <param name="sender">The event sender</param>
		/// <param name="args">The event arguments</param>
		public virtual void Notify(GameEvent e, object sender, EventArgs args)
		{
		}

		/// <summary>
		/// This method is called whenever the brain does some thinking
		/// </summary>
		public abstract void Think();
	}