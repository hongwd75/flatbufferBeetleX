using System.Reflection;
using Game.Logic.network;
using Game.Logic.Utils;
using log4net;

namespace Game.Logic.Commands;

public abstract class AbstractCommandHandler
{
    private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    
	public bool IsSpammingCommand(GamePlayer player, string commandName)
	{
		return IsSpammingCommand(player, commandName, ServerProperties.Properties.COMMAND_SPAM_DELAY);
	}

	/// <summary>
	/// Is this player spamming this command
	/// </summary>
	/// <param name="player"></param>
	/// <param name="commandName"></param>
	/// <param name="delay">How long is the spam delay in milliseconds</param>
	/// <returns>true if less than spam protection interval</returns>
	public bool IsSpammingCommand(GamePlayer player, string commandName, int delay)
	{
		string spamKey = commandName + "NOSPAM";
		long tick = player.TempProperties.getProperty<long>(spamKey, 0);

		if (tick > 0 && player.CurrentRegion.Time - tick <= 0)
		{
			player.TempProperties.removeProperty(spamKey);
		}

		long changeTime = player.CurrentRegion.Time - tick;

		if (tick > 0 && (player.CurrentRegion.Time - tick) < delay)
		{
			return true;
		}

		player.TempProperties.setProperty(spamKey, player.CurrentRegion.Time);
		return false;
	}

	public virtual void DisplayMessage(GamePlayer player, string message)
	{
		DisplayMessage(player.Network, message, System.Array.Empty<object>() );
	}

	public virtual void DisplayMessage(GameClient client, string message)
	{
		DisplayMessage(client, message, System.Array.Empty<object>() );
	}

	public virtual void DisplayMessage(GameClient client, string message, params object[] objs)
	{
		if (client == null || !client.IsPlaying)
			return;

		ChatUtil.SendSystemMessage(client, string.Format(message, objs));
		return;
	}

	public virtual void DisplaySyntax(GameClient client)
	{
		if (client == null || !client.IsPlaying)
			return;

		var attrib = (CmdAttribute[]) GetType().GetCustomAttributes(typeof (CmdAttribute), false);
		if (attrib.Length == 0)
			return;

		ChatUtil.SendSystemMessage(client, attrib[0].Description, null);

		foreach (string sentence in attrib[0].Usage)
		{
			ChatUtil.SendSystemMessage(client, sentence, null);
		}

		return;
	}

	public virtual void DisplaySyntax(GameClient client, string subcommand)
	{
		if (client == null || !client.IsPlaying)
			return;

		var attrib = (CmdAttribute[]) GetType().GetCustomAttributes(typeof (CmdAttribute), false);

		if (attrib.Length == 0)
			return;

		foreach (string sentence in attrib[0].Usage)
		{
			string[] words = sentence.Split(new[] {' '}, 3);

			if (words.Length >= 2 && words[1].Equals(subcommand))
			{
				ChatUtil.SendSystemMessage(client, sentence, null);
			}
		}

		return;
	}

	public virtual void DisplaySyntax(GameClient client, string subcommand1, string subcommand2)
	{
		if (client == null || !client.IsPlaying)
			return;

		var attrib = (CmdAttribute[]) GetType().GetCustomAttributes(typeof (CmdAttribute), false);

		if (attrib.Length == 0)
			return;

		foreach (string sentence in attrib[0].Usage)
		{
			string[] words = sentence.Split(new[] {' '}, 4);

			if (words.Length >= 3 && words[1].Equals(subcommand1) && words[2].Equals(subcommand2))
			{
				ChatUtil.SendSystemMessage(client, sentence, null);
			}
		}

		return;
	}    
}