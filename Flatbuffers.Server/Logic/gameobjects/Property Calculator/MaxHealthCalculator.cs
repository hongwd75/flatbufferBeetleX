using Game.Logic.Skills;

namespace Game.Logic.PropertyCalc;

[PropertyCalculator(eProperty.MaxHealth)]
public class MaxHealthCalculator : PropertyCalculator
{
	public override int CalcValue(GameLiving living, eProperty property)
	{
		if (living is GamePlayer)
		{
			GamePlayer player = living as GamePlayer;
			int hpBase = player.CalculateMaxHealth(player.Level, player.GetModified(eProperty.Constitution));
			int buffBonus = living.GetBuffBonus(eBuffBonusType.BaseBuff)[(int)property];
			if (buffBonus < 0) buffBonus = (int)((1 + (buffBonus / -100.0)) * hpBase)-hpBase;
			int itemBonus = living.GetBuffBonus(eBuffBonusType.ItemBonus)[(int)property];
			int cap = Math.Max(player.Level * 4, 20) + // at least 20
					  Math.Min(living.GetBuffBonus(eBuffBonusType.ItemBonus)[(int)eProperty.MaxHealthCapBonus], player.Level * 4);	
			itemBonus = Math.Min(itemBonus, cap);
            if (player.HasAbility(Abilities.ScarsOfBattle) && player.Level >= 40)
            {
                int levelbonus = Math.Min(player.Level - 40, 10);
                hpBase = (int)(hpBase * (100 + levelbonus) * 0.01);
            }
			int abilityBonus = living.GetBuffBonus(eBuffBonusType.AbilityBonus)[(int)property];

			return Math.Max(hpBase + itemBonus + buffBonus + abilityBonus, 1); // at least 1
		}
		else if (living is GameNPC)
		{
			int hp = 0;

			if (living.Level<10)
			{
				hp = living.Level * 20 + 20 + living.GetBuffBonus(eBuffBonusType.BaseBuff)[(int)property];	// default
			}
			else
			{
				hp = (int)(50 + 11*living.Level + 0.548331 * living.Level * living.Level) + living.GetBuffBonus(eBuffBonusType.BaseBuff)[(int)property];
				if (living.Level < 25)
					hp += 20;
			}

			int basecon = (living as GameNPC).Constitution;
			int conmod = 20; // at level 50 +75 con ~= +300 hit points
			

			int conhp = hp + (conmod * living.Level * (living.GetModified(eProperty.Constitution) - basecon) / 250);

			// 50% buff / debuff cap
			if (conhp > hp * 1.5)
				conhp = (int)(hp * 1.5);
			else if (conhp < hp / 2)
				conhp = hp / 2;

			return conhp;
		}
        else
        {
            if (living.Level < 10)
            {
                return living.Level * 20 + 20 + living.GetBuffBonus(eBuffBonusType.BaseBuff)[(int)property];	// default
            }
            else
            {
	            int hp = (int)(50 + 11 * living.Level + 0.548331 * living.Level * living.Level) + living.GetBuffBonus(eBuffBonusType.BaseBuff)[(int)property];
                if (living.Level < 25)
                {
                    hp += 20;
                }
                return hp;
            }
        }
	}
	public static int GetItemBonusCap(GameLiving living)
    {
        if (living == null) return 0;
        return living.Level * 4;
    }
    public static int GetItemBonusCapIncrease(GameLiving living)
    {
        if (living == null) return 0;
        int itemBonusCapIncreaseCap = GetItemBonusCapIncreaseCap(living);
        int itemBonusCapIncrease = living.GetBuffBonus(eBuffBonusType.ItemBonus)[(int)(eProperty.MaxHealthCapBonus)];
        return Math.Min(itemBonusCapIncrease, itemBonusCapIncreaseCap);
    }
    public static int GetItemBonusCapIncreaseCap(GameLiving living)
    {
        if (living == null) return 0;
        return living.Level * 4;
    }
}