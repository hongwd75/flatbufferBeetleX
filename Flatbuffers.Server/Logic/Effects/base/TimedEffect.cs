using Game.Logic.Language;
using Game.Logic.World.Timer;

namespace Game.Logic.Effects;

public class TimedEffect : StaticEffect
{
    private readonly object m_LockObject = new object();
    protected int m_duration;
    protected RegionTimer m_expireTimer;
    
    public TimedEffect(int timespan)
    {
        m_duration = timespan;
    }

    public override void Start(GameLiving target)
    {
        lock (m_LockObject)
        {
            if (m_expireTimer == null)
            {
                m_expireTimer = new RegionTimer(target, new RegionTimerCallback(ExpiredCallback), m_duration);
            }
            base.Start(target);
        }
    }

    public override void Stop()
    {
        lock (m_LockObject)
        {
            if (m_expireTimer != null)
            {
                m_expireTimer.Stop();
                m_expireTimer = null;
            }
            base.Stop();
        }
    }

    private int ExpiredCallback(RegionTimer timer)
    {
        Stop();
        return 0;
    }

    public override int RemainingTime
    {
        get
        {
            RegionTimer timer = m_expireTimer;
            if (timer == null || !timer.IsAlive)
                return 0;
            return timer.TimeUntilElapsed;
        }
    }

    public override IList<string> DelveInfo
    {
        get
        {
            var list = new List<string>();

            int seconds = RemainingTime / 1000;
            if (seconds > 0)
            {
                list.Add(" "); //empty line
                if (seconds > 60)
                    list.Add(LanguageMgr.GetTranslation(((GamePlayer)Owner).Network, "Effects.DelveInfo.MinutesRemaining", (seconds / 60), (seconds % 60).ToString("00")));
                else
                    list.Add(LanguageMgr.GetTranslation(((GamePlayer)Owner).Network, "Effects.DelveInfo.SecondsRemaining", seconds));
            }
            return list;
        }
    }
}