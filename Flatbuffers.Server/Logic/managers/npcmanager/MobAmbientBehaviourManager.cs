using Logic.database;
using Logic.database.table;

namespace Game.Logic.managers;

public sealed class MobAmbientBehaviourManager
{
    private Dictionary<string, MobXAmbientBehaviour[]> AmbientBehaviour { get; }

    public MobXAmbientBehaviour[] this[string index]
    {
        get
        {
            if (string.IsNullOrEmpty(index))
            {
                return Array.Empty<MobXAmbientBehaviour>();
            }

            var lower = index.ToLower();
            return AmbientBehaviour.ContainsKey(lower)
                ? AmbientBehaviour[lower]
                : Array.Empty<MobXAmbientBehaviour>();
        }
    }

    public MobAmbientBehaviourManager(IObjectDatabase database)
    {
        if (database == null)
        {
            throw new ArgumentNullException(nameof(database));
        }

        AmbientBehaviour = database.SelectAllObjects<MobXAmbientBehaviour>()
            .GroupBy(x => x.Source)
            .ToDictionary(key => key.Key.ToLower(), value => value.ToArray());
    }
}