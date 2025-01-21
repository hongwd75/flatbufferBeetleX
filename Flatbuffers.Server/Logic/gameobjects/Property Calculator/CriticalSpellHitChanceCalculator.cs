using Game.Logic.AI.Brain;
using Game.Logic.CharacterClasses;

namespace Game.Logic.PropertyCalc;

[PropertyCalculator(eProperty.CriticalSpellHitChance)]
public class CriticalSpellHitChanceCalculator : PropertyCalculator
{
    public CriticalSpellHitChanceCalculator() {}

    public override int CalcValue(GameLiving living, eProperty property) 
    {
        int chance = living.GetBuffBonus(eBuffBonusType.AbilityBonus)[(int)property];

        if (living is GamePlayer player)
        {
            if (player.CharacterClass.ClassType == eClassType.ListCaster)
            {
                chance += 10;
            }
        }
        return Math.Min(chance, 50);
    }
}