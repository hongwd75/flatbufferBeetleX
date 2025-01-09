﻿using Game.Logic.network;
using Game.Logic.Skills;

namespace Game.Logic.ServerRules;

public interface IServerRules
{
    void Initialize();
    bool IsAllowedToConnect(GameClient client, string username);
    bool IsAllowedToAttack(GameLiving attacker, GameLiving defender, bool quiet);
    bool IsAllowedToCastSpell(GameLiving caster, GameLiving target, Spell spell, SpellLine spellLine);
    
    void OnLivingKilled(GameLiving living, GameObject killer);
    void OnNPCKilled(GameNPC killedNPC, GameObject killer);
    void OnPlayerKilled(GamePlayer killedPlayer, GameObject killer);
    
    bool IsSameRealm(GameLiving source, GameLiving target, bool quiet);
    string RulesDescription();
    
    
    string GetPlayerTitle(GamePlayer source, GamePlayer target);
    byte GetLivingRealm(GamePlayer player, GameLiving target);
    string GetPlayerName(GamePlayer source, GamePlayer target);
    string GetPlayerLastName(GamePlayer source, GamePlayer target);
    string GetPlayerGuildName(GamePlayer source, GamePlayer target);
    string GetPlayerPrefixName(GamePlayer source, GamePlayer target);
}