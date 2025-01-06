using System.Reflection;
using Game.Logic.World.Timer;
using log4net;

namespace Game.Logic.AI.Brain
{
    public abstract class APlayerVicinityBrain : ABrain
    {
        /// <summary>
        /// Defines a logger for this class.
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// constructor of this brain
        /// </summary>
        public APlayerVicinityBrain() : base()
        {
        }

        /// <summary>
        /// Returns the string representation of the APlayerVicinityBrain
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return base.ToString() + ", noPlayersStopCountdown=" + noPlayersStopCountdown.ToString();
        }

        /// <summary>
        /// The number of ticks this brain stays active while no player
        /// is in the vicinity.
        /// </summary>
        protected int noPlayersStopCountdown;

        /// <summary>
        /// The number of milliseconds this brain will stay active even when no player is close
        /// This abstract class always returns 45 Seconds
        /// </summary>
        protected virtual int NoPlayersStopDelay
        {
            set
            {
            }
            get { return 45000; }
        }

        /// <summary>
        /// Starts the brain thinking and resets the inactivity countdown
        /// </summary>
        /// <returns>true if started</returns>
        public override bool Start()
        {
            if (!Body.IsVisibleToPlayers)
                return false;
            Interlocked.Exchange(ref noPlayersStopCountdown, (int)(NoPlayersStopDelay/ThinkInterval));
            return base.Start();
        }

        /// <summary>
        /// Called whenever the brain should do some thinking.
        /// We check if there is at least one player around and nothing
        /// bad has happened. If so, we shutdown our brain.
        /// </summary>
        /// <param name="callingTimer"></param>
        protected override int BrainTimerCallback(RegionTimer callingTimer)
        {
            if (Interlocked.Decrement(ref noPlayersStopCountdown) <= 0)
            {
                //Stop the brain timer
                Stop();
                return 0;
            }
            return base.BrainTimerCallback(callingTimer);
        }
    }   
}