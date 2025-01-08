using Game.Logic;
using Game.Logic.datatable;

namespace Game.Logic.World;

public interface ITranslatableObject
{
    string TranslationId { get; set; }

    LanguageDataObject.eTranslationIdentifier TranslationIdentifier { get; }
}