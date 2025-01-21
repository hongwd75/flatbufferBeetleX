using Game.Logic.Skills;

namespace Game.Logic.PropertyCalc;

[PropertyCalculator(eProperty.EvadeChance)]
public class EvadeChanceCalculator : PropertyCalculator
{
    public override int CalcValue(GameLiving living, eProperty property)
    {
        GamePlayer player = living as GamePlayer;
        if (player != null)
        {
            int evadechance = 0;
            if (player.HasAbility(Abilities.Evade))
            {
                evadechance +=
                    (1000 + player.GetModified(eProperty.Quickness) + player.GetModified(eProperty.Dexterity) - 100) *
                    player.GetAbilityLevel(Abilities.Evade) * 5 / 100;
            }

            evadechance += player.GetBuffBonus(eBuffBonusType.BaseBuff)[(int)property] * 10
                           + player.GetBuffBonus(eBuffBonusType.SpecBuff)[(int)property] * 10
                           - player.GetBuffBonus(eBuffBonusType.Debuff)[(int)property] * 10
                           + player.GetBuffBonus(eBuffBonusType.BuffBonus)[(int)property] * 10
                           + player.GetBuffBonus(eBuffBonusType.AbilityBonus)[(int)property] * 10;
            return evadechance;
        }

        GameNPC npc = living as GameNPC;
        if (npc != null)
        {
            return living.GetBuffBonus(eBuffBonusType.AbilityBonus)[(int)property] * 10 + npc.EvadeChance * 10;
        }

        return 0;
    }
}