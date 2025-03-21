﻿namespace Game.Logic.Events;

public class ScriptEvent : GameEvent
{
    /// <summary>
    /// Constructs a new ScriptEvent
    /// </summary>
    /// <param name="name">the event name</param>
    protected ScriptEvent(string name) : base(name)
    {
    }

    /// <summary>
    /// The Loaded event is fired whenever the scripts have loaded
    /// </summary>
    public static readonly ScriptEvent Loaded = new ScriptEvent("Script.Loaded");
    /// <summary>
    /// The Unloaded event is fired whenever the scripts should unload
    /// </summary>
    public static readonly ScriptEvent Unloaded = new ScriptEvent("Script.Unloaded");
}