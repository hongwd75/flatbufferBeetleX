﻿using Game.Logic.Language;
using Game.Logic.Skills;

namespace Game.Logic.Effects;

public class StealthEffect : StaticEffect, IGameEffect
{
    /// <summary>
    /// The owner of the effect
    /// </summary>
    GamePlayer m_player;

    /// <summary>
    /// Start the stealth on player
    /// </summary>
    public void Start(GamePlayer player)
    {
        m_player = player;
        player.EffectList.Add(this);
    }

    /// <summary>
    /// Stop the effect on target
    /// </summary>
    public override void Stop()
    {
        if (m_player.HasAbility(Abilities.Camouflage))
        {
            IGameEffect camouflage = m_player.EffectList.GetOfType<CamouflageEffect>();
            if (camouflage!=null)
                camouflage.Cancel(false);
        }
        m_player.EffectList.Remove(this);
    }

    /// <summary>
    /// Called when effect must be canceled
    /// </summary>
    public override void Cancel(bool playerCancel)
    {
        m_player.Stealth(false);
    }

    /// <summary>
    /// Name of the effect
    /// </summary>
    public override string Name { get { return LanguageMgr.GetTranslation(m_player.Network, "Effects.StealthEffect.Name"); } }

    /// <summary>
    /// Remaining Time of the effect in milliseconds
    /// </summary>
    public override int RemainingTime { get { return 0; } }

    /// <summary>
    /// Icon to show on players, can be id
    /// </summary>
    public override ushort Icon { get { return 0x193; } }

    /// <summary>
    /// Delve Info
    /// </summary>
    public override IList<string> DelveInfo { get { return Array.Empty<string>(); } }
}