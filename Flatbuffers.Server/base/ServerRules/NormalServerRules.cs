using System.Collections;
using Game.Logic.AI.Brain;
using Game.Logic.Inventory;
using Game.Logic.network;
using Game.Logic.World;
using Logic.database.table;

namespace Game.Logic.ServerRules;

[ServerRules(eGameServerType.GST_Normal)]
public class NormalServerRules : AbstractServerRules
{
	public override string RulesDescription()
	{
		return "standard Normal server rules";
	}

	/// <summary>
	/// Invoked on NPC death and deals out
	/// experience/realm points if needed
	/// </summary>
	/// <param name="killedNPC">npc that died</param>
	/// <param name="killer">killer</param>
	public override void OnNPCKilled(GameNPC killedNPC, GameObject killer)
	{
		base.OnNPCKilled(killedNPC, killer); 	
	}

	public override bool IsAllowedToAttack(GameLiving attacker, GameLiving defender, bool quiet)
	{
		if (!base.IsAllowedToAttack(attacker, defender, quiet))
			return false;
		
		if (attacker is GameNPC)
		{
			IControlledBrain controlled = ((GameNPC)attacker).Brain as IControlledBrain;
			if (controlled != null)
			{
                attacker = controlled.GetLivingOwner();
				quiet = true; // silence all attacks by controlled npc
			}
		}
		
		if (defender is GameNPC)
		{
			IControlledBrain controlled = ((GameNPC)defender).Brain as IControlledBrain;
			if (controlled != null)
                defender = controlled.GetLivingOwner();
		}

		if(attacker == defender)
		{
			if (quiet == false) MessageToLiving(attacker, "You can't attack yourself!");
			return false;
		}

		if (attacker.Realm == defender.Realm)
		{
			// allow confused mobs to attack same realm
			if (attacker is GameNPC && (attacker as GameNPC).IsConfused)
				return true;
			
			if(quiet == false) MessageToLiving(attacker, "You can't attack a member of your realm!");
			return false;
		}

		return true;
	}

	public override bool IsSameRealm(GameLiving source, GameLiving target, bool quiet)
	{
		if(source == null || target == null) 
			return false;

		// if controlled NPC - do checks for owner instead
		if (source is GameNPC)
		{
			IControlledBrain controlled = ((GameNPC)source).Brain as IControlledBrain;
			if (controlled != null)
			{
                source = controlled.GetLivingOwner();
				quiet = true; // silence all attacks by controlled npc
			}
		}
		if (target is GameNPC)
		{
			IControlledBrain controlled = ((GameNPC)target).Brain as IControlledBrain;
			if (controlled != null)
                target = controlled.GetLivingOwner();
		}

		if (source == target)
			return true;

		// clients with priv level > 1 are considered friendly by anyone
		if(target is GamePlayer && ((GamePlayer)target).Network.Account.PrivLevel > 1) return true;
		// checking as a gm, targets are considered friendly
		if (source is GamePlayer && ((GamePlayer)source).Network.Account.PrivLevel > 1) return true;

		//Peace flag NPCs are same realm
		if (target is GameNPC)
			if (((GameNPC)target).IsPeaceful)
				return true;

		if (source is GameNPC)
			if (((GameNPC)source).IsPeaceful)
				return true;

		if(source.Realm != target.Realm)
		{
			if(quiet == false) MessageToLiving(source, target.GetName(0, true) + " is not a member of your realm!");
			return false;
		}
		return true;
	}

	public override bool IsAllowedCharsInAllRealms(GameClient client)
	{
		if (client.Account.PrivLevel > 1)
			return true;
		return false;
	}

	public override bool IsAllowedToGroup(GamePlayer source, GamePlayer target, bool quiet)
	{
		if(source == null || target == null) return false;

		if(source.Realm != target.Realm)
		{
			if(quiet == false) MessageToLiving(source, "You can't invite a player of another realm.");
			return false;
		}
		return true;
	}


	public override bool IsAllowedToJoinGuild(GamePlayer source, Guild.Guild guild)
	{
		if (source == null) 
			return false;

		if (guild.Realm != eRealm.None && source.Realm != guild.Realm)
		{
			return false;
		}

		return true;
	}

	public override bool IsAllowedToTrade(GameLiving source, GameLiving target, bool quiet)
	{
		if(source == null || target == null) return false;

		// clients with priv level > 1 are allowed to trade with anyone
		if(source is GamePlayer && target is GamePlayer)
		{
			if ((source as GamePlayer).Network.Account.PrivLevel > 1 ||(target as GamePlayer).Network.Account.PrivLevel > 1)
				return true;
		}

		//Peace flag NPCs can trade with everyone
		if (target is GameNPC)
			if (((GameNPC)target).IsPeaceful)
				return true;

		if (source is GameNPC)
			if (((GameNPC)source).IsPeaceful)
				return true;

		if(source.Realm != target.Realm)
		{
			if(quiet == false) MessageToLiving(source, "You can't trade with enemy realm!");
			return false;
		}
		return true;
	}

	public override bool IsAllowedToUnderstand(GameLiving source, GamePlayer target)
	{
		if(source == null || target == null) return false;

		if(source is GamePlayer && ((GamePlayer)source).Network.Account.PrivLevel > 1) return true;
		if(target.Network.Account.PrivLevel > 1) return true;

		if (source is GameNPC)
			if (((GameNPC)source).IsPeaceful)
				return true;

		if(source.Realm > 0 && source.Realm != target.Realm) return false;
		return true;
	}

	public override bool IsAllowedToBind(GamePlayer player, BindPoint point)
	{
		if (point.Realm == 0) return true;
		return player.Realm == (eRealm)point.Realm;
	}

	protected override eObjectType[] GetCompatibleObjectTypes(eObjectType objectType)
	{
		if(m_compatibleObjectTypes == null)
		{
			m_compatibleObjectTypes = new Hashtable();
			m_compatibleObjectTypes[(int)eObjectType.Staff] = new eObjectType[] { eObjectType.Staff };
			m_compatibleObjectTypes[(int)eObjectType.Fired] = new eObjectType[] { eObjectType.Fired };
            m_compatibleObjectTypes[(int)eObjectType.MaulerStaff] = new eObjectType[] { eObjectType.MaulerStaff };
			m_compatibleObjectTypes[(int)eObjectType.FistWraps] = new eObjectType[] { eObjectType.FistWraps };

			//alb
			m_compatibleObjectTypes[(int)eObjectType.CrushingWeapon]  = new eObjectType[] { eObjectType.CrushingWeapon };
			m_compatibleObjectTypes[(int)eObjectType.SlashingWeapon]  = new eObjectType[] { eObjectType.SlashingWeapon };
			m_compatibleObjectTypes[(int)eObjectType.ThrustWeapon]    = new eObjectType[] { eObjectType.ThrustWeapon };
			m_compatibleObjectTypes[(int)eObjectType.TwoHandedWeapon] = new eObjectType[] { eObjectType.TwoHandedWeapon };
			m_compatibleObjectTypes[(int)eObjectType.PolearmWeapon]   = new eObjectType[] { eObjectType.PolearmWeapon };
			m_compatibleObjectTypes[(int)eObjectType.Flexible]        = new eObjectType[] { eObjectType.Flexible };
			m_compatibleObjectTypes[(int)eObjectType.Longbow]         = new eObjectType[] { eObjectType.Longbow };
			m_compatibleObjectTypes[(int)eObjectType.Crossbow]        = new eObjectType[] { eObjectType.Crossbow };
			//TODO: case 5: abilityCheck = Abilities.Weapon_Thrown; break;                                         

			//mid
			m_compatibleObjectTypes[(int)eObjectType.Hammer]       = new eObjectType[] { eObjectType.Hammer };
			m_compatibleObjectTypes[(int)eObjectType.Sword]        = new eObjectType[] { eObjectType.Sword };
			m_compatibleObjectTypes[(int)eObjectType.LeftAxe]      = new eObjectType[] { eObjectType.LeftAxe };
			m_compatibleObjectTypes[(int)eObjectType.Axe]          = new eObjectType[] { eObjectType.Axe };
			m_compatibleObjectTypes[(int)eObjectType.HandToHand]   = new eObjectType[] { eObjectType.HandToHand };
			m_compatibleObjectTypes[(int)eObjectType.Spear]        = new eObjectType[] { eObjectType.Spear };
			m_compatibleObjectTypes[(int)eObjectType.CompositeBow] = new eObjectType[] { eObjectType.CompositeBow };
			m_compatibleObjectTypes[(int)eObjectType.Thrown]       = new eObjectType[] { eObjectType.Thrown };

			//hib
			m_compatibleObjectTypes[(int)eObjectType.Blunt]        = new eObjectType[] { eObjectType.Blunt };
			m_compatibleObjectTypes[(int)eObjectType.Blades]       = new eObjectType[] { eObjectType.Blades };
			m_compatibleObjectTypes[(int)eObjectType.Piercing]     = new eObjectType[] { eObjectType.Piercing };
			m_compatibleObjectTypes[(int)eObjectType.LargeWeapons] = new eObjectType[] { eObjectType.LargeWeapons };
			m_compatibleObjectTypes[(int)eObjectType.CelticSpear]  = new eObjectType[] { eObjectType.CelticSpear };
			m_compatibleObjectTypes[(int)eObjectType.Scythe]       = new eObjectType[] { eObjectType.Scythe };
			m_compatibleObjectTypes[(int)eObjectType.RecurvedBow]  = new eObjectType[] { eObjectType.RecurvedBow };

			m_compatibleObjectTypes[(int)eObjectType.Shield]       = new eObjectType[] { eObjectType.Shield };
			m_compatibleObjectTypes[(int)eObjectType.Poison]       = new eObjectType[] { eObjectType.Poison };
			//TODO: case 45: abilityCheck = Abilities.instruments; break;
		}

		eObjectType[] res = (eObjectType[])m_compatibleObjectTypes[(int)objectType];
		if(res == null)
			return Array.Empty<eObjectType>();
		return res;
	}
	
	public override string GetPlayerName(GamePlayer source, GamePlayer target)
	{
		return target.Name;
	}
	
	public override string GetPlayerLastName(GamePlayer source, GamePlayer target)
	{
		return target.LastName;
	}

	public override string GetPlayerGuildName(GamePlayer source, GamePlayer target)
	{
		if (IsSameRealm(source, target, true))
			return target.GuildName;
		return string.Empty;
	}
}