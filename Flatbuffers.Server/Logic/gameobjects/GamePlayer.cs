using Game.Logic.Geometry;
using Game.Logic.network;
using Game.Logic.World;
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
        
        public Position BindPosition
        {
            get
            {
                if(DBCharacter == null) return Position.Zero;
                
                return DBCharacter.GetBindPosition();
            }
            set
            {
                if (DBCharacter == null) return;

                DBCharacter.BindRegion = value.RegionID;
                DBCharacter.BindXpos = value.X;
                DBCharacter.BindYpos = value.Y;
                DBCharacter.BindZpos = value.Z;
                DBCharacter.BindHeading = value.Orientation.InHeading;
            }
        }
        
        public virtual bool MoveToBind()
        {
            Region rgn = WorldManager.GetRegion(BindPosition.RegionID);
            if (rgn == null || rgn.GetZone(BindPosition.Coordinate) == null)
            {
                if (log.IsErrorEnabled)
                    log.Error("Player: " + Name + " unknown bind point : (R/X/Y) " + BindPosition.RegionID + "/" + BindPosition.X + "/" + BindPosition.Y);
                //Kick the player, avoid server freeze
                Client.Out.SendPlayerQuit(true);
                SaveIntoDatabase();
                Quit(true);
                //now ban him
                if (ServerProperties.Properties.BAN_HACKERS)
                {
                    DBBannedAccount b = new DBBannedAccount();
                    b.Author = "SERVER";
                    b.Ip = Client.TcpEndpointAddress;
                    b.Account = Client.Account.Name;
                    b.DateBan = DateTime.Now;
                    b.Type = "B";
                    b.Reason = "X/Y/RegionID : " + Position.X + "/" + Position.Y + "/" + Position.RegionID;
                    GameServer.Database.AddObject(b);
                    GameServer.Database.SaveObject(b);
                    string message = "Unknown bind point, your account is banned, contact a GM.";
                    Client.Out.SendMessage(message, eChatType.CT_Help, eChatLoc.CL_SystemWindow);
                    Client.Out.SendMessage(message, eChatType.CT_Help, eChatLoc.CL_ChatWindow);
                }
                return false;
            }

            if (GameServer.ServerRules.IsAllowedToMoveToBind(this))
                return MoveTo(BindPosition);

            return false;
        }        
    }
}