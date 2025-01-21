namespace Game.Logic.PropertyCalc;

[PropertyCalculator(eProperty.StyleDamage)]
public class StyleDamagePercentCalculator : PropertyCalculator
{
    public override int CalcValue(GameLiving living, eProperty property)
    {
        return Math.Max(0, 100 + Math.Min(10,living.GetBuffBonus(eBuffBonusType.ItemBonus)[(int)property]));
    }
}