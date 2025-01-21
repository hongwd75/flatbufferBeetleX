using Game.Logic.Effects;
using Game.Logic.Spells;

namespace Game.Logic.PropertyCalc;

[PropertyCalculator(eProperty.SpellRange)]
public class SpellRangePercentCalculator : PropertyCalculator
{
    public override int CalcValue(GameLiving living, eProperty property) 
    {
        int debuff = living.GetBuffBonus(eBuffBonusType.Debuff)[(int)property];
        if(debuff > 0)
        {
            GameSpellEffect nsreduction = SpellHandler.FindEffectOnTarget(living, "NearsightReduction");
            if(nsreduction!=null) debuff = (int)(debuff * (1.00 - nsreduction.Spell.Value * 0.01));
        }
        int buff = CalcValueFromBuffs(living, property);
        int item = CalcValueFromItems(living, property);
        return Math.Max(0, 100 + (buff + item) - debuff);
    }

    public override int CalcValueFromBuffs(GameLiving living, eProperty property)
    {
        return Math.Min(5, living.GetBuffBonus(eBuffBonusType.SpecBuff)[(int) property]);
    }

    public override int CalcValueFromItems(GameLiving living, eProperty property)
    {
        return Math.Min(10, living.GetBuffBonus(eBuffBonusType.ItemBonus)[(int)property]);
    }
}