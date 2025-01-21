namespace Game.Logic.PropertyCalc;

[PropertyCalculator(eProperty.ResistPierce)]
public class ResistPierceCalculator : PropertyCalculator
{
    public override int CalcValue(GameLiving living, eProperty property)
    {
        // cap at living.level/5
        return Math.Min(Math.Max(1,living.Level/5),
                living.GetBuffBonus(eBuffBonusType.BaseBuff)[(int)property]
                - living.GetBuffBonus(eBuffBonusType.Debuff)[(int)property]
                + living.GetBuffBonus(eBuffBonusType.ItemBonus)[(int)property]); 
    }
}