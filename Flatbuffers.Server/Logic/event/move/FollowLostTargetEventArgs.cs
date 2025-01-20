namespace Game.Logic.Events;

public class FollowLostTargetEventArgs : EventArgs
{
    private readonly GameObject m_lostTarget;
    public FollowLostTargetEventArgs(GameObject lostTarget)
    {
        m_lostTarget = lostTarget;
    }
    public GameObject LostTarget
    {
        get { return m_lostTarget; }
    }
}