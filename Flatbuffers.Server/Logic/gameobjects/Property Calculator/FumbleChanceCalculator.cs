namespace Game.Logic.PropertyCalc;

[PropertyCalculator(eProperty.FumbleChance)]
public class FumbleChanceCalculator : PropertyCalculator
{
    public override int CalcValue(GameLiving living, eProperty property)
    {
        return Math.Max(51 - living.Level, 
            (10 * living.GetBuffBonus(eBuffBonusType.Debuff)[(int)property]) 
            + (10 * living.GetBuffBonus(eBuffBonusType.AbilityBonus)[(int)property]));
    }
}

[PropertyCalculator(eProperty.SpellFumbleChance)]
public class SpellFumbleChanceCalculator : PropertyCalculator
{
    public override int CalcValue(GameLiving living, eProperty property)
    {
        return Math.Min(100, living.GetBuffBonus(eBuffBonusType.Debuff)[(int)property]);
    }
}