﻿using System.Collections;
using Game.Logic.Effects;
using Game.Logic.RealmAblilities;
using Game.Logic.World;
using Logic.database.table;
using NetworkMessage;

namespace Game.Logic.Skills;

public class SpeedOfSoundAbility : TimedRealmAbility
{
	public SpeedOfSoundAbility(DBAbility dba, int level) : base(dba, level) { }

	int m_range = 2000;
	int m_duration = 1;

	public override void Execute(GameLiving living)
	{
		if (CheckPreconditions(living, DEAD | SITTING | MEZZED | STUNNED)) return;
		GamePlayer player = living as GamePlayer;

		if (player.TempProperties.getProperty("Charging", false)
			|| player.EffectList.CountOfType(typeof(SpeedOfSoundEffect), typeof(ChargeEffect)) > 0)
		{
			player.Out.SendMessage("You already an effect of that type!", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
			return;
		}

		switch (Level)
		{
			case 1: m_duration = 10000; break;
			case 2: m_duration = 30000; break;
			case 3: m_duration = 60000; break;
			default: return;
		}

		DisableSkill(living);

		ArrayList targets = new ArrayList();

		targets.Add(player);

		bool success;
		foreach (GamePlayer target in targets)
		{
			//send spelleffect
			success = target.EffectList.CountOfType<SpeedOfSoundEffect>() == 0;
			foreach (GamePlayer visPlayer in target.GetPlayersInRadius(WorldManager.VISIBILITY_DISTANCE))
				visPlayer.Out.SendSpellEffectAnimation(player, target, 7021, 0, false, CastSuccess(success));
			if (success)
			{
				GameSpellEffect speed = Spells.SpellHandler.FindEffectOnTarget(target, "SpeedEnhancement");
				if (speed != null)
					speed.Cancel(false);
				new SpeedOfSoundEffect(m_duration).Start(target);
			}
		}

	}
	private byte CastSuccess(bool suc)
	{
		if (suc)
			return 1;
		else
			return 0;
	}
	public override int GetReUseDelay(int level)
	{
		return 600;
	}
}