﻿namespace Game.Logic.Events;

public class GameNPCEvent : GameLivingEvent
	{
		/// <summary>
		/// Constructs a new GameNPCEvent
		/// </summary>
		/// <param name="name">the event name</param>
		protected GameNPCEvent(string name) : base(name)
		{
		}

		/// <summary>
		/// Tests if this event is valid for the specified object
		/// </summary>
		/// <param name="o">The object for which the event wants to be registered</param>
		/// <returns>true if valid, false if not</returns>
		public override bool IsValidFor(object o)
		{
			return o is GameNPC;
		}

		/// <summary>
		/// The TurnTo event is fired whenever the npc turns towards some coordinates
		/// <seealso cref="TurnToEventArgs"/>
		/// </summary>
		public static readonly GameNPCEvent TurnTo = new GameNPCEvent("GameNPC.TurnTo");
		/// <summary>
		/// The TurnToHeading event is fired whenever the npc turns towards a specific heading
		/// <seealso cref="TurnToHeadingEventArgs"/>
		/// </summary>
		public static readonly GameNPCEvent TurnToHeading = new GameNPCEvent("GameNPC.TurnToHeading");
		/// <summary>
		/// The ArriveAtTarget event is fired whenever the npc arrives at its WalkTo target
		/// <see cref="DOL.GS.GameNPC.WalkTo(int, int, int, int)"/>
		/// </summary>
		public static readonly GameNPCEvent ArriveAtTarget = new GameNPCEvent("GameNPC.ArriveAtTarget");
        /// <summary>
        /// The ArriveAtSpawnPoint event is fired whenever the npc arrives at its spawn point
        /// <see cref="DOL.GS.GameNPC.WalkTo(int, int, int, int)"/>
        /// </summary>
        public static readonly GameNPCEvent ArriveAtSpawnPoint = new GameNPCEvent("GameNPC.ArriveAtSpawnPoint");
		/// <summary>
		/// The CloseToTarget event is fired whenever the npc is close to its WalkTo target
		/// <see cref="DOL.GS.GameNPC.WalkTo(int, int, int, int)"/>
		/// </summary>
		public static readonly GameNPCEvent CloseToTarget = new GameNPCEvent("GameNPC.CloseToTarget");
		/// <summary>
		/// The WalkTo event is fired whenever the npc is commanded to walk to a specific target
		/// <seealso cref="WalkToEventArgs"/>
		/// </summary>
		public static readonly GameNPCEvent WalkTo = new GameNPCEvent("GameNPC.WalkTo");
		/// <summary>
		/// The Walk event is fired whenever the npc is commanded to walk
		/// <seealso cref="WalkEventArgs"/>
		/// </summary>
		public static readonly GameNPCEvent Walk = new GameNPCEvent("GameNPC.Walk");
		/// <summary>
		/// The RiderMount event is fired whenever the npc is mounted by a ride
		/// <seealso cref="RiderMountEventArgs"/>
		/// </summary>
		public static readonly GameNPCEvent RiderMount = new GameNPCEvent("GameNPC.RiderMount");
		/// <summary>
		/// The RiderDismount event is fired whenever the rider dismounts from the npc
		/// <seealso cref="RiderDismountEventArgs"/>
		/// </summary>
		public static readonly GameNPCEvent RiderDismount = new GameNPCEvent("GameNPC.RiderDismount");
		/// <summary>
		/// Fired when pathing starts
		/// </summary>
		public static readonly GameNPCEvent PathMoveStarts = new GameNPCEvent("GameNPC.PathMoveStarts");
		/// <summary>
		/// Fired when npc is on end of path
		/// </summary>
		public static readonly GameNPCEvent PathMoveEnds = new GameNPCEvent("GameNPC.PathMoveEnds");
		/// <summary>
		/// Fired on every AI callback
		/// </summary>
		public static readonly GameNPCEvent OnAICallback = new GameNPCEvent("GameNPC.OnAICallback");
		/// <summary>
		/// Fired whenever following NPC lost its target
		/// </summary>
		public static readonly GameNPCEvent FollowLostTarget = new GameNPCEvent("GameNPC.FollowLostTarget");
		/// <summary>
		/// Fired whenever pet is supposed to cast a spell.
		/// </summary>
		public static readonly GameNPCEvent PetSpell = new GameNPCEvent("GameNPC.PetSpell");
		/// <summary>
		/// Fired whenever pet is out of tether range (necromancer).
		/// </summary>
		public static readonly GameNPCEvent OutOfTetherRange = new GameNPCEvent("GameNPC.OutOfTetherRange");
		/// <summary>
		/// Fired when pet is lost (necromancer).
		/// </summary>
		public static readonly GameNPCEvent PetLost = new GameNPCEvent("GameNPC.PetLost");
        /// <summary>
        /// The SwitchedTarget event is fired when an NPC changes its target.
        /// </summary>
        public static readonly GameLivingEvent SwitchedTarget = new GameNPCEvent("GameNPC.SwitchedTarget");
	}