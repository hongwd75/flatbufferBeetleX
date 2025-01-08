﻿using Game.Logic.AI.Brain;
using Game.Logic.Effects;

namespace Game.Logic;

public class AttackAction : RegionAction
{
	/// <summary>
	/// Constructs a new attack action
	/// </summary>
	/// <param name="owner">The action source</param>
	public AttackAction(GameLiving owner)
		: base(owner)
	{
	}

	/// <summary>
	/// Called on every timer tick
	/// </summary>
	protected override void OnTick()
	{
		GameLiving owner = (GameLiving)m_actionSource;

		if (owner.IsMezzed || owner.IsStunned)
		{
			Interval = 100;
			return;
		}

		if (owner.IsCasting && !owner.CurrentSpellHandler.Spell.Uninterruptible)
		{
			Interval = 100;
			return;
		}

		if (!owner.AttackState)
		{
			AttackData ad = owner.TempProperties.getProperty<object>(LAST_ATTACK_DATA, null) as AttackData;
			owner.TempProperties.removeProperty(LAST_ATTACK_DATA);
			if (ad != null && ad.Target != null)
				ad.Target.RemoveAttacker(owner);
			Stop();
			return;
		}
		
		// Store all datas which must not change during the attack
		// double effectiveness = 1.0;
		double effectiveness = owner.Effectiveness;
		int ticksToTarget = 1;
		int interruptDuration = 0;
		int leftHandSwingCount = 0;
		Style combatStyle = null;
		InventoryItem attackWeapon = owner.AttackWeapon;
		InventoryItem leftWeapon = (owner.Inventory == null) ? null : owner.Inventory.GetItem(eInventorySlot.LeftHandWeapon);
		GameObject attackTarget = null;

		if (owner.ActiveWeaponSlot == GameLiving.eActiveWeaponSlot.Distance)
		{
			attackTarget = owner.RangeAttackTarget; // must be do here because RangeAttackTarget is changed in CheckRangeAttackState
			eCheckRangeAttackStateResult rangeCheckresult = owner.CheckRangeAttackState(attackTarget);
			if (rangeCheckresult == eCheckRangeAttackStateResult.Hold)
			{
				Interval = 100;
				return; //Hold the shot another second
			}
			else if (rangeCheckresult == eCheckRangeAttackStateResult.Stop || attackTarget == null)
			{
				owner.StopAttack(); //Stop the attack
				Stop();
				return;
			}

			int model = (attackWeapon == null ? 0 : attackWeapon.Model);
			foreach (GamePlayer player in owner.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
			{
				if (player == null) continue;
				player.Out.SendCombatAnimation(owner, attackTarget, (ushort)model, 0x00, player.Out.BowShoot, 0x01, 0, ((GameLiving)attackTarget).HealthPercent);
			}

			interruptDuration = owner.AttackSpeed(attackWeapon);

			switch (owner.RangedAttackType)
			{
				case eRangedAttackType.Critical:
					{
						effectiveness *= 2 - 0.3 * owner.GetConLevel(attackTarget);
						if (effectiveness > 2)
							effectiveness *= 2;
						else if (effectiveness < 1.1)
							effectiveness *= 1.1;
					}
					break;

				case eRangedAttackType.SureShot:
					{
						effectiveness *= 0.5;
					}
					break;

				case eRangedAttackType.RapidFire:
					{
						// Source : http://www.camelotherald.com/more/888.shtml
						// - (About Rapid Fire) If you release the shot 75% through the normal timer, the shot (if it hits) does 75% of its normal damage. If you
						// release 50% through the timer, you do 50% of the damage, and so forth - The faster the shot, the less damage it does.

						// Source : http://www.camelotherald.com/more/901.shtml
						// Related note about Rapid Fire interrupts are determined by the speed of the bow is fired, meaning that the time of interruptions for each shot will be scaled
						// down proportionally to bow speed. If that made your eyes bleed, here's an example from someone who would know: "I fire a 5.0 spd bow. Because I am buffed and have
						// stat bonuses, I fire that bow at 3.0 seconds. The resulting interrupt on the caster will last 3.0 seconds. If I rapid fire that same bow, I will fire at 1.5 seconds,
						// and the resulting interrupt will last 1.5 seconds."

						long rapidFireMaxDuration = owner.AttackSpeed(attackWeapon) / 2; // half of the total time
						long elapsedTime = owner.CurrentRegion.Time - owner.TempProperties.getProperty<long>(GamePlayer.RANGE_ATTACK_HOLD_START); // elapsed time before ready to fire
						if (elapsedTime < rapidFireMaxDuration)
						{
							effectiveness *= 0.5 + (double)elapsedTime * 0.5 / (double)rapidFireMaxDuration;
							interruptDuration = (int)(interruptDuration * effectiveness);
						}
					}
					break;
			}

			// calculate Penetrating Arrow damage reduction
			if (attackTarget is GameLiving)
			{
				int PALevel = owner.GetAbilityLevel(Abilities.PenetratingArrow);
				if ((PALevel > 0) && (owner.RangedAttackType != eRangedAttackType.Long))
				{
					GameSpellEffect bladeturn = null;
					lock (((GameLiving)attackTarget).EffectList)
					{
						foreach (IGameEffect effect in ((GameLiving)attackTarget).EffectList)
						{
							if (effect is GameSpellEffect && ((GameSpellEffect)effect).Spell.SpellType == "Bladeturn")
							{
								bladeturn = (GameSpellEffect)effect;
								break;
							}
						}
					}

					if (bladeturn != null && attackTarget != bladeturn.SpellHandler.Caster)
					{
						// Penetrating Arrow work
						effectiveness *= 0.25 + PALevel * 0.25;
					}
				}
			}

			ticksToTarget = 1 + owner.GetDistanceTo( attackTarget ) * 100 / 150; // 150 units per 1/10s
		}
		else
		{
			attackTarget = owner.TargetObject;

			// wait until target is selected
			if (attackTarget == null || attackTarget == owner)
			{
				Interval = 100;
				return;
			}

			AttackData ad = owner.TempProperties.getProperty<object>(LAST_ATTACK_DATA, null) as AttackData;
			if (ad != null && ad.AttackResult == eAttackResult.Fumbled)
			{
				Interval = owner.AttackSpeed(attackWeapon);
				ad.AttackResult = eAttackResult.Missed;
				return; //Don't start the attack if the last one fumbled
			}

			combatStyle = owner.GetStyleToUse();
			if (combatStyle != null && combatStyle.WeaponTypeRequirement == (int)eObjectType.Shield)
			{
				attackWeapon = leftWeapon;
			}
			interruptDuration = owner.AttackSpeed(attackWeapon);

			// Damage is doubled on sitting players
			// but only with melee weapons; arrows and magic does normal damage.
			if (attackTarget is GamePlayer && ((GamePlayer)attackTarget).IsSitting)
			{
				effectiveness *= 2;
			}

			ticksToTarget = 1;
		}

		if (attackTarget != null && !owner.IsWithinRadius(attackTarget, owner.AttackRange) && owner.ActiveWeaponSlot != GameLiving.eActiveWeaponSlot.Distance)
		{
			if (owner is GameNPC && (owner as GameNPC).Brain is StandardMobBrain && ((owner as GameNPC).Brain as StandardMobBrain).AggroTable.Count > 0 && (owner as GameNPC).Brain is IControlledBrain == false)
			{
				#region Attack another target in range

				GameNPC npc = owner as GameNPC;
				StandardMobBrain npc_brain = npc.Brain as StandardMobBrain;
				GameLiving Possibly_target = null;
				long maxaggro = 0, aggro = 0;

				foreach (GamePlayer player_test in owner.GetPlayersInRadius((ushort)owner.AttackRange))
				{
					if (npc_brain.AggroTable.ContainsKey(player_test))
					{
						aggro = npc_brain.GetAggroAmountForLiving(player_test);
						if (aggro <= 0) continue;
						if (aggro > maxaggro)
						{
							Possibly_target = player_test;
							maxaggro = aggro;
						}
					}
				}
				foreach (GameNPC target_possibility in owner.GetNPCsInRadius((ushort)owner.AttackRange))
				{
					if (npc_brain.AggroTable.ContainsKey(target_possibility))
					{
						aggro = npc_brain.GetAggroAmountForLiving(target_possibility);
						if (aggro <= 0) continue;
						if (aggro > maxaggro)
						{
							Possibly_target = target_possibility;
							maxaggro = aggro;
						}
					}
				}

				if (Possibly_target == null)
				{
					Interval = 100;
					return;
				}
				else
				{
					attackTarget = Possibly_target;
				}

				#endregion

			}
			else
			{
				Interval = 100;
				return;
			}
		}

		new WeaponOnTargetAction(owner, attackTarget, attackWeapon, leftWeapon, effectiveness, interruptDuration, combatStyle).Start(ticksToTarget);  // really start the attack

		//Are we inactive?
		if (owner.ObjectState != GameObject.eObjectState.Active)
		{
			Stop();
			return;
		}

		//switch to melee if range to target is less than 200
		if (owner is GameNPC && owner.ActiveWeaponSlot == GameLiving.eActiveWeaponSlot.Distance && owner.TargetObject != null && owner.IsWithinRadius( owner.TargetObject, 200 ) )
		{
			owner.SwitchWeapon(GameLiving.eActiveWeaponSlot.Standard);
		}

		if (owner.ActiveWeaponSlot == GameLiving.eActiveWeaponSlot.Distance)
		{
			//Mobs always shot and reload
			if (owner is GameNPC)
			{
				owner.RangedAttackState = eRangedAttackState.AimFireReload;
			}

			if (owner.RangedAttackState != eRangedAttackState.AimFireReload)
			{
				owner.StopAttack();
				Stop();
				return;
			}
			else
			{
				if (!(owner is GamePlayer) || (owner.RangedAttackType != eRangedAttackType.Long))
				{
					owner.RangedAttackType = eRangedAttackType.Normal;
					lock (owner.EffectList)
					{
						foreach (IGameEffect effect in owner.EffectList) // switch to the correct range attack type
						{
							if (effect is SureShotEffect)
							{
								owner.RangedAttackType = eRangedAttackType.SureShot;
								break;
							}
							else if (effect is RapidFireEffect)
							{
								owner.RangedAttackType = eRangedAttackType.RapidFire;
								break;
							}
							else if (effect is TrueshotEffect)
							{
								owner.RangedAttackType = eRangedAttackType.Long;
								break;
							}
						}
					}
				}

				owner.RangedAttackState = eRangedAttackState.Aim;

				if (owner is GamePlayer)
				{
					owner.TempProperties.setProperty(GamePlayer.RANGE_ATTACK_HOLD_START, 0L);
				}

				int speed = owner.AttackSpeed(attackWeapon);
				byte attackSpeed = (byte)(speed / 100);
				int model = (attackWeapon == null ? 0 : attackWeapon.Model);
				foreach (GamePlayer player in owner.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
				{
					player.Out.SendCombatAnimation(owner, null, (ushort)model, 0x00, player.Out.BowPrepare, attackSpeed, 0x00, 0x00);
				}

				if (owner.RangedAttackType == eRangedAttackType.RapidFire)
				{
					speed /= 2; // can start fire at the middle of the normal time
				}

				Interval = speed;
			}
		}
		else
		{
			if (leftHandSwingCount > 0)
			{
				Interval = owner.AttackSpeed(attackWeapon, leftWeapon);
			}
			else
			{
				Interval = owner.AttackSpeed(attackWeapon);
			}
		}
	}
}