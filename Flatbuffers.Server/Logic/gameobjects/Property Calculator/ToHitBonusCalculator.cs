namespace Game.Logic.PropertyCalc;

[PropertyCalculator(eProperty.ToHitBonus)]
public class ToHitBonusCalculator : PropertyCalculator
{
    public ToHitBonusCalculator() { }

    public override int CalcValue(GameLiving living, eProperty property)
    {
        return (int)(
            +living.GetBuffBonus(eBuffBonusType.BaseBuff)[(int)property]
            + living.GetBuffBonus(eBuffBonusType.SpecBuff)[(int)property]
            - living.GetBuffBonus(eBuffBonusType.Debuff)[(int)property]
            + living.GetBuffBonus(eBuffBonusType.BuffBonus)[(int)property]);
    }
}