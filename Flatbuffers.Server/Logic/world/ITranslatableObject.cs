using Game.Logic;

namespace Game.Logic.World;

public interface ITranslatableObject
{
    string TranslationId { get; set; }

    eTranslationIdentifier TranslationIdentifier { get; }
}