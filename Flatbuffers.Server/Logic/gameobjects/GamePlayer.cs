using Flatbuffers.Server.Logic.network;

namespace Game.Logic
{
    public class GamePlayer : GameLiving
    {
        private UInt64 mAccountIndex;
        private GameClient mNetwork;

        public GameClient Network
        {
            get => mNetwork;
            set
            {
                mNetwork = value;
            }
        }
    }
}