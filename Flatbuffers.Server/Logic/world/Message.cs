using Game.Logic.Language;
using Game.Logic.Spells;
using NetworkMessage;

namespace Game.Logic.World;

public static class Message
{
	public static void ChatToArea(GameObject centerObject, string message, eChatType chatType, params GameObject[] excludes)
	{
		ChatToArea(centerObject, message, chatType, WorldManager.INFO_DISTANCE, excludes);
	}
	public static void ChatToOthers(GameObject centerObject, string message, eChatType chatType)
	{
		ChatToArea(centerObject, message, chatType, WorldManager.INFO_DISTANCE, centerObject);
	}
	public static void ChatToArea(GameObject centerObject, string message, eChatType chatType, ushort distance, params GameObject[] excludes)
	{
		MessageToArea(centerObject, message, chatType, eChatLoc.CL_ChatWindow, distance, excludes);
	}
	public static void SystemToArea(GameObject centerObject, string message, eChatType chatType, params GameObject[] excludes)
	{
		SystemToArea(centerObject, message, chatType, WorldManager.INFO_DISTANCE, excludes);
	}
	public static void SystemToOthers(GameObject centerObject, string message, eChatType chatType)
	{
		SystemToArea(centerObject, message, chatType, WorldManager.INFO_DISTANCE, centerObject);
	}
	public static void SystemToOthers2(GameObject centerObject, eChatType chatType, string LanguageMessageID, params object[] args)
	{
		if (LanguageMessageID == null || LanguageMessageID.Length <= 0) return;
		foreach (GamePlayer player in centerObject.GetPlayersInRadius(WorldManager.INFO_DISTANCE))
		{
			if (!(centerObject is GamePlayer && centerObject == player))
			{
				player.MessageFromArea(centerObject, LanguageMgr.GetTranslation(player.Network?.Account.Language, LanguageMessageID, args), chatType, eChatLoc.CL_SystemWindow);
			}
		}
	}
	public static void SystemToArea(GameObject centerObject, string message, eChatType chatType, ushort distance, params GameObject[] excludes)
	{
		MessageToArea(centerObject, message, chatType, eChatLoc.CL_SystemWindow, distance, excludes);
	}
	public static void MessageToArea(GameObject centerObject, string message, eChatType chatType, eChatLoc chatLoc, ushort distance, params GameObject[] excludes)
	{
		if (message == null || message.Length <= 0) return;
		bool excluded;
		foreach(GamePlayer player in centerObject.GetPlayersInRadius(distance))
		{
			excluded = false;
			if(excludes!=null)
			{
				foreach(GameObject obj in excludes)
					if(obj == player)
					{
						excluded = true;
						break;
					}
			}
			if (!excluded)
			{
				player.MessageFromArea(centerObject, message, chatType, chatLoc);
			}
		}
	}
}

//======================================================================================================================
//
public static class SpellMessagesHelper
{
	public static void MessageToCaster(this SpellHandler handler, eChatType type, string format, params object[] args)
	{
		handler.MessageToCaster(string.Format(format, args), type);
	}

	public static void MessageToLiving(this SpellHandler handler, GameLiving living, eChatType type, string format,
		params object[] args)
	{
		handler.MessageToLiving(living, string.Format(format, args), type);
	}
}