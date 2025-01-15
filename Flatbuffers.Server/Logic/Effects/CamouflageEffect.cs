using Game.Logic.Language;
using NetworkMessage;

namespace Game.Logic.Effects;

public class CamouflageEffect : StaticEffect, IGameEffect
{
		
    public override void Start(GameLiving target)
    {
        base.Start(target);
        if (target is GamePlayer)
            (target as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((target as GamePlayer).Network, "Effects.CamouflageEffect.YouAreCamouflaged"), eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
    }

    public override void Stop()
    {
        base.Stop();
        if (m_owner is GamePlayer)
            (m_owner as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((m_owner as GamePlayer).Network, "Effects.CamouflageEffect.YourCFIsGone"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
    }
		
    public override string Name
    {
        get { return LanguageMgr.GetTranslation(((GamePlayer)Owner).Network, "Effects.CamouflageEffect.Name"); }
    }
		
    public override ushort Icon
    {
        get { return 476; }
    }
		
    /// <summary>
    /// Delve Info
    /// </summary>
    public override IList<string> DelveInfo
    {
        get
        {
            var delveInfoList = new List<string>();
            delveInfoList.Add(LanguageMgr.GetTranslation(((GamePlayer)Owner).Network, "Effects.CamouflageEffect.InfoEffect"));

            return delveInfoList;
        }
    }
}