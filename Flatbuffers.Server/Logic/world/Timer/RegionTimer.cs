using System.Data;
using System.Text;

namespace Game.Logic.World.Timer;
public delegate int RegionTimerCallback(RegionTimer callingTimer);
public sealed class RegionTimer : GameTimer
	{
		/// <summary>
		/// The timer callback
		/// </summary>
		private RegionTimerCallback m_callback;

		/// <summary>
		/// Holds properties for this region timer
		/// </summary>
		private PropertyCollection m_properties;

		private GameObject m_owner;

		/// <summary>
		/// The timer owner
		/// </summary>
		public GameObject Owner
		{
			get { return m_owner; }
		}

		/// <summary>
		/// Gets or sets the timer callback
		/// </summary>
		public RegionTimerCallback Callback
		{
			get { return m_callback; }
			set { m_callback = value; }
		}

		/// <summary>
		/// Gets the properties of this timer
		/// </summary>
		public PropertyCollection Properties
		{
			get
			{
				if (m_properties == null)
				{
					lock (this)
					{
						if (m_properties == null)
						{
							PropertyCollection properties = new PropertyCollection();
							Thread.MemoryBarrier();
							m_properties = properties;
						}
					}
				}
				return m_properties;
			}
		}

		/// <summary>
		/// Constructs a new region timer
		/// </summary>
		/// <param name="timerOwner">The game object that is starting the timer</param>
		public RegionTimer(GameObject timerOwner)
			: base(timerOwner.CurrentRegion.TimeManager)
		{
			m_owner = timerOwner;
		}

		/// <summary>
		/// Constructs a new region timer
		/// </summary>
		/// <param name="timerOwner">The game object that is starting the timer</param>
		/// <param name="callback">The callback function to call</param>
		public RegionTimer(GameObject timerOwner, RegionTimerCallback callback)
			: this(timerOwner)
		{
			m_owner = timerOwner;
			m_callback = callback;
		}

		/// <summary>
		/// Constructs a new region timer and starts it with specified delay
		/// </summary>
		/// <param name="timerOwner">The game object that is starting the timer</param>
		/// <param name="callback">The callback function to call</param>
		/// <param name="delay">The interval in milliseconds when to call the callback (>0)</param>
		public RegionTimer(GameObject timerOwner, RegionTimerCallback callback, int delay)
			: this(timerOwner, callback)
		{
			m_owner = timerOwner;
			Start(delay);
		}

		/// <summary>
		/// Constructs a new region timer
		/// </summary>
		/// <param name="time"></param>
		public RegionTimer(TimeManager time)
			: base(time)
		{
		}

		/// <summary>
		/// Called on every timer tick
		/// </summary>
		protected override void OnTick()
		{
			if (m_callback != null)
				Interval = m_callback(this);
		}

		/// <summary>
		/// Returns short information about the timer
		/// </summary>
		/// <returns>Short info about the timer</returns>
		public override string ToString()
		{
			RegionTimerCallback callback = m_callback;
			object target = null;
			if (callback != null)
				target = callback.Target;
			return new StringBuilder(128)
				.Append("callback: ").Append(callback == null ? "(null)" : callback.Method.Name)
				.Append(' ').Append(base.ToString())
				.Append(" target: ").Append(target == null ? "" : (target.GetType().FullName + " "))
				.Append('(').Append(target == null ? "null" : target.ToString())
				.Append(')')
				.ToString();
		}
	}