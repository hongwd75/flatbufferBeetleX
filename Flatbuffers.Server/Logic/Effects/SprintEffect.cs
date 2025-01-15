using Game.Logic.Language;

namespace Game.Logic.Effects;

public sealed class SprintEffect : StaticEffect, IGameEffect
{
	public override void Start(GameLiving target)
	{
		base.Start(target);
		target.StartEnduranceRegeneration();
	}
	
	public override void Cancel(bool playerCancel)
	{
		base.Cancel(playerCancel);
		if (m_owner is GamePlayer)
			(m_owner as GamePlayer).Sprint(false);
	}
	
	public override string Name { get { return LanguageMgr.GetTranslation(((GamePlayer)Owner).Network, "Effects.SprintEffect.Name"); } }
	
	public override int RemainingTime { get { return 1000; } } // always 1 for blink effect

	public override ushort Icon { get { return 0x199; } }
}