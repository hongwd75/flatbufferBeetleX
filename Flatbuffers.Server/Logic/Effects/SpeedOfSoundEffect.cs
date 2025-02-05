using Game.Logic.Events;
using Game.Logic.PropertyCalc;

namespace Game.Logic.Effects;

public class SpeedOfSoundEffect : TimedEffect, IGameEffect
{
	public SpeedOfSoundEffect(int duration)
		: base(duration)
	{ }

	GameEventHandler m_attackFinished = new GameEventHandler(AttackFinished);

	public override void Start(GameLiving living)
	{
		base.Start(living);
		living.TempProperties.setProperty("Charging", true);
		GameEventManager.AddHandler(living, GameLivingEvent.AttackFinished, m_attackFinished);
		GameEventManager.AddHandler(living, GameLivingEvent.CastFinished, m_attackFinished);
		living.BuffBonusMultCategory.Set((int)eProperty.MaxSpeed, this, PropertyCalc.MaxSpeedCalculator.SPEED4);		
		if (living is GamePlayer)
			(living as GamePlayer).Out.SendUpdateMaxSpeed();
	}
	
	private static void AttackFinished(GameEvent e, object sender, EventArgs args)
	{
		GamePlayer player = (GamePlayer)sender;
		if (e == GameLivingEvent.CastFinished)
		{
			CastingEventArgs cfea = args as CastingEventArgs;

			if (cfea.SpellHandler.Caster != player)
				return;

			//cancel if the effectowner casts a non-positive spell
			if (!cfea.SpellHandler.HasPositiveEffect)
			{
				SpeedOfSoundEffect effect = player.EffectList.GetOfType<SpeedOfSoundEffect>();
				if (effect != null)
					effect.Cancel(false);
			}
		}
		else if (e == GameLivingEvent.AttackFinished)
		{
			AttackFinishedEventArgs afargs = args as AttackFinishedEventArgs;
			if (afargs == null)
				return;

			if (afargs.AttackData.Attacker != player)
				return;

			switch (afargs.AttackData.AttackResult)
			{
				case GameLiving.eAttackResult.HitStyle:
				case GameLiving.eAttackResult.HitUnstyled:
				case GameLiving.eAttackResult.Blocked:
				case GameLiving.eAttackResult.Evaded:
				case GameLiving.eAttackResult.Fumbled:
				case GameLiving.eAttackResult.Missed:
				case GameLiving.eAttackResult.Parried:
					SpeedOfSoundEffect effect = player.EffectList.GetOfType<SpeedOfSoundEffect>();
					if (effect != null)
						effect.Cancel(false);
					break;
			}
		}
	}

	public override void Stop()
	{
		base.Stop();
		m_owner.TempProperties.removeProperty("Charging");
		m_owner.BuffBonusMultCategory.Remove((int)eProperty.MaxSpeed, this);
		if (m_owner is GamePlayer)
			(m_owner as GamePlayer).Out.SendUpdateMaxSpeed();
		GameEventManager.RemoveHandler(m_owner, GameLivingEvent.AttackFinished, m_attackFinished);
		GameEventManager.RemoveHandler(m_owner, GameLivingEvent.CastFinished, m_attackFinished);
	}

	public override string Name
	{
		get
		{
			return "Speed of Sound";
		}
	}

	public override UInt16 Icon
	{
		get
		{
			return 3020;
		}
	}
	
	public override IList<string> DelveInfo
	{
		get
		{
			var delveInfoList = new List<string>();
			delveInfoList.Add("Gives immunity to stun/snare/root and mesmerize spells and provides unbreakeable speed.");
			delveInfoList.Add(" ");

			int seconds = (int)(RemainingTime / 1000);
			if (seconds > 0)
			{
				delveInfoList.Add(" ");
				delveInfoList.Add("- " + seconds + " seconds remaining.");
			}

			return delveInfoList;
		}
	}
}