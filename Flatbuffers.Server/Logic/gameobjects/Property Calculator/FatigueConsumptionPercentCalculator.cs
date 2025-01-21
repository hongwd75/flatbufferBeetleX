namespace Game.Logic.PropertyCalc;

[PropertyCalculator(eProperty.FatigueConsumption)]
public class FatigueConsumptionPercentCalculator : PropertyCalculator
{
    public override int CalcValue(GameLiving living, eProperty property)
    {
        return Math.Max(1, 100
                           - living.GetBuffBonus(eBuffBonusType.BaseBuff)[(int)property]
                           + living.GetBuffBonus(eBuffBonusType.Debuff)[(int)property]
                           - Math.Min(10, living.GetBuffBonus(eBuffBonusType.ItemBonus)[(int)property]));
    }
}