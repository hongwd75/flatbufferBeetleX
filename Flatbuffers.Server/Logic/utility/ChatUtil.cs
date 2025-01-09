using Game.Logic.Language;
using Game.Logic.network;
using NetworkMessage;

namespace Game.Logic.Utils;

public static class ChatUtil
	{
		public static void SendSystemMessage(GamePlayer target, string message)
		{
			target.Network.Out.SendMessage(message, eChatType.CT_System, eChatLoc.CL_SystemWindow);
		}

		public static void SendSystemMessage(GamePlayer target, string translationID, params object[] args)
		{
			var translatedMsg = LanguageMgr.GetTranslation(target.Network, translationID, args);

			target.Network.Out.SendMessage(translatedMsg, eChatType.CT_System, eChatLoc.CL_SystemWindow);
		}

		public static void SendSystemMessage(GameClient target, string message)
		{
			target.Out.SendMessage(message, eChatType.CT_System, eChatLoc.CL_SystemWindow);
		}

		public static void SendSystemMessage(GameClient target, string translationID, params object[] args)
		{
			var translatedMsg = LanguageMgr.GetTranslation(target, translationID, args);

			target.Out.SendMessage(translatedMsg, eChatType.CT_System, eChatLoc.CL_SystemWindow);
		}

		public static void SendMerchantMessage(GamePlayer target, string message)
		{
			target.Network.Out.SendMessage(message, eChatType.CT_Merchant, eChatLoc.CL_SystemWindow);
		}

		public static void SendMerchantMessage(GamePlayer target, string translationID, params object[] args)
		{
			var translatedMsg = LanguageMgr.GetTranslation(target.Network, translationID, args);

			target.Network.Out.SendMessage(translatedMsg, eChatType.CT_Merchant, eChatLoc.CL_SystemWindow);
		}

		public static void SendMerchantMessage(GameClient target, string message)
		{
			target.Out.SendMessage(message, eChatType.CT_Merchant, eChatLoc.CL_SystemWindow);
		}

		public static void SendMerchantMessage(GameClient target, string translationID, params object[] args)
		{
			var translatedMsg = LanguageMgr.GetTranslation(target, translationID, args);

			target.Out.SendMessage(translatedMsg, eChatType.CT_Merchant, eChatLoc.CL_SystemWindow);
		}

		public static void SendHelpMessage(GamePlayer target, string message)
		{
			target.Network.Out.SendMessage(message, eChatType.CT_Help, eChatLoc.CL_SystemWindow);
		}

		public static void SendHelpMessage(GamePlayer target, string translationID, params object[] args)
		{
			var translatedMsg = LanguageMgr.GetTranslation(target.Network, translationID, args);

			target.Network.Out.SendMessage(translatedMsg, eChatType.CT_Help, eChatLoc.CL_SystemWindow);
		}

		public static void SendHelpMessage(GameClient target, string message)
		{
			target.Out.SendMessage(message, eChatType.CT_Help, eChatLoc.CL_SystemWindow);
		}

		public static void SendHelpMessage(GameClient target, string translationID, params object[] args)
		{
			var translatedMsg = LanguageMgr.GetTranslation(target, translationID, args);

			target.Out.SendMessage(translatedMsg, eChatType.CT_Help, eChatLoc.CL_SystemWindow);
		}

		public static void SendErrorMessage(GamePlayer target, string message)
		{
			SendErrorMessage(target.Network, message);
		}
		public static void SendErrorMessage(GameClient target, string message)
		{
			target.Out.SendMessage(message, eChatType.CT_Important, eChatLoc.CL_SystemWindow);
		}

		public static bool SendPrivateMessage(GameClient from,GameClient to, string str)
		{
			if (from.Account.Realm != to.Account.Realm)
			{
				from.Out.SendMessage(LanguageMgr.GetTranslation(from.Account.Language, "GamePlayer.Send.target.DontUnderstandYou", to.Account.Nickname), eChatType.CT_Send, eChatLoc.CL_ChatWindow);
				return false;
			}
			if (from.Account.PrivLevel == 1 && to.Account.PrivLevel > 1 && to.Account.IsAnonymous)
			{
				return true;
			}
			from.Out.SendMessage(LanguageMgr.GetTranslation(from.Account.Language, "GamePlayer.Send.YouSendTo", str, to.Account.Nickname), eChatType.CT_Send, eChatLoc.CL_ChatWindow);		
			return true;
		}
	}