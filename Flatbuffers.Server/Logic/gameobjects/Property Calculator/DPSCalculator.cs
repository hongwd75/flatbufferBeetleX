namespace Game.Logic.PropertyCalc;

[PropertyCalculator(eProperty.DPS)]
public class DPSCalculator : PropertyCalculator
{
    public DPSCalculator() {}
    public override int CalcValue(GameLiving living, eProperty property)
    {
        return (int)(living.GetBuffBonus(eBuffBonusType.BaseBuff)[(int)property]
                     + living.GetBuffBonus(eBuffBonusType.SpecBuff)[(int)property]
                     - living.GetBuffBonus(eBuffBonusType.Debuff)[(int)property]
                     + living.GetBuffBonus(eBuffBonusType.BuffBonus)[(int)property]);
    }
}