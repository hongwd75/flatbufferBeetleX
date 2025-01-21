using Game.Logic.Language;
using NetworkMessage;

namespace Game.Logic.Effects;

public class QuickCastEffect : StaticEffect, IGameEffect
{
    public override void Start(GameLiving living)
    {
        base.Start(living);
        if (m_owner is GamePlayer player)
        {
            player.Out.SendMessage(
                LanguageMgr.GetTranslation((m_owner as GamePlayer).Network, "Effects.QuickCastEffect.YouActivatedQC"),
                eChatType.CT_System, eChatLoc.CL_SystemWindow);
        }
        m_owner.TempProperties.removeProperty(Spells.SpellHandler.INTERRUPT_TIMEOUT_PROPERTY);
    }
    
    public override void Cancel(bool playerCancel)
    {
        base.Cancel(playerCancel);
        if (m_owner is GamePlayer player)
        {
            player.Out.SendMessage(
                LanguageMgr.GetTranslation((m_owner as GamePlayer).Network,
                    "Effects.QuickCastEffect.YourNextSpellNoQCed"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
        }
    }
    public override string Name { get { return LanguageMgr.GetTranslation(((GamePlayer)Owner).Network, "Effects.QuickCastEffect.Name"); } }
    public override int RemainingTime { get { return 0; } }
    public override ushort Icon { get { return 0x0190; } }
    public override IList<string> DelveInfo
    {
        get
        {
            var delveInfoList = new List<string>();
            delveInfoList.Add(LanguageMgr.GetTranslation(((GamePlayer)Owner).Network, "Effects.Quickcast.DelveInfo"));

            return delveInfoList;
        }
    }
}