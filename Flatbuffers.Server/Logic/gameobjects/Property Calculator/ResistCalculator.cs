using Game.Logic.Skills;

namespace Game.Logic.PropertyCalc;

[PropertyCalculator(eProperty.Resist_First, eProperty.Resist_Last)]
public class ResistCalculator : PropertyCalculator
{
	public ResistCalculator() { }

    public override int CalcValue(GameLiving living, eProperty property)
    {
        int propertyIndex = (int)property;

        int debuff = Math.Abs(living.GetBuffBonus(eBuffBonusType.Debuff)[propertyIndex]);
		int abilityBonus = living.GetBuffBonus(eBuffBonusType.AbilityBonus)[propertyIndex];
		int racialBonus = SkillBase.GetRaceResist( living.Race, (eResist)property );

        int itemBonus = CalcValueFromItems(living, property);
        int buffBonus = CalcValueFromBuffs(living, property);

        switch (property)
        {
            case eProperty.Resist_Body:
            case eProperty.Resist_Cold:
            case eProperty.Resist_Energy:
            case eProperty.Resist_Heat:
            case eProperty.Resist_Matter:
            case eProperty.Resist_Natural:
            case eProperty.Resist_Spirit:
                debuff += Math.Abs(living.GetBuffBonus(eBuffBonusType.Debuff)[eProperty.MagicAbsorption]);
                abilityBonus += living.GetBuffBonus(eBuffBonusType.AbilityBonus)[eProperty.MagicAbsorption];
                buffBonus += living.GetBuffBonus(eBuffBonusType.BaseBuff)[eProperty.MagicAbsorption];
                break;
        }

        if (living is GameNPC)
        {
            double constitutionPerMagicAbsorptionPercent = 8;
            var constitutionBuffBonus = living.GetBuffBonus(eBuffBonusType.BaseBuff)[eProperty.Constitution] + living.GetBuffBonus(eBuffBonusType.SpecBuff)[eProperty.Constitution];
            var constitutionDebuffMalus = Math.Abs(living.GetBuffBonus(eBuffBonusType.Debuff)[eProperty.Constitution] + living.GetBuffBonus(eBuffBonusType.SpecDebuff)[eProperty.Constitution]);
            var magicAbsorptionFromConstitution = (int)((constitutionBuffBonus - constitutionDebuffMalus) / constitutionPerMagicAbsorptionPercent);
            buffBonus += magicAbsorptionFromConstitution;
        }

        buffBonus -= Math.Abs(debuff);

        // Apply debuffs. 100% Effectiveness for player buffs, but only 50%
        // effectiveness for item bonuses.
        if (living is GamePlayer && buffBonus < 0)
        {
            itemBonus += buffBonus / 2;
            buffBonus = 0;
        }

        // Add up and apply hardcap.

        return Math.Min(itemBonus + buffBonus + abilityBonus + racialBonus, HardCap);
	}

    public override int CalcValueBase(GameLiving living, eProperty property)
    {
        int propertyIndex = (int)property;
        int debuff = Math.Abs(living.GetBuffBonus(eBuffBonusType.Debuff)[propertyIndex]);
        int racialBonus = (living is GamePlayer) ? SkillBase.GetRaceResist(((living as GamePlayer).Race), (eResist)property) : 0;
        int itemBonus = CalcValueFromItems(living, property);
        int buffBonus = CalcValueFromBuffs(living, property);
        switch (property)
        {
            case eProperty.Resist_Body:
            case eProperty.Resist_Cold:
            case eProperty.Resist_Energy:
            case eProperty.Resist_Heat:
            case eProperty.Resist_Matter:
            case eProperty.Resist_Natural:
            case eProperty.Resist_Spirit:
                debuff += Math.Abs(living.GetBuffBonus(eBuffBonusType.Debuff)[eProperty.MagicAbsorption]);
                buffBonus += living.GetBuffBonus(eBuffBonusType.BaseBuff)[eProperty.MagicAbsorption];
                break;
        }

        if (living is GameNPC)
        {
            // NPC buffs effects are halved compared to debuffs, so it takes 2% debuff to mitigate 1% buff
            // See PropertyChangingSpell.ApplyNpcEffect() for details.
            buffBonus = buffBonus << 1;
            int specDebuff = Math.Abs(living.GetBuffBonus(eBuffBonusType.SpecDebuff)[property]);

            switch (property)
            {
                case eProperty.Resist_Body:
                case eProperty.Resist_Cold:
                case eProperty.Resist_Energy:
                case eProperty.Resist_Heat:
                case eProperty.Resist_Matter:
                case eProperty.Resist_Natural:
                case eProperty.Resist_Spirit:
                    specDebuff += Math.Abs(living.GetBuffBonus(eBuffBonusType.SpecDebuff)[eProperty.MagicAbsorption]);
                    break;
            }

            buffBonus -= specDebuff;
            if (buffBonus > 0)
                buffBonus = buffBonus >> 1;
        }

        buffBonus -= Math.Abs(debuff);

        if (living is GamePlayer && buffBonus < 0)
        {
            itemBonus += buffBonus / 2;
            buffBonus = 0;
            if (itemBonus < 0) itemBonus = 0;
        }
        return Math.Min(itemBonus + buffBonus + racialBonus, HardCap);
    }
    
    public override int CalcValueFromBuffs(GameLiving living, eProperty property)
    {
        int buffBonus = living.GetBuffBonus(eBuffBonusType.BaseBuff)[(int)property]
			+ living.GetBuffBonus(eBuffBonusType.BuffBonus)[(int)property];
        if (living is GameNPC)
            return buffBonus;
        return Math.Min(buffBonus, BuffBonusCap);
    }
    
    public override int CalcValueFromItems(GameLiving living, eProperty property)
    {
        if (living is GameNPC)
            return 0;

        int itemBonus = living.GetBuffBonus(eBuffBonusType.ItemBonus)[(int)property];

        // Item bonus cap and cap increase from Mythirians.

        int itemBonusCap = living.Level / 2 + 1;
        int itemBonusCapIncrease = GetItemBonusCapIncrease(living, property);
        return Math.Min(itemBonus, itemBonusCap + itemBonusCapIncrease);
    }
    
    public static int GetItemBonusCapIncrease(GameLiving living, eProperty property)
    {
        if (living == null) return 0;
        return Math.Min(living.GetBuffBonus(eBuffBonusType.ItemBonus)[(int)(eProperty.ResCapBonus_First - eProperty.Resist_First + property)], 5);
    }

    /// <summary>
    /// Cap for player cast resist buffs.
    /// </summary>
    public static int BuffBonusCap
    {
        get { return 24; }
    }

    /// <summary>
    /// Hard cap for resists.
    /// </summary>
    public static int HardCap
    {
        get { return 70; }
    }
}

[PropertyCalculator(eProperty.Resist_Natural)]
public class ResistNaturalCalculator : PropertyCalculator
{
	public ResistNaturalCalculator() { }

    public override int CalcValue(GameLiving living, eProperty property)
    {
        int propertyIndex = (int)property;
        int debuff = Math.Abs(living.GetBuffBonus(eBuffBonusType.Debuff)[propertyIndex]) + Math.Abs(living.GetBuffBonus(eBuffBonusType.Debuff)[eProperty.MagicAbsorption]);
		int abilityBonus = living.GetBuffBonus(eBuffBonusType.AbilityBonus)[propertyIndex] + living.GetBuffBonus(eBuffBonusType.AbilityBonus)[eProperty.MagicAbsorption];
        int itemBonus = CalcValueFromItems(living, property);
        int buffBonus = CalcValueFromBuffs(living, property);

        if (living is GameNPC)
        {
            // NPC buffs effects are halved compared to debuffs, so it takes 2% debuff to mitigate 1% buff
            // See PropertyChangingSpell.ApplyNpcEffect() for details.
            buffBonus = buffBonus << 1;
            int specDebuff = Math.Abs(living.GetBuffBonus(eBuffBonusType.SpecDebuff)[property]) + Math.Abs(living.GetBuffBonus(eBuffBonusType.SpecDebuff)[eProperty.MagicAbsorption]);

            buffBonus -= specDebuff;
            if (buffBonus > 0)
                buffBonus = buffBonus >> 1;
        }

        buffBonus -= Math.Abs(debuff);

        if (living is GamePlayer && buffBonus < 0)
        {
            itemBonus += buffBonus / 2;
            buffBonus = 0;
        }
		return (itemBonus + buffBonus + abilityBonus);
    }
    public override int CalcValueFromBuffs(GameLiving living, eProperty property)
    {
        int buffBonus = living.GetBuffBonus(eBuffBonusType.BaseBuff)[(int)property] 
            + living.GetBuffBonus(eBuffBonusType.BuffBonus)[(int)property]
            + living.GetBuffBonus(eBuffBonusType.BaseBuff)[eProperty.MagicAbsorption];
        if (living is GameNPC)
            return buffBonus;
        return Math.Min(buffBonus, BuffBonusCap);
    }
    public override int CalcValueFromItems(GameLiving living, eProperty property)
    {
        int itemBonus = living.GetBuffBonus(eBuffBonusType.ItemBonus)[(int)property];
        int itemBonusCap = living.Level / 2 + 1;
        return Math.Min(itemBonus, itemBonusCap);
    }
    public static int BuffBonusCap { get { return 25; } }

    public static int HardCap { get { return 70; } }
}	