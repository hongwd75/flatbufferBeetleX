using Game.Logic.AI.Brain;

namespace Game.Logic.PropertyCalc;

[PropertyCalculator(eProperty.LivingEffectiveLevel)]
public class LivingEffectiveLevelCalculator : PropertyCalculator
{
    public override int CalcValue(GameLiving living, eProperty property) 
    {
        if (living is GamePlayer) 
        {
            return living.Level + living.GetBuffBonus(eBuffBonusType.ItemBonus)[(int)property] + living.GetBuffBonus(eBuffBonusType.BaseBuff)[(int)property];
        } 		
        else if (living is GameNPC) 
        {
            IControlledBrain brain = ((GameNPC)living).Brain as IControlledBrain;
            if (brain != null)
                return brain.Owner.Level + living.GetBuffBonus(eBuffBonusType.ItemBonus)[(int)property] + living.GetBuffBonus(eBuffBonusType.BaseBuff)[(int)property];
            return living.Level + living.GetBuffBonus(eBuffBonusType.ItemBonus)[(int)property] + living.GetBuffBonus(eBuffBonusType.BaseBuff)[(int)property];
        }
        return 0;
    }
}