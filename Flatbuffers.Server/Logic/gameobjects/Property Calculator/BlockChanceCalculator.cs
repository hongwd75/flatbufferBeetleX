using Game.Logic.Skills;

namespace Game.Logic.PropertyCalc;

[PropertyCalculator(eProperty.BlockChance)]
public class BlockChanceCalculator : PropertyCalculator
{
    public override int CalcValue(GameLiving living, eProperty property)
    {
        GamePlayer player = living as GamePlayer;
        if (player != null)
        {
            int shield = (player.GetModifiedSpecLevel(Specs.Shields) - 1) * (10 / 2);
            int ability = player.GetBuffBonus(eBuffBonusType.AbilityBonus)[(int)property] * 10;
            int chance = 50 + shield + ((player.GetModified(eProperty.Dexterity) * 2 - 100) / 4) + ability;
				
            return chance;
        }

        GameNPC npc = living as GameNPC;
        if (npc != null)
        {
            return npc.BlockChance * 10;
        }

        return 0;
    }
}