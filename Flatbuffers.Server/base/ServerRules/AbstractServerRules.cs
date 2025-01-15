using System.Collections;
using System.Net;
using System.Reflection;
using Flatbuffers.Messages.Enums;
using Game.Logic.AI.Brain;
using Game.Logic.datatable;
using Game.Logic.Events;
using Game.Logic.Inventory;
using Game.Logic.Language;
using Game.Logic.network;
using Game.Logic.ServerProperties;
using Game.Logic.Skills;
using Game.Logic.Utils;
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
    
    public virtual string GetPlayerPrefixName(GamePlayer source, GamePlayer target)
    {
	    // string language;
	    //
	    // try
	    // {
		   //  language = source.Network.Account.Language;
	    // }
	    // catch
	    // {
		   //  language = LanguageMgr.DefaultLanguage;
	    // }
	    //
	    // if (IsSameRealm(source, target, true) && target.RealmLevel >= 110)
		   //  return target.RealmRankTitle(language);

	    return string.Empty;
    }    
    #endregion
  
	#region GetCompatibleObjectTypes
	protected Hashtable m_compatibleObjectTypes = null;
	protected virtual eObjectType[] GetCompatibleObjectTypes(eObjectType objectType)
	{
		if (m_compatibleObjectTypes == null)
		{
			m_compatibleObjectTypes = new Hashtable();
			m_compatibleObjectTypes[(int)eObjectType.Staff] = new eObjectType[] { eObjectType.Staff };
			m_compatibleObjectTypes[(int)eObjectType.Fired] = new eObjectType[] { eObjectType.Fired };

			m_compatibleObjectTypes[(int)eObjectType.FistWraps] = new eObjectType[] { eObjectType.FistWraps };
			m_compatibleObjectTypes[(int)eObjectType.MaulerStaff] = new eObjectType[] { eObjectType.MaulerStaff };

			//alb
			m_compatibleObjectTypes[(int)eObjectType.CrushingWeapon] = new eObjectType[] { eObjectType.CrushingWeapon, eObjectType.Blunt, eObjectType.Hammer };
			m_compatibleObjectTypes[(int)eObjectType.SlashingWeapon] = new eObjectType[] { eObjectType.SlashingWeapon, eObjectType.Blades, eObjectType.Sword, eObjectType.Axe };
			m_compatibleObjectTypes[(int)eObjectType.ThrustWeapon] = new eObjectType[] { eObjectType.ThrustWeapon, eObjectType.Piercing };
			m_compatibleObjectTypes[(int)eObjectType.TwoHandedWeapon] = new eObjectType[] { eObjectType.TwoHandedWeapon, eObjectType.LargeWeapons };
			m_compatibleObjectTypes[(int)eObjectType.PolearmWeapon] = new eObjectType[] { eObjectType.PolearmWeapon, eObjectType.CelticSpear, eObjectType.Spear };
			m_compatibleObjectTypes[(int)eObjectType.Flexible] = new eObjectType[] { eObjectType.Flexible };
			m_compatibleObjectTypes[(int)eObjectType.Longbow] = new eObjectType[] { eObjectType.Longbow };
			m_compatibleObjectTypes[(int)eObjectType.Crossbow] = new eObjectType[] { eObjectType.Crossbow };
			//TODO: case 5: abilityCheck = Abilities.Weapon_Thrown; break;

			//mid
			m_compatibleObjectTypes[(int)eObjectType.Hammer] = new eObjectType[] { eObjectType.Hammer, eObjectType.CrushingWeapon, eObjectType.Blunt };
			m_compatibleObjectTypes[(int)eObjectType.Sword] = new eObjectType[] { eObjectType.Sword, eObjectType.SlashingWeapon, eObjectType.Blades };
			m_compatibleObjectTypes[(int)eObjectType.LeftAxe] = new eObjectType[] { eObjectType.LeftAxe };
			m_compatibleObjectTypes[(int)eObjectType.Axe] = new eObjectType[] { eObjectType.Axe, eObjectType.SlashingWeapon, eObjectType.Blades, eObjectType.LeftAxe };
			m_compatibleObjectTypes[(int)eObjectType.HandToHand] = new eObjectType[] { eObjectType.HandToHand };
			m_compatibleObjectTypes[(int)eObjectType.Spear] = new eObjectType[] { eObjectType.Spear, eObjectType.CelticSpear, eObjectType.PolearmWeapon };
			m_compatibleObjectTypes[(int)eObjectType.CompositeBow] = new eObjectType[] { eObjectType.CompositeBow };
			m_compatibleObjectTypes[(int)eObjectType.Thrown] = new eObjectType[] { eObjectType.Thrown };

			//hib
			m_compatibleObjectTypes[(int)eObjectType.Blunt] = new eObjectType[] { eObjectType.Blunt, eObjectType.CrushingWeapon, eObjectType.Hammer };
			m_compatibleObjectTypes[(int)eObjectType.Blades] = new eObjectType[] { eObjectType.Blades, eObjectType.SlashingWeapon, eObjectType.Sword, eObjectType.Axe };
			m_compatibleObjectTypes[(int)eObjectType.Piercing] = new eObjectType[] { eObjectType.Piercing, eObjectType.ThrustWeapon };
			m_compatibleObjectTypes[(int)eObjectType.LargeWeapons] = new eObjectType[] { eObjectType.LargeWeapons, eObjectType.TwoHandedWeapon };
			m_compatibleObjectTypes[(int)eObjectType.CelticSpear] = new eObjectType[] { eObjectType.CelticSpear, eObjectType.Spear, eObjectType.PolearmWeapon };
			m_compatibleObjectTypes[(int)eObjectType.Scythe] = new eObjectType[] { eObjectType.Scythe };
			m_compatibleObjectTypes[(int)eObjectType.RecurvedBow] = new eObjectType[] { eObjectType.RecurvedBow };

			m_compatibleObjectTypes[(int)eObjectType.Shield] = new eObjectType[] { eObjectType.Shield };
			m_compatibleObjectTypes[(int)eObjectType.Poison] = new eObjectType[] { eObjectType.Poison };
			//TODO: case 45: abilityCheck = Abilities.instruments; break;
		}

		eObjectType[] res = (eObjectType[])m_compatibleObjectTypes[(int)objectType];
		if (res == null)
			return Array.Empty<eObjectType>();
		return res;
	}

	#endregion    
	public virtual bool CheckAbilityToUseItem(GameLiving living, ItemTemplate item)
	{
		if (living == null || item == null)
			return false;

		GamePlayer player = living as GamePlayer;

		if (player != null && player.Network.Account.PrivLevel > (uint)ePrivLevel.Player)
			return true;

		if ((item.Object_Type == 0 || item.Object_Type >= (int)eObjectType._FirstHouse) && item.Object_Type <= (int)eObjectType._LastHouse)
			return true;

		if (!ServerProperties.Properties.ALLOW_CROSS_REALM_ITEMS)
		{
			if (item.Realm != 0 && item.Realm != (int)living.Realm)
				return false;
		}

		if (player != null && !Util.IsEmpty(item.AllowedClasses, true))
		{
			if (!Util.SplitCSV(item.AllowedClasses, true).Contains(player.CharacterClass.ID.ToString()))
				return false;
		}

		//armor
		if (item.Object_Type >= (int)eObjectType._FirstArmor && item.Object_Type <= (int)eObjectType._LastArmor)
		{
			int armorAbility = -1;

			if (ServerProperties.Properties.ALLOW_CROSS_REALM_ITEMS && item.Item_Type != (int)eEquipmentItems.HEAD)
			{
				switch (player.Realm) // Choose based on player rather than item region
				{
					case eRealm.Albion: armorAbility = living.GetAbilityLevel(Abilities.AlbArmor); break;
					case eRealm.Hibernia: armorAbility = living.GetAbilityLevel(Abilities.HibArmor); break;
					case eRealm.Midgard: armorAbility =  living.GetAbilityLevel(Abilities.MidArmor); break;
					default: break;
				}
			}
			else
			{
				switch ((eRealm)item.Realm)
				{
					case eRealm.Albion: armorAbility = living.GetAbilityLevel(Abilities.AlbArmor); break;
					case eRealm.Hibernia: armorAbility = living.GetAbilityLevel(Abilities.HibArmor); break;
					case eRealm.Midgard: armorAbility = living.GetAbilityLevel(Abilities.MidArmor); break;
					default: // use old system
						armorAbility = Math.Max(armorAbility, living.GetAbilityLevel(Abilities.AlbArmor));
						armorAbility = Math.Max(armorAbility, living.GetAbilityLevel(Abilities.HibArmor));
						armorAbility = Math.Max(armorAbility, living.GetAbilityLevel(Abilities.MidArmor));
						break;
				}
			}
			switch ((eObjectType)item.Object_Type)
			{
				case eObjectType.GenericArmor: return armorAbility >= ArmorLevel.GenericArmor;
				case eObjectType.Cloth: return armorAbility >= ArmorLevel.Cloth;
				case eObjectType.Leather: return armorAbility >= ArmorLevel.Leather;
				case eObjectType.Reinforced:
				case eObjectType.Studded: return armorAbility >= ArmorLevel.Studded;
				case eObjectType.Scale:
				case eObjectType.Chain: return armorAbility >= ArmorLevel.Chain;
				case eObjectType.Plate: return armorAbility >= ArmorLevel.Plate;
				default: return false;
			}
		}

		// non-armors
		string abilityCheck = null;
		string[] otherCheck = Array.Empty<string>();

		switch ((eObjectType)item.Object_Type)
		{
			case eObjectType.GenericItem: return true;
			case eObjectType.GenericArmor: return true;
			case eObjectType.GenericWeapon: return true;
			case eObjectType.Staff: abilityCheck = Abilities.Weapon_Staves; break;
			case eObjectType.Fired: abilityCheck = Abilities.Weapon_Shortbows; break;
			case eObjectType.FistWraps: abilityCheck = Abilities.Weapon_FistWraps; break;
			case eObjectType.MaulerStaff: abilityCheck = Abilities.Weapon_MaulerStaff; break;

			//alb
			case eObjectType.CrushingWeapon:
				if (ServerProperties.Properties.ALLOW_CROSS_REALM_ITEMS)
					switch (living.Realm)
					{
						case eRealm.Albion: abilityCheck = Abilities.Weapon_Crushing; break;
						case eRealm.Hibernia: abilityCheck = Abilities.Weapon_Blunt; break;
						case eRealm.Midgard: abilityCheck = Abilities.Weapon_Hammers; break;
						default: break;
					} 
				else abilityCheck = Abilities.Weapon_Crushing;
				break;
			case eObjectType.SlashingWeapon:
				if (ServerProperties.Properties.ALLOW_CROSS_REALM_ITEMS)
					switch (living.Realm)
					{
						case eRealm.Albion: abilityCheck = Abilities.Weapon_Slashing; break;
						case eRealm.Hibernia: abilityCheck = Abilities.Weapon_Blades; break;
						case eRealm.Midgard: abilityCheck = Abilities.Weapon_Swords; break;
						default: break;
					}
				else abilityCheck = Abilities.Weapon_Slashing;
				break;
			case eObjectType.ThrustWeapon:
				if (ServerProperties.Properties.ALLOW_CROSS_REALM_ITEMS && living.Realm == eRealm.Hibernia)
					abilityCheck = Abilities.Weapon_Piercing;
				else
					abilityCheck = Abilities.Weapon_Thrusting;
				break;
			case eObjectType.TwoHandedWeapon:
				if (ServerProperties.Properties.ALLOW_CROSS_REALM_ITEMS && living.Realm == eRealm.Hibernia)
					abilityCheck = Abilities.Weapon_LargeWeapons;
				else abilityCheck = Abilities.Weapon_TwoHanded;
				break;
			case eObjectType.PolearmWeapon:
				if (ServerProperties.Properties.ALLOW_CROSS_REALM_ITEMS)
					switch (living.Realm)
					{
						case eRealm.Albion: abilityCheck = Abilities.Weapon_Polearms; break;
						case eRealm.Hibernia: abilityCheck = Abilities.Weapon_CelticSpear; break;
						case eRealm.Midgard: abilityCheck = Abilities.Weapon_Spears; break;
						default: break;
					}
				else abilityCheck = Abilities.Weapon_Polearms;
				break;
			case eObjectType.Longbow:
				otherCheck = new string[] { Abilities.Weapon_Longbows, Abilities.Weapon_Archery };
				break;
			case eObjectType.Crossbow: abilityCheck = Abilities.Weapon_Crossbow; break;
			case eObjectType.Flexible: abilityCheck = Abilities.Weapon_Flexible; break;
			//TODO: case 5: abilityCheck = Abilities.Weapon_Thrown;break;

			//mid
			case eObjectType.Sword:
				if (ServerProperties.Properties.ALLOW_CROSS_REALM_ITEMS)
					switch (living.Realm)
					{
						case eRealm.Albion: abilityCheck = Abilities.Weapon_Slashing; break;
						case eRealm.Hibernia: abilityCheck = Abilities.Weapon_Blades; break;
						case eRealm.Midgard: abilityCheck = Abilities.Weapon_Swords; break;
						default: break;
					}
				else abilityCheck = Abilities.Weapon_Swords; 
				break;
			case eObjectType.Hammer:
				if (ServerProperties.Properties.ALLOW_CROSS_REALM_ITEMS)
					switch (living.Realm)
					{
						case eRealm.Albion: abilityCheck = Abilities.Weapon_Crushing; break;
						case eRealm.Midgard: abilityCheck = Abilities.Weapon_Hammers; break;
						case eRealm.Hibernia: abilityCheck = Abilities.Weapon_Blunt; break;
						default: break;
					}
				else abilityCheck = Abilities.Weapon_Hammers; 
				break;
			case eObjectType.LeftAxe:
			case eObjectType.Axe:
				if (ServerProperties.Properties.ALLOW_CROSS_REALM_ITEMS)
					switch (living.Realm)
					{
						case eRealm.Albion: abilityCheck = Abilities.Weapon_Slashing; break;
						case eRealm.Hibernia: abilityCheck = Abilities.Weapon_Blades; break;
						case eRealm.Midgard: abilityCheck = Abilities.Weapon_Axes; break;
						default: break;
					}
				else abilityCheck = Abilities.Weapon_Axes; 
				break;
			case eObjectType.Spear:
				if (ServerProperties.Properties.ALLOW_CROSS_REALM_ITEMS)
					switch (living.Realm)
					{
						case eRealm.Albion: abilityCheck = Abilities.Weapon_Polearms; break;
						case eRealm.Hibernia: abilityCheck = Abilities.Weapon_CelticSpear; break;
						case eRealm.Midgard: abilityCheck = Abilities.Weapon_Spears; break;
						default: break;
					}
				else abilityCheck = Abilities.Weapon_Spears; 
				break;
			case eObjectType.CompositeBow:
				otherCheck = new string[] { Abilities.Weapon_CompositeBows, Abilities.Weapon_Archery };
				break;
			case eObjectType.Thrown: abilityCheck = Abilities.Weapon_Thrown; break;
			case eObjectType.HandToHand: abilityCheck = Abilities.Weapon_HandToHand; break;

			//hib
			case eObjectType.RecurvedBow:
				otherCheck = new string[] { Abilities.Weapon_RecurvedBows, Abilities.Weapon_Archery };
				break;
			case eObjectType.Blades:
				if (ServerProperties.Properties.ALLOW_CROSS_REALM_ITEMS)
					switch (living.Realm)
					{
						case eRealm.Albion: abilityCheck = Abilities.Weapon_Slashing; break;
						case eRealm.Hibernia: abilityCheck = Abilities.Weapon_Blades; break;
						case eRealm.Midgard: abilityCheck = Abilities.Weapon_Swords; break;
						default: break;
					}
				else abilityCheck = Abilities.Weapon_Blades; 
				break;
			case eObjectType.Blunt:
				if (ServerProperties.Properties.ALLOW_CROSS_REALM_ITEMS)
					switch (living.Realm)
					{
						case eRealm.Albion: abilityCheck = Abilities.Weapon_Crushing; break;
						case eRealm.Hibernia: abilityCheck = Abilities.Weapon_Blunt; break;
						case eRealm.Midgard: abilityCheck = Abilities.Weapon_Hammers; break;
						default: break;
					}
				else abilityCheck = Abilities.Weapon_Blunt;
				break;
			case eObjectType.Piercing:
				if (ServerProperties.Properties.ALLOW_CROSS_REALM_ITEMS && living.Realm == eRealm.Albion)
					abilityCheck = Abilities.Weapon_Thrusting;
				else abilityCheck = Abilities.Weapon_Piercing;
				break;
			case eObjectType.LargeWeapons:
				if (ServerProperties.Properties.ALLOW_CROSS_REALM_ITEMS && living.Realm == eRealm.Albion)
					abilityCheck = Abilities.Weapon_TwoHanded;
				else abilityCheck = Abilities.Weapon_LargeWeapons; break;
			case eObjectType.CelticSpear:
				if (ServerProperties.Properties.ALLOW_CROSS_REALM_ITEMS)
					switch (living.Realm)
					{
						case eRealm.Albion: abilityCheck = Abilities.Weapon_Polearms; break;
						case eRealm.Hibernia: abilityCheck = Abilities.Weapon_CelticSpear; break;
						case eRealm.Midgard: abilityCheck = Abilities.Weapon_Spears; break;
						default: break;
					}
				else abilityCheck = Abilities.Weapon_CelticSpear;
				break;
			case eObjectType.Scythe: abilityCheck = Abilities.Weapon_Scythe; break;

			//misc
			case eObjectType.Magical: return true;
			case eObjectType.Shield: return living.GetAbilityLevel(Abilities.Shield) >= item.Type_Damage;
			case eObjectType.Bolt: abilityCheck = Abilities.Weapon_Crossbow; break;
			case eObjectType.Arrow: otherCheck = new string[] { Abilities.Weapon_CompositeBows, Abilities.Weapon_Longbows, Abilities.Weapon_RecurvedBows, Abilities.Weapon_Shortbows }; break;
			case eObjectType.Poison: return living.GetModifiedSpecLevel(Specs.Envenom) > 0;
			case eObjectType.Instrument: return living.HasAbility(Abilities.Weapon_Instruments);
				//TODO: different shield sizes
		}

		if (abilityCheck != null && living.HasAbility(abilityCheck))
			return true;

		foreach (string str in otherCheck)
			if (living.HasAbility(str))
				return true;

		return false;
	}

	public abstract bool IsAllowedToBind(GamePlayer player, BindPoint point);
    public abstract bool IsSameRealm(GameLiving source, GameLiving target, bool quiet);
    public abstract bool IsAllowedCharsInAllRealms(GameClient client);
    public abstract bool IsAllowedToGroup(GamePlayer source, GamePlayer target, bool quiet);
    public abstract bool IsAllowedToJoinGuild(GamePlayer source, Guild.Guild guild);
    public abstract bool IsAllowedToTrade(GameLiving source, GameLiving target, bool quiet);
    public abstract bool IsAllowedToUnderstand(GameLiving source, GamePlayer target);
    public abstract string RulesDescription();
    
    
}