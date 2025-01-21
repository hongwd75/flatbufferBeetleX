using Game.Logic.Utils;

namespace Game.Logic.PropertyCalc;

[PropertyCalculator(eProperty.EnduranceRegenerationRate)]
public class EnduranceRegenerationRateCalculator : PropertyCalculator
{
    public EnduranceRegenerationRateCalculator() {}

    public override int CalcValue(GameLiving living, eProperty property)
    {
        int debuff = living.GetBuffBonus(eBuffBonusType.SpecBuff)[(int)property];
        if (debuff < 0) debuff = -debuff;

        double regen = living.GetBuffBonus(eBuffBonusType.BaseBuff)[(int)property] + living.GetBuffBonus(eBuffBonusType.ItemBonus)[(int)property];
        if (regen == 0 && living is GamePlayer) regen++;
        
        if (!living.InCombat)
        {
            if (living is GamePlayer)
            {
                if (!((GamePlayer)living).IsSprinting) regen += 4;
            }
        }
        
        regen -= debuff;

        if (regen < 0) regen = 0;

        double decimals = regen - (int)regen;
        if (RandomUtil.Chance(decimals))
        {
            regen += 1;	
        }
        return (int)regen;
    }
}