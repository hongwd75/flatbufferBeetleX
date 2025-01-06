namespace Game.Logic.PropertyCalc;

	[PropertyCalculator(eProperty.Stat_First, eProperty.Stat_Last)]
	public class StatCalculator : PropertyCalculator
    {
        public StatCalculator() { }

        public override int CalcValue(GameLiving living, eProperty property)
        {
            int propertyIndex = (int)property;

            int baseStat = living.GetBaseStat((eStat)property);
            int abilityBonus = living.GetBuffBonus(eBuffBonusType.AbilityBonus)[propertyIndex];
            int debuff = living.GetBuffBonus(eBuffBonusType.Debuff)[propertyIndex];
			int deathConDebuff = 0;

            int itemBonus = CalcValueFromItems(living, property);
            int buffBonus = CalcValueFromBuffs(living, property);

			int unbuffedBonus = baseStat + itemBonus;
			buffBonus -= Math.Abs(debuff);

			if (living is GamePlayer && buffBonus < 0)
			{
				unbuffedBonus += buffBonus / 2;
				buffBonus = 0;
			}

			int stat = unbuffedBonus + buffBonus + abilityBonus;
			stat = (int)(stat * living.BuffBonusMultCategory.Get((int)property));

			stat -= (property == eProperty.Constitution)? deathConDebuff : 0;

			return Math.Max(1, stat);
        }

        public override int CalcValueFromBuffs(GameLiving living, eProperty property)
        {
            if (living == null)
                return 0;

            int propertyIndex = (int)property;
            int baseBuffBonus = living.GetBuffBonus(eBuffBonusType.BaseBuff)[propertyIndex];
            int specBuffBonus = living.GetBuffBonus(eBuffBonusType.SpecBuff)[propertyIndex];

            int baseBuffBonusCap = (living is GamePlayer) ? (int)(living.Level * 1.25) : Int16.MaxValue;
            int specBuffBonusCap = (living is GamePlayer) ? (int)(living.Level * 1.5 * 1.25) : Int16.MaxValue;

            baseBuffBonus = Math.Min(baseBuffBonus, baseBuffBonusCap);
            specBuffBonus = Math.Min(specBuffBonus, specBuffBonusCap);

            return baseBuffBonus + specBuffBonus;
        }

        public override int CalcValueFromItems(GameLiving living, eProperty property)
        {
            if (living == null)
                return 0;

            int itemBonus = living.GetBuffBonus(eBuffBonusType.ItemBonus)[(int)property];
            int itemBonusCap = GetItemBonusCap(living, property);
            
            int itemBonusCapIncrease = GetItemBonusCapIncrease(living, property);
            return Math.Min(itemBonus, itemBonusCap + itemBonusCapIncrease);
        }

        public static int GetItemBonusCap(GameLiving living, eProperty property)
        {
            if (living == null) return 0;
            return (int) (living.Level * 1.5);
        }

        public static int GetItemBonusCapIncrease(GameLiving living, eProperty property)
        {
            if (living == null) return 0;
            int itemBonusCapIncreaseCap = GetItemBonusCapIncreaseCap(living);
            int itemBonusCapIncrease = living.GetBuffBonus(eBuffBonusType.ItemBonus)[(int)(eProperty.StatCapBonus_First - eProperty.Stat_First + property)];

            return Math.Min(itemBonusCapIncrease, itemBonusCapIncreaseCap);
        }

        public static int GetItemBonusCapIncreaseCap(GameLiving living)
        {
            if (living == null) return 0;
            return living.Level / 2 + 1;
        }
    }