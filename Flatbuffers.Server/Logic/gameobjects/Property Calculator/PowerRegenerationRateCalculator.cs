using Game.Logic.Utils;

namespace Game.Logic.PropertyCalc;

[PropertyCalculator(eProperty.PowerRegenerationRate)]
public class PowerRegenerationRateCalculator : PropertyCalculator
{
    public PowerRegenerationRateCalculator() {}

    public override int CalcValue(GameLiving living, eProperty property) 
    {
        double regen = 5 + (living.Level / 2.75);

        if (living is GameNPC && living.InCombat)
            regen /= 2.0;
        double decimals = regen - (int)regen;
        if (RandomUtil.Chance(decimals)) 
        {
            regen += 1;	// compensate int rounding error
        }

        int debuff = living.GetBuffBonus(eBuffBonusType.SpecBuff)[(int)property];
        if (debuff < 0)
            debuff = -debuff;

        regen += living.GetBuffBonus(eBuffBonusType.BaseBuff)[(int)property] 
                    + living.GetBuffBonus(eBuffBonusType.AbilityBonus)[(int)property] 
                    + living.GetBuffBonus(eBuffBonusType.ItemBonus)[(int)property] - debuff;

        if (regen < 1)
            regen = 1;

        return (int)regen;
    }
}