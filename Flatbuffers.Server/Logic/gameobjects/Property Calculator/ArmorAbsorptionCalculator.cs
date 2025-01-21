namespace Game.Logic.PropertyCalc;

[PropertyCalculator(eProperty.ArmorAbsorption)]
public class ArmorAbsorptionCalculator : PropertyCalculator
{
    public override int CalcValue(GameLiving living, eProperty property)
    {
        int buffBonus = living.GetBuffBonus(eBuffBonusType.BaseBuff)[property];
        int debuffMalus = Math.Abs(living.GetBuffBonus(eBuffBonusType.Debuff)[property]);
        int itemBonus = living.GetBuffBonus(eBuffBonusType.ItemBonus)[property];
        int abilityBonus = living.GetBuffBonus(eBuffBonusType.AbilityBonus)[property];
        int hardCap = 50;
        return Math.Min(hardCap, (buffBonus - debuffMalus + itemBonus + abilityBonus));
    }
}