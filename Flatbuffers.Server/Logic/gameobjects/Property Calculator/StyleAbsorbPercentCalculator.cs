namespace Game.Logic.PropertyCalc;

[PropertyCalculator(eProperty.StyleAbsorb)]
public class StyleAbsorbPercentCalculator : PropertyCalculator
{
    public override int CalcValue(GameLiving living, eProperty property)
    {
        return living.GetBuffBonus(eBuffBonusType.ItemBonus)[(int)property] + living.GetBuffBonus(eBuffBonusType.BaseBuff)[(int)property];
    }
}