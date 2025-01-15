namespace Game.Logic.Skills;
[SkillHandlerAttribute(Abilities.Sprint)]
public class SprintAbilityHandler  : IAbilityActionHandler
{
    public void Execute(Ability ab, GamePlayer player)
    {
        player.Sprint(!player.IsSprinting);
    }
}
