namespace Game.Logic.PropertyCalc;

[PropertyCalculator(eProperty.MeleeSpeed)]
public class MeleeSpeedPercentCalculator : PropertyCalculator
{
    public override int CalcValue(GameLiving living, eProperty property)
    {
        if (living is GameNPC)
        {
            int buffs = living.GetBuffBonus(eBuffBonusType.BaseBuff)[property] << 1;
            int debuff = Math.Abs(living.GetBuffBonus(eBuffBonusType.Debuff)[property]);
            int specDebuff = Math.Abs(living.GetBuffBonus(eBuffBonusType.SpecDebuff)[property]);

            buffs -= specDebuff;
            if (buffs > 0)
                buffs = buffs >> 1;
            buffs -= debuff;

            return 100 - buffs;
        }

        return Math.Max(1, 100
                           -living.GetBuffBonus(eBuffBonusType.BaseBuff)[(int)property]
                           +Math.Abs(living.GetBuffBonus(eBuffBonusType.Debuff)[(int)property])
                           -Math.Min(10, living.GetBuffBonus(eBuffBonusType.ItemBonus)[(int)property]));
    }
}