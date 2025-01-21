namespace Game.Logic.PropertyCalc;

[PropertyCalculator(eProperty.ArmorFactor)]
public class ArmorFactorCalculator : PropertyCalculator
{
    public override int CalcValue(GameLiving living, eProperty property)
    {
        if (living is GamePlayer)
        {
            int af;
            // 
            // 1.5*1.25 spec line buff cap
            af = Math.Min((int)(living.Level * 1.875), living.GetBuffBonus(eBuffBonusType.SpecBuff)[(int)property]);
            // debuff
            af -= Math.Abs(living.GetBuffBonus(eBuffBonusType.Debuff)[(int)property]);
            // TrialsOfAtlantis af bonus
            af += Math.Min(living.Level, living.GetBuffBonus(eBuffBonusType.ItemBonus)[(int)property]);
            // uncapped category
            af += living.GetBuffBonus(eBuffBonusType.BuffBonus)[(int)property];

            return af;
        }
        else
        {
            return (int)((1 + (living.Level / 170.0)) * (living.Level << 1) * 4.67)
                   + living.GetBuffBonus(eBuffBonusType.SpecBuff)[(int)property]
                   - Math.Abs(living.GetBuffBonus(eBuffBonusType.Debuff)[(int)property])
                   + living.GetBuffBonus(eBuffBonusType.BuffBonus)[(int)property];
        }
    }
}