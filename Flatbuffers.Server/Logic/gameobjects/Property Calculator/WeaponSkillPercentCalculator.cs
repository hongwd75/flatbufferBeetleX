namespace Game.Logic.PropertyCalc;

[PropertyCalculator(eProperty.WeaponSkill)]
public class WeaponSkillPercentCalculator : PropertyCalculator
{
    public override int CalcValue(GameLiving living, eProperty property)
    {
        double percent = 100
                         + living.GetBuffBonus(eBuffBonusType.BaseBuff)[(int)property]
                         + living.GetBuffBonus(eBuffBonusType.SpecBuff)[(int)property]
                         - (int)Math.Abs(living.GetBuffBonus(eBuffBonusType.Debuff)[(int)property] / 5.4)
                         + living.GetBuffBonus(eBuffBonusType.ItemBonus)[(int)property];
        return (int)Math.Max(1, percent);
    }
}