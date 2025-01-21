using Game.Logic.Skills;

namespace Game.Logic.PropertyCalc;

[PropertyCalculator(eProperty.Skill_First, eProperty.Skill_Last)]
public class SkillLevelCalculator : PropertyCalculator
{
    public SkillLevelCalculator() {}

    public override int CalcValue(GameLiving living, eProperty property) 
    {
        if (living is GamePlayer) 
        {
            GamePlayer player = (GamePlayer)living;
            int itemCap = player.Level/5+1;
            int itemBonus = player.GetBuffBonus(eBuffBonusType.ItemBonus)[(int)property];

            // if (SkillBase.CheckPropertyType(property, ePropertyType.SkillMeleeWeapon))
            //     itemBonus += player.ItemBonus[(int)eProperty.AllMeleeWeaponSkills];
            // if (SkillBase.CheckPropertyType(property, ePropertyType.SkillMagical))
            //     itemBonus += player.ItemBonus[(int)eProperty.AllMagicSkills];
            // if (SkillBase.CheckPropertyType(property, ePropertyType.SkillDualWield))
            //     itemBonus += player.ItemBonus[(int)eProperty.AllDualWieldingSkills];
            // if (SkillBase.CheckPropertyType(property, ePropertyType.SkillArchery))
            //     itemBonus += player.ItemBonus[(int)eProperty.AllArcherySkills];

            itemBonus += player.GetBuffBonus(eBuffBonusType.ItemBonus)[(int)eProperty.AllSkills];

            if (itemBonus > itemCap)
                itemBonus = itemCap;
            int buffs = player.GetBuffBonus(eBuffBonusType.BaseBuff)[(int)property]; // one buff category just in case..

            return itemBonus + buffs;
        } 
        else 
        {
            // TODO other living types
        }
        return 0;
    }
}