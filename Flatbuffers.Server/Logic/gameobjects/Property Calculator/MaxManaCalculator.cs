namespace Game.Logic.PropertyCalc;

[PropertyCalculator(eProperty.MaxMana)]
public class MaxManaCalculator : PropertyCalculator
{
	public MaxManaCalculator() {}

	public override int CalcValue(GameLiving living, eProperty property) 
	{
		if (living is GamePlayer) 
		{
			GamePlayer player = living as GamePlayer;
			eStat manaStat = player.CharacterClass.ManaStat;

			if (player.CharacterClass.ManaStat == eStat.UNDEFINED)
			{
				return 0;
			}

			int manaBase = player.CalculateMaxMana(player.Level, player.GetModified((eProperty)manaStat));
			int itemBonus = living.GetBuffBonus(eBuffBonusType.ItemBonus)[(int)property];
			int poolBonus = living.GetBuffBonus(eBuffBonusType.ItemBonus)[(int)eProperty.PowerPool];
			int abilityBonus = living.GetBuffBonus(eBuffBonusType.AbilityBonus)[(int)property]; 

			int itemCap = player.Level / 2 + 1;
			int poolCap = player.Level / 2;
			itemCap = itemCap + Math.Min(player.GetBuffBonus(eBuffBonusType.ItemBonus)[(int)eProperty.PowerPoolCapBonus], itemCap);
			poolCap = poolCap + Math.Min(player.GetBuffBonus(eBuffBonusType.ItemBonus)[(int)eProperty.PowerPoolCapBonus], player.Level);


			if (itemBonus > itemCap) {
				itemBonus = itemCap;
			}
			if (poolBonus > poolCap)
				poolBonus = poolCap;

			return (int)(manaBase + itemBonus + abilityBonus + (manaBase + itemBonus + abilityBonus) * poolBonus * 0.01); 
		}
		else 
		{
			return 1000000;	// default
		}
	}
}