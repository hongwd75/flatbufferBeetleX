﻿using System.Text;
using Game.Logic.World.Timer;

namespace Game.Logic;

public abstract class RegionAction : GameTimer
{
    /// <summary>
    /// The source of the action
    /// </summary>
    protected readonly GameObject m_actionSource;

    /// <summary>
    /// Constructs a new region action
    /// </summary>
    /// <param name="actionSource">The action source</param>
    public RegionAction(GameObject actionSource) : base(actionSource.CurrentRegion.TimeManager)
    {
        if (actionSource == null)
            throw new ArgumentNullException("actionSource");
        m_actionSource = actionSource;
    }

    /// <summary>
    /// Returns short information about the timer
    /// </summary>
    /// <returns>Short info about the timer</returns>
    public override string ToString()
    {
        return new StringBuilder(base.ToString(), 128)
            .Append(" actionSource: (").Append(m_actionSource.ToString())
            .Append(')')
            .ToString();
    }
}