using Game.Logic.network;
using Logic.database.table;

namespace Game.Logic
{
    public class GamePlayer : GameLiving
    {
        private GameClient mNetwork = null;
        private string mAccountName = null;

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