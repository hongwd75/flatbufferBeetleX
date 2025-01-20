namespace Game.Logic;

public abstract class GameMovingObject : GameNPC
{
    public GameMovingObject() : base()
    {
    }

    public virtual ushort Type()
    {
        return 2;
    }

    private ushort m_emblem;
    public ushort Emblem
    {
        get { return m_emblem; }
        set { m_emblem = value; }
    }

    public override ushort Model
    {
        get
        {
            return base.Model;
        }
        set
        {
            base.Model = value;
            if(ObjectState==eObjectState.Active)
                BroadcastUpdate();
        }
    }
    public override bool IsWorthReward
    {
        get {return false;}
    }

    public override void TakeDamage(GameObject source, eDamageType damageType, int damageAmount, int criticalAmount)
    {
        if (IsAlive)
        {
            Health -= (damageAmount + criticalAmount);
            if (!IsAlive)
            {
                Health = 0;
                Die(source);
            }
        }
    }

    public override void StartPowerRegeneration()
    {
        //No regeneration for moving objects
        return;
    }

    public override void StartEnduranceRegeneration()
    {
        //No regeneration for moving objects
        return;
    }
}