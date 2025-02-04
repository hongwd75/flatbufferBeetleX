namespace Game.Logic.managers;

public sealed class NpcManager
{
    private GameServer GameServerInstance { get; set; }
    public MobAmbientBehaviourManager AmbientBehaviour { get; private set; }

    public NpcManager(GameServer GameServerInstance)
    {
        if (GameServerInstance == null)
            throw new ArgumentNullException("GameServerInstance");

        this.GameServerInstance = GameServerInstance;
			
        AmbientBehaviour = new MobAmbientBehaviourManager(this.GameServerInstance.IDatabase);
    }
}