﻿using System.Collections;
using Logic.database;
using Logic.database.table;

namespace Game.Logic.Guild;

public class Alliance
{
	protected ArrayList m_guilds;
	protected DBAlliance m_dballiance;
	public Alliance()
	{
		m_dballiance = null;
		m_guilds = new ArrayList(2);
	}
	public ArrayList Guilds
	{
		get
		{
			return m_guilds;
		}
		set
		{
			m_guilds = value;
		}
	}
	public DBAlliance Dballiance
	{
		get
		{
			return m_dballiance;
		}
		set
		{
			m_dballiance = value;
		}
	}

	#region IList
	public void AddGuild(Guild myguild)
	{
		lock (Guilds.SyncRoot)
		{
			myguild.alliance = this;
			Guilds.Add(myguild);
			myguild.AllianceId = m_dballiance.ObjectId;
			m_dballiance.DBguilds = null;
			//sirru 23.12.06 Add the new object instead of trying to save it
			GameServer.Database.AddObject(m_dballiance);
			GameServer.Database.FillObjectRelations(m_dballiance);
			//sirru 23.12.06 save changes to db for each guild
			SaveIntoDatabase();
			SendMessageToAllianceMembers(myguild.Name + " has joined the alliance of " + m_dballiance.AllianceName, NetworkMessage.eChatType.CT_System, NetworkMessage.eChatLoc.CL_SystemWindow);
		}
	}
	public void RemoveGuild(Guild myguild)
	{
		lock (Guilds.SyncRoot)
		{
			myguild.alliance = null;
			myguild.AllianceId = "";
            Guilds.Remove(myguild);
            if (myguild.GuildID == m_dballiance.DBguildleader.GuildID)
            {
                SendMessageToAllianceMembers(myguild.Name + " has disbanded the alliance of " + m_dballiance.AllianceName, NetworkMessage.eChatType.CT_System, NetworkMessage.eChatLoc.CL_SystemWindow);
                ArrayList mgl = new ArrayList(Guilds);
                foreach (Guild mg in mgl)
                {
                    try
                    {
                        RemoveGuild(mg);
                    }
                    catch (Exception)
                    {
                    }
                }
                GameServer.Database.DeleteObject(m_dballiance);
            }
            else
            {
                m_dballiance.DBguilds = null;
                GameServer.Database.SaveObject(m_dballiance);
                GameServer.Database.FillObjectRelations(m_dballiance);
            }
			//sirru 23.12.06 save changes to db for each guild
			myguild.SaveIntoDatabase();
            myguild.SendMessageToGuildMembers(myguild.Name + " has left the alliance of " + m_dballiance.AllianceName, NetworkMessage.eChatType.CT_System, NetworkMessage.eChatLoc.CL_SystemWindow);
            SendMessageToAllianceMembers(myguild.Name + " has left the alliance of " + m_dballiance.AllianceName, NetworkMessage.eChatType.CT_System, NetworkMessage.eChatLoc.CL_SystemWindow);
		}
	}
	public void Clear()
	{
		lock (Guilds.SyncRoot)
		{
			foreach (Guild guild in Guilds)
			{
				guild.alliance = null;
				guild.AllianceId = "";
				//sirru 23.12.06 save changes to db
				guild.SaveIntoDatabase();
			}
			Guilds.Clear();
		}
	}
	public bool Contains(Guild myguild)
	{
		lock (Guilds.SyncRoot)
		{
			return Guilds.Contains(myguild);
		}
	}

	#endregion

	/// <summary>
	/// send message to all member of alliance
	/// </summary>
	public void SendMessageToAllianceMembers(string msg, NetworkMessage.eChatType type, NetworkMessage.eChatLoc loc)
	{
		lock (Guilds.SyncRoot)
		{
			foreach (Guild guild in Guilds)
			{
				guild.SendMessageToGuildMembers(msg, type, loc);
			}
		}
	}

	/// <summary>
	/// Loads this alliance from an alliance table
	/// </summary>
	/// <param name="obj"></param>
	public void LoadFromDatabase(DataObject obj)
	{
		if (!(obj is DBAlliance))
			return;

		m_dballiance = (DBAlliance)obj;
	}

	/// <summary>
	/// Saves this alliance to database
	/// </summary>
	public void SaveIntoDatabase()
	{
		GameServer.Database.SaveObject(m_dballiance);
		lock (Guilds.SyncRoot)
		{
			foreach (Guild guild in Guilds)
			{
				guild.SaveIntoDatabase();
			}
		}
	}
}