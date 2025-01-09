using System.Net;
using System.Reflection;
using Flatbuffers.Messages.Enums;
using Game.Logic.AI.Brain;
using Game.Logic.datatable;
using Game.Logic.Events;
using Game.Logic.Language;
using Game.Logic.network;
using Game.Logic.ServerProperties;
using Game.Logic.Skills;
using Game.Logic.World;
using log4net;
using Logic.database;
using Logic.database.table;
using NetworkMessage;

namespace Game.Logic.ServerRules;

public abstract class AbstractServerRules : IServerRules
{
    private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    protected GamePlayer.InvulnerabilityExpiredCallback m_invExpiredCallback;

    #region #### interface
    public virtual void Initialize()
    {
        GameEventManager.AddHandler(GamePlayerEvent.GameEntered, new GameEventHandler(OnGameEntered));
        GameEventManager.AddHandler(GamePlayerEvent.RegionChanged, new GameEventHandler(OnRegionChanged));
        GameEventManager.AddHandler(GamePlayerEvent.Released, new GameEventHandler(OnReleased));
        m_invExpiredCallback = new GamePlayer.InvulnerabilityExpiredCallback(ImmunityExpiredCallback);
    }

    public virtual bool IsAllowedToConnect(GameClient client, string username)
    {
	    if (client.IsConnected() == false)
	    {
		    return false;
	    }

	    // Ban account
		IList<DBBannedAccount> objs;
		objs = GameDB<DBBannedAccount>.SelectObjects(DB.Column(nameof(DBBannedAccount.Type)).IsEqualTo("A").Or(DB.Column(nameof(DBBannedAccount.Type)).IsEqualTo("B")).And(DB.Column(nameof(DBBannedAccount.Account)).IsEqualTo(username)));
		if (objs.Count > 0)
		{
			client.IsConnectedBan = true;
			client.Out.SendLoginDenied(eLoginError.AccountIsBannedFromThisServerType);
			log.Debug("IsAllowedToConnect deny access to username " + username);
			return false;
		}

		// Ban IP Address or range (example: 5.5.5.%)
		string accip = client.TcpEndpointAddress;
		objs = GameDB<DBBannedAccount>.SelectObjects(DB.Column(nameof(DBBannedAccount.Type)).IsEqualTo("I").Or(DB.Column(nameof(DBBannedAccount.Type)).IsEqualTo("B")).And(DB.Column(nameof(DBBannedAccount.Ip)).IsLike(accip)));
		if (objs.Count > 0)
		{
			client.IsConnectedBan = true;
			client.Out.SendLoginDenied(eLoginError.AccountIsBannedFromThisServerType);
			log.Debug("IsAllowedToConnect deny access to IP " + accip);
			return false;
		}

		// GameClient.eClientVersion min = (GameClient.eClientVersion)Properties.CLIENT_VERSION_MIN;
		// if (min != GameClient.eClientVersion.VersionNotChecked && client.Version < min)
		// {
		// 	client.IsConnected = false;
		// 	client.Out.SendLoginDenied(eLoginError.ClientVersionTooLow);
		// 	log.Debug("IsAllowedToConnect deny access to client version (too low) " + client.Version);
		// 	return false;
		// }
		//
		// GameClient.eClientVersion max = (GameClient.eClientVersion)Properties.CLIENT_VERSION_MAX;
		// if (max != GameClient.eClientVersion.VersionNotChecked && client.Version > max)
		// {
		// 	client.IsConnected = false;
		// 	client.Out.SendLoginDenied(eLoginError.NotAuthorizedToUseExpansionVersion);
		// 	log.Debug("IsAllowedToConnect deny access to client version (too high) " + client.Version);
		// 	return false;
		// }
		//
		// if (Properties.CLIENT_TYPE_MAX > -1)
		// {
		// 	GameClient.eClientType type = (GameClient.eClientType)Properties.CLIENT_TYPE_MAX;
		// 	if ((int)client.ClientType > (int)type)
		// 	{
		// 		client.IsConnected = false;
		// 		client.Out.SendLoginDenied(eLoginError.ExpansionPacketNotAllowed);
		// 		log.Debug("IsAllowedToConnect deny access to expansion pack.");
		// 		return false;
		// 	}
		// }
		
		Account account = GameServer.Database.FindObjectByKey<Account>(username);

		if (Properties.MAX_PLAYERS > 0)
		{
			if (GameServer.Instance.Clients.GetAllClients().Count >= Properties.MAX_PLAYERS)
			{
				// GMs are still allowed to enter server
				if (account == null || (account.PrivLevel == 1 && account.Status <= 0))
				{
					// Normal Players will not be allowed over the max
					client.IsConnectedBan = true;
					client.Out.SendLoginDenied(eLoginError.TooManyPlayersLoggedIn);
					log.Debug("IsAllowedToConnect deny access due to too many players.");
					return false;
				}

			}
		}

		if (Properties.STAFF_LOGIN)
		{
			if (account == null || account.PrivLevel == 1)
			{
				// GMs are still allowed to enter server
				// Normal Players will not be allowed to Log in
				client.IsConnectedBan = true;
				client.Out.SendLoginDenied(eLoginError.GameCurrentlyClosed);
				log.Debug("IsAllowedToConnect deny access; staff only login");
				return false;
			}
		}

		// 중복접속 처리
		if ((account == null || account.PrivLevel == 1) && client.TcpEndpointAddress != "not connected")
		{
			foreach (GameClient cln in GameServer.Instance.Clients.GetAllClients())
			{
				if (cln == null || client == cln) continue;
				if (cln.TcpEndpointAddress == client.TcpEndpointAddress)
				{
					if (cln.Account != null && cln.Account.PrivLevel > 1)
					{
						break;
					}
					client.IsConnectedBan = true;
					client.Out.SendLoginDenied(eLoginError.AccountAlreadyLoggedIntoOtherServer);
					log.Debug("IsAllowedToConnect deny access; dual login not allowed");
					return false;
				}
			}
		}
		return true;
    }

    public virtual bool IsAllowedToAttack(GameLiving attacker, GameLiving defender, bool quiet)
    {
		if (attacker == null || defender == null)
			return false;

		//dead things can't attack
		if (!defender.IsAlive || !attacker.IsAlive)
			return false;

		GamePlayer playerAttacker = attacker as GamePlayer;
		GamePlayer playerDefender = defender as GamePlayer;

		// if Pet, let's define the controller once
		if (defender is GameNPC)
			if ((defender as GameNPC).Brain is IControlledBrain)
				playerDefender = ((defender as GameNPC).Brain as IControlledBrain).GetPlayerOwner();

		if (attacker is GameNPC)
			if ((attacker as GameNPC).Brain is IControlledBrain)
				playerAttacker = ((attacker as GameNPC).Brain as IControlledBrain).GetPlayerOwner();

		if (playerDefender != null && (playerDefender.Network.ClientState == GameClient.eClientState.WorldEnter || playerDefender.IsInvulnerableToAttack))
		{
			if (!quiet)
				MessageToLiving(attacker, defender.Name + " is entering the game and is temporarily immune to PvP attacks!");
			return false;
		}

		if (playerAttacker != null && playerDefender != null)
		{
			// Attacker immunity
			if (playerAttacker.IsInvulnerableToAttack)
			{
				if (quiet == false) MessageToLiving(attacker, "You can't attack players until your PvP invulnerability timer wears off!");
				return false;
			}

			// Defender immunity
			if (playerDefender.IsInvulnerableToAttack)
			{
				if (quiet == false) MessageToLiving(attacker, defender.Name + " is temporarily immune to PvP attacks!");
				return false;
			}
		}

		// PEACE NPCs can't be attacked/attack
		if (attacker is GameNPC)
			if (((GameNPC)attacker).IsPeaceful)
				return false;
		if (defender is GameNPC)
			if (((GameNPC)defender).IsPeaceful)
				return false;
		// Players can't attack mobs while they have immunity
		if (playerAttacker != null && defender != null)
		{
			if ((defender is GameNPC) && (playerAttacker.IsInvulnerableToAttack))
			{
				if (quiet == false) MessageToLiving(attacker, "You can't attack until your PvP invulnerability timer wears off!");
				return false;
			}
		}
		// Your pet can only attack stealthed players you have selected
		if (defender.IsStealthed && attacker is GameNPC)
			if (((attacker as GameNPC).Brain is IControlledBrain) &&
				defender is GamePlayer &&
				playerAttacker.TargetObject != defender)
				return false;

		// GMs can't be attacked
		if (playerDefender != null && playerDefender.Network.Account.PrivLevel > 1)
			return false;

		// Safe area support for defender
		foreach (AbstractArea area in defender.CurrentAreas)
		{
			if (!area.IsSafeArea)
				continue;

			if (defender is GamePlayer)
			{
				if (quiet == false) MessageToLiving(attacker, "You can't attack someone in a safe area!");
				return false;
			}
		}

		//safe area support for attacker
		foreach (AbstractArea area in attacker.CurrentAreas)
		{
			if ((area.IsSafeArea) && (defender is GamePlayer) && (attacker is GamePlayer))
			{
				if (quiet == false) MessageToLiving(attacker, "You can't attack someone in a safe area!");
				return false;
			}
		}
		
		return true;
    }

    public virtual bool IsAllowedToCastSpell(GameLiving caster, GameLiving target, Spell spell, SpellLine spellLine)
    {
	    return true;
    }

    public virtual void OnLivingKilled(GameLiving living, GameObject killer)
    {
        // 경험치 관련 필요시 기능 추가
    }

    public virtual void OnNPCKilled(GameNPC killedNPC, GameObject killer)
    {
	    // 경험치 관련 필요시 기능 추가
    }

    public virtual void OnPlayerKilled(GamePlayer killedPlayer, GameObject killer)
    {
	    // 경험치 관련 필요시 기능 추가
    }
    #endregion

    #region #### Callback
    public virtual void OnGameEntered(GameEvent e, object sender, EventArgs args)
    {
        StartImmunityTimer((GamePlayer)sender, ServerProperties.Properties.TIMER_GAME_ENTERED * 1000);
    }
    public virtual void OnRegionChanged(GameEvent e, object sender, EventArgs args)
    {
        StartImmunityTimer((GamePlayer)sender, ServerProperties.Properties.TIMER_REGION_CHANGED * 1000);
    }
    public virtual void OnReleased(GameEvent e, object sender, EventArgs args)
    {
        GamePlayer player = (GamePlayer)sender;
        StartImmunityTimer(player, ServerProperties.Properties.TIMER_KILLED_BY_MOB * 1000);//When Killed by a Mob
    }
    public virtual void StartImmunityTimer(GamePlayer player, int duration)
    {
        if (duration > 0)
        {
            player.StartInvulnerabilityTimer(duration, m_invExpiredCallback);
        }
    }
    public virtual void ImmunityExpiredCallback(GamePlayer player)
    {
        if (player.ObjectState != GameObject.eObjectState.Active) return;
        if (player.Network == null || player.Network.IsPlaying == false) return;
        player.Network.Out.SendMessage("Your temporary invulnerability timer has expired.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
        return;
    }    
    #endregion
    
    public abstract bool IsSameRealm(GameLiving source, GameLiving target, bool quiet);
    public abstract string RulesDescription();
    
    #region MessageToLiving
    public virtual void MessageToLiving(GameLiving living, string message)
    {
	    MessageToLiving(living, message, eChatType.CT_System, eChatLoc.CL_SystemWindow);
    }
    public virtual void MessageToLiving(GameLiving living, string message, eChatType type)
    {
	    MessageToLiving(living, message, type, eChatLoc.CL_SystemWindow);
    }
    public virtual void MessageToLiving(GameLiving living, string message, eChatType type, eChatLoc loc)
    {
	    if (living is GamePlayer)
		    ((GamePlayer)living).Network?.Out.SendMessage(message, type, loc);
    }
    #endregion

    #region get Player info
    public virtual byte GetLivingRealm(GamePlayer player, GameLiving target)
    {
	    if (player == null || target == null) return 0;

	    // clients with priv level > 1 are considered friendly by anyone
	    GamePlayer playerTarget = target as GamePlayer;
	    if (playerTarget != null && playerTarget.Network.Account.PrivLevel > 1) return (byte)player.Realm;

	    return (byte)target.Realm;
    }
    
    public virtual string GetPlayerName(GamePlayer source, GamePlayer target)
    {
	    return target.Name;
    }
    
    public virtual string GetPlayerLastName(GamePlayer source, GamePlayer target)
    {
	    return target.LastName;
    }
    
    public virtual string GetPlayerGuildName(GamePlayer source, GamePlayer target)
    {
	    return target.GuildName;
    }
    
    public virtual string GetPlayerTitle(GamePlayer source, GamePlayer target)
    {
	    return target.CurrentTitle.GetValue(source, target);
    }
    
    public virtual string GetPlayerPrefixName(GamePlayer source, GamePlayer target)
    {
	    string language;

	    try
	    {
		    language = source.Network.Account.Language;
	    }
	    catch
	    {
		    language = LanguageMgr.DefaultLanguage;
	    }

	    if (IsSameRealm(source, target, true) && target.RealmLevel >= 110)
		    return target.RealmRankTitle(language);

	    return string.Empty;
    }    
    #endregion
}