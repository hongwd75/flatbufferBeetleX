using Game.Logic.network;

namespace Game.Logic.Commands;

public interface ICommandHandler
{
    void OnCommand(GameClient client, string[] args);
}