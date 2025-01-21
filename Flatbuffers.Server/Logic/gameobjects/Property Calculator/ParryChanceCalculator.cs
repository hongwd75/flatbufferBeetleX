using Game.Logic.Skills;

namespace Game.Logic.PropertyCalc;

[PropertyCalculator(eProperty.ParryChance)]
public class ParryChanceCalculator : PropertyCalculator
{
    public override int CalcValue(GameLiving living, eProperty property)
    {
			
        GamePlayer player = living as GamePlayer;
        if (player != null)
        {
            int buff = player.GetBuffBonus(eBuffBonusType.BaseBuff)[(int)property] * 10
                       + player.GetBuffBonus(eBuffBonusType.SpecBuff)[(int)property] * 10
                       - player.GetBuffBonus(eBuffBonusType.Debuff)[(int)property] * 10
                       + player.GetBuffBonus(eBuffBonusType.BuffBonus)[(int)property] * 10
                       + player.GetBuffBonus(eBuffBonusType.AbilityBonus)[(int)property] * 10;
            int parrySpec = 0;
            if (player.HasSpecialization(Specs.Parry))
            {					
                parrySpec = (player.GetModified(eProperty.Dexterity) * 2 - 100) / 4 + (player.GetModifiedSpecLevel(Specs.Parry) - 1) * (10 / 2) + 50;
            }
            if (parrySpec > 500)
            {
                parrySpec = 500;
            }
            return parrySpec + buff;
        }
		
        GameNPC npc = living as GameNPC;
        if (npc != null)
        {
            return npc.ParryChance * 10;
        }

        return 0;
    }
}