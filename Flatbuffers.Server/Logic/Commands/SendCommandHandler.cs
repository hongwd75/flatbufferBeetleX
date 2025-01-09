using Game.Logic.Language;
using Game.Logic.network;
using Game.Logic.Utils;
using NetworkMessage;

namespace Game.Logic.Commands;

	[CmdAttribute(
		"&send",
		new string[] { "&tell", "&t" },
		ePrivLevel.Player,
		"Sends a private message to a player",
		"Use: SEND <TARGET> <TEXT TO SEND>")]
	public class SendCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		public void OnCommand(GameClient client, string[] args)
		{
			if (args.Length < 3)
			{
				client.Out.SendMessage("Use: SEND <TARGET> <TEXT TO SEND>", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			if (IsSpammingCommand(client.Player, "send", 500))
			{
                DisplayMessage(client, LanguageMgr.GetTranslation(client, "GamePlayer.Spamming.Say"));
                return;
			}

			string targetName = args[1];
			string message = string.Join(" ", args, 2, args.Length - 2);

			int result = 0;
			GameClient targetClient = GameServer.Instance.Clients.GetClientByAccountName(targetName,true);

			if (targetClient == null)
			{
                // nothing found
                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Players.Send.NotInGame", targetName), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return;
			}

            // prevent to send an anon GM a message to find him - but send the message to the GM - thx to Sumy
            if (targetClient.Player != null && targetClient.Account.IsAnonymous && targetClient.Account.PrivLevel > (uint)ePrivLevel.Player)
            {
				if (client.Account.PrivLevel == (uint)ePrivLevel.Player)
				{
                    client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Players.Send.NotInGame", targetName), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    targetClient.Player.Network.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Players.Send.Anon", client.Player.Name, message), eChatType.CT_Send, eChatLoc.CL_ChatWindow);
				}
				else
				{
					// Let GM's communicate with other anon GM's
					ChatUtil.SendPrivateMessage(client,targetClient, "(anon) " + message);
				}
                return;
            }

			switch (result)
			{
				case 2: // name not unique
                    client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Players.Send.NotUnique"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    return;
				case 3: // exact match
				case 4: // guessed name
					if (targetClient == client)
					{
                        client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Players.Send.Yourself"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    }
					else
					{
						ChatUtil.SendPrivateMessage(client,targetClient, message);
					}
					return;
			}
		}
	}