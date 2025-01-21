using Game.Logic.AI.Brain;
using Game.Logic.Effects;
using Game.Logic.Skills;
using Game.Logic.Spells;
using NetworkMessage;

namespace Game.Logic.PropertyCalc;

[PropertyCalculator(eProperty.MaxSpeed)]
public class MaxSpeedCalculator : PropertyCalculator
{
	public static readonly double SPEED1 = 1.753;
	public static readonly double SPEED2 = 1.816;
	public static readonly double SPEED3 = 1.91;
	public static readonly double SPEED4 = 1.989;
	public static readonly double SPEED5 = 2.068;

	public override int CalcValue(GameLiving living, eProperty property)
	{
		if (living.IsMezzed || living.IsStunned) return 0;

		double speed = living.BuffBonusMultCategory.Get((int)property);

		if (living is GamePlayer)
		{
			GamePlayer player = (GamePlayer)living;
			
			if (player.IsStealthed)
			{
				GameSpellEffect bloodrage = SpellHandler.FindEffectOnTarget(player, "BloodRage");

				double stealthSpec = player.GetModifiedSpecLevel(Specs.Stealth);
				if (stealthSpec > player.Level)
					stealthSpec = player.Level;
				speed *= 0.3 + (stealthSpec + 10) * 0.3 / (player.Level + 10);
				if (bloodrage != null)
					speed *= 1 + (bloodrage.Spell.Value * 0.01); // 25 * 0.01 = 0.25 (a.k 25%) value should be 25.
				
			}

			if (player.IsSprinting)
			{
				speed *= 1.3;
			}
		}
		else if (living is GameNPC npc)
		{
            if(npc.Brain is IControlledBrain brain)
            {
                var owner = brain.GetPlayerOwner();
                if(owner != null && owner.IsSprinting)
                {
                    speed *= 1.29;
                }
            }

			double healthPercent = npc.Health / (double)npc.MaxHealth;
			if (healthPercent < 0.33)
			{
				speed *= 0.2 + healthPercent * (0.8 / 0.33); //33%hp=full speed 0%hp=20%speed
			}
		}

		speed = living.MaxSpeedBase * speed + 0.5; // 0.5 is to fix the rounding error when converting to int so root results in speed 2 (191*0.01=1.91+0.5=2.41)

		GameSpellEffect iConvokerEffect = SpellHandler.FindEffectOnTarget(living, "SpeedWrap");
		if (iConvokerEffect != null)
		{
			if (living.EffectList.GetOfType<SprintEffect>() != null && speed > 248)
			{
				return 248;
			}
			else if (speed > 191)
			{
				return 191;
			}
		}

		if (speed < 0)
			return 0;

		return (int)speed;
	}
}