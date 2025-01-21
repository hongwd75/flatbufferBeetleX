namespace Game.Logic.PropertyCalc;

[PropertyCalculator(eProperty.MeleeDamage)]
public class MeleeDamagePercentCalculator : PropertyCalculator
{
    public override int CalcValue(GameLiving living, eProperty property)
    {
        if (living is GameNPC)
        {
            int strengthPerMeleeDamagePercent = 8;
            var strengthBuffBonus = living.GetBuffBonus(eBuffBonusType.BaseBuff)[eProperty.Strength] + living.GetBuffBonus(eBuffBonusType.SpecBuff)[eProperty.Strength];
            var strengthDebuffMalus = living.GetBuffBonus(eBuffBonusType.Debuff)[eProperty.Strength] + living.GetBuffBonus(eBuffBonusType.SpecDebuff)[eProperty.Strength];
            return living.GetBuffBonus(eBuffBonusType.AbilityBonus)[property] + (strengthBuffBonus - strengthDebuffMalus) / strengthPerMeleeDamagePercent;
        }

        int hardCap = 10;
        int abilityBonus = living.GetBuffBonus(eBuffBonusType.AbilityBonus)[(int)property];
        int itemBonus = Math.Min(hardCap, living.GetBuffBonus(eBuffBonusType.ItemBonus)[(int)property]);
        int buffBonus = living.GetBuffBonus(eBuffBonusType.BaseBuff)[(int)property] + living.GetBuffBonus(eBuffBonusType.SpecBuff)[(int)property];
        int debuffMalus = Math.Min(hardCap, Math.Abs(living.GetBuffBonus(eBuffBonusType.Debuff)[(int)property]));
        return abilityBonus + buffBonus + itemBonus - debuffMalus;
    }
}