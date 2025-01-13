namespace Game.Logic.Skills;

public interface ISpellCastingAbilityHandler
{
    Spell Spell { get; }
    SpellLine SpellLine { get; }
    Ability Ability { get; }    
}