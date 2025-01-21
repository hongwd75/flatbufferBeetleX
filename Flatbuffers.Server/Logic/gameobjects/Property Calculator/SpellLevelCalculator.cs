namespace Game.Logic.PropertyCalc;

[PropertyCalculator(eProperty.SpellLevel)]
public class SpellLevelCalculator : PropertyCalculator
{
    public SpellLevelCalculator() { }

    public override int CalcValue(GameLiving living, eProperty property)
    {
        return (int)(
            +living.GetBuffBonus(eBuffBonusType.BaseBuff)[(int)property]
            + living.GetBuffBonus(eBuffBonusType.SpecBuff)[(int)property]
            - living.GetBuffBonus(eBuffBonusType.Debuff)[(int)property]
            + living.GetBuffBonus(eBuffBonusType.BuffBonus)[(int)property]
            + living.GetBuffBonus(eBuffBonusType.AbilityBonus)[(int)property]
            + Math.Min(10, living.GetBuffBonus(eBuffBonusType.ItemBonus)[(int)property]));
    }
}