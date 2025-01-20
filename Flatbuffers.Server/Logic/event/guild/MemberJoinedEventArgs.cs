namespace Game.Logic.Events.guild;

public class MemberJoinedEventArgs : EventArgs
{
    private GameLiving m_member;

    public MemberJoinedEventArgs(GameLiving living)
    {
        m_member = living;
    }
    public GameLiving Member
    {
        get { return m_member; }
    }
}