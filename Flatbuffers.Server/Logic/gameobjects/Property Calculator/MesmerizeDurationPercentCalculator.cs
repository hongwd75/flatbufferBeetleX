using Game.Logic.Skills;

namespace Game.Logic.PropertyCalc;

[PropertyCalculator(eProperty.MesmerizeDurationReduction)]
public class MesmerizeDurationPercentCalculator : PropertyCalculator
{
    public override int CalcValue(GameLiving living, eProperty property) 
    {
        int percent = 100
                      - living.GetBuffBonus(eBuffBonusType.BaseBuff)[(int)property] 
                      + living.GetBuffBonus(eBuffBonusType.Debuff)[(int)property]
                      - living.GetBuffBonus(eBuffBonusType.ItemBonus)[(int)property]
                      - living.GetBuffBonus(eBuffBonusType.AbilityBonus)[(int)property];

        if (living.HasAbility(Abilities.Stoicism))
            percent -= 25;

        return Math.Max(1, percent);
    }
}