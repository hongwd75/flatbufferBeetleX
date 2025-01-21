using Game.Logic.AI.Brain;

namespace Game.Logic.PropertyCalc;

[PropertyCalculator(eProperty.CriticalMeleeHitChance)]
public class CriticalMeleeHitChanceCalculator : PropertyCalculator
{
    public CriticalMeleeHitChanceCalculator() { }

    public override int CalcValue(GameLiving living, eProperty property)
    {
        int chance = living.GetBuffBonus(eBuffBonusType.BuffBonus)[(int)property] + living.GetBuffBonus(eBuffBonusType.AbilityBonus)[(int)property];
        chance += 10;
        return Math.Min(chance, 50);
    }
}