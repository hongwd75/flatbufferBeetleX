﻿using System.Collections;
using System.Reflection;
using Game.Logic.Effects;
using Game.Logic.Events;
using Game.Logic.Geometry;
using Game.Logic.Skills;
using Game.Logic.Spells;
using Game.Logic.World.Timer;
using log4net;

namespace Game.Logic.AI.Brain;

public class ControlledNpcBrain : StandardMobBrain, IControlledBrain
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		// note that a minimum distance is inforced in GameNPC
		public static readonly short MIN_OWNER_FOLLOW_DIST = 50;
		//4000 - rough guess, needs to be confirmed
		public static readonly short MAX_OWNER_FOLLOW_DIST = 5000; // setting this to max stick distance
		public static readonly short MIN_ENEMY_FOLLOW_DIST = 90;
		public static readonly short MAX_ENEMY_FOLLOW_DIST = 512;

        protected Coordinate tempPosition = Coordinate.Nowhere;

		/// <summary>
		/// Holds the controlling player of this brain
		/// </summary>
		protected readonly GameLiving m_owner;

		/// <summary>
		/// Holds the walk state of the brain
		/// </summary>
		protected eWalkState m_walkState;

		/// <summary>
		/// Holds the aggression level of the brain
		/// </summary>
		protected eAggressionState m_aggressionState;
		
		/// <summary>
		/// Allows to check if your target is stealthing - trying to escape your pet
		/// </summary>
		protected bool previousIsStealthed;

		/// <summary>
		/// Constructs new controlled npc brain
		/// </summary>
		/// <param name="owner"></param>
		public ControlledNpcBrain(GameLiving owner)
			: base()
		{
            if (owner == null)
                throw new ArgumentNullException("owner");

            m_owner = owner;
            m_aggressionState = eAggressionState.Defensive;
            m_walkState = eWalkState.Follow;
            if (owner is GameNPC && (owner as GameNPC).Brain is StandardMobBrain)
            {
                m_aggroLevel = ((owner as GameNPC).Brain as StandardMobBrain).AggroLevel;
            }
            else
                m_aggroLevel = 99;
            m_aggroMaxRange = 1500;
		}

		protected bool m_isMainPet = true;
		private bool checkAbility;

		/// <summary>
		/// Checks if this NPC is a permanent/charmed or timed pet
		/// </summary>
		public bool IsMainPet
		{
			get { return m_isMainPet; }
			set { m_isMainPet = value; }
		}

		/// <summary>
		/// The number of seconds/10 this brain will stay active even when no player is close
		/// Overriden. Returns int.MaxValue
		/// </summary>
		protected override int NoPlayersStopDelay
		{
			get { return int.MaxValue; }
		}

		/// <summary>
		/// The interval for thinking, set via server property, default is 1500 or every 1.5 seconds
		/// </summary>
		public override int ThinkInterval
		{
			get { return GS.ServerProperties.Properties.PET_THINK_INTERVAL; }
		}

		#region Control

		/// <summary>
		/// Gets the controlling owner of the brain
		/// </summary>
		public GameLiving Owner
		{
			get { return m_owner; }
		}

        /// <summary>
        /// Find the player owner of the pets at the top of the tree
        /// </summary>
        /// <returns>Player owner at the top of the tree.  If there was no player, then return null.</returns>
        public virtual GamePlayer GetPlayerOwner()
        {
            GameLiving owner = Owner;
            int i = 0;
            while (owner is GameNPC && owner != null)
            {
                i++;
                if (i > 50)
                    throw new Exception("GetPlayerOwner() from " + Owner.Name + "caused a cyclical loop.");
                //If this is a pet, get its owner
                if (((GameNPC)owner).Brain is IControlledBrain)
                    owner = ((IControlledBrain)((GameNPC)owner).Brain).Owner;
                //This isn't a pet, that means it's at the top of the tree.  This case will only happen if
                //owner is not a GamePlayer
                else
                    break;
            }
            //Return if we found the gameplayer
            if (owner is GamePlayer)
                return (GamePlayer)owner;
            //If the root owner was not a player or npc then make sure we know that something went wrong!
            if (!(owner is GameNPC))
                throw new Exception("Unrecognized owner: " + owner.GetType().FullName);
            //No GamePlayer at the top of the tree
            return null;
        }

        public virtual GameNPC GetNPCOwner()
        {
            if (!(Owner is GameNPC))
                return null;

            GameNPC owner = Owner as GameNPC;

            int i = 0;
            while (owner != null)
            {
                i++;
                if (i > 50)
                {
                    log.Error("Boucle itérative dans GetNPCOwner !");
                    break;
                }
                if (owner.Brain is IControlledBrain)
                {
                    if ((owner.Brain as IControlledBrain).Owner is GamePlayer)
                        return null;
                    else
                        owner = (owner.Brain as IControlledBrain).Owner as GameNPC;
                }
                else
                    break;
            }
            return owner;
        }

        public virtual GameLiving GetLivingOwner()
        {
            GamePlayer player = GetPlayerOwner();
            if (player != null)
                return player;

            GameNPC npc = GetNPCOwner();
            if (npc != null)
                return npc;

            return null;
        }

		/// <summary>
		/// Gets or sets the walk state of the brain
		/// </summary>
		public virtual eWalkState WalkState
		{
			get { return m_walkState; }
			set
			{
				m_walkState = value;
				UpdatePetWindow();
			}
		}

		/// <summary>
		/// Gets or sets the aggression state of the brain
		/// </summary>
		public virtual eAggressionState AggressionState
		{
			get { return m_aggressionState; }
			set
			{
				m_aggressionState = value;
				m_orderAttackTarget = null;
				if (m_aggressionState == eAggressionState.Passive)
				{
					ClearAggroList();
					Body.StopAttack();
					Body.TargetObject = null;
					if (WalkState == eWalkState.Follow)
						FollowOwner();
					else if (!tempPosition.Equals(Coordinate.Nowhere))
						Body.PathTo(tempPosition, Body.MaxSpeed);
				}
				AttackMostWanted();
			}
		}

		/// <summary>
		/// Attack the target on command
		/// </summary>
		/// <param name="target"></param>
		public virtual void Attack(GameObject target)
		{
			if (AggressionState == eAggressionState.Passive)
			{
				AggressionState = eAggressionState.Defensive;
				UpdatePetWindow();
			}
			m_orderAttackTarget = target as GameLiving;
			previousIsStealthed = false;
			if (target is GamePlayer) 
				previousIsStealthed = (target as GamePlayer).IsStealthed;
			AttackMostWanted();
		}

		/// <summary>
		/// Follow the target on command
		/// </summary>
		/// <param name="target"></param>
		public virtual void Follow(GameObject target)
		{
			WalkState = eWalkState.Follow;
			Body.Follow(target, MIN_OWNER_FOLLOW_DIST, MAX_OWNER_FOLLOW_DIST);
		}

		/// <summary>
		/// Stay at current position on command
		/// </summary>
		public virtual void Stay()
		{
            tempPosition = Body.Coordinate;
			WalkState = eWalkState.Stay;
			Body.StopFollowing();
		}

		/// <summary>
		/// Go to owner on command
		/// </summary>
		public virtual void ComeHere()
		{
            tempPosition = Body.Coordinate;
			WalkState = eWalkState.ComeHere;
			Body.StopFollowing();
			Body.PathTo(Owner.Coordinate, Body.MaxSpeed);
		}

		/// <summary>
		/// Go to targets location on command
		/// </summary>
		/// <param name="target"></param>
		public virtual void Goto(GameObject target)
		{
			tempPosition = Body.Coordinate;
			WalkState = eWalkState.GoTarget;
			Body.StopFollowing();
			Body.PathTo(target.Coordinate, Body.MaxSpeed);
		}

		public virtual void SetAggressionState(eAggressionState state)
		{
			AggressionState = state;
			UpdatePetWindow();
		}

		/// <summary>
		/// Updates the pet window
		/// </summary>
		public virtual void UpdatePetWindow()
		{
			if (m_owner is GamePlayer)
				((GamePlayer)m_owner).Out.SendPetWindow(m_body, ePetWindowAction.Update, m_aggressionState, m_walkState);
		}

		/// <summary>
		/// Start following the owner
		/// </summary>
		public virtual void FollowOwner()
		{
			Body.StopAttack();
			if (Owner is GamePlayer
			    && IsMainPet
			    && ((GamePlayer)Owner).CharacterClass.ID != (int)eCharacterClass.Animist
			    && ((GamePlayer)Owner).CharacterClass.ID != (int)eCharacterClass.Theurgist)
				Body.Follow(Owner, MIN_OWNER_FOLLOW_DIST, MAX_OWNER_FOLLOW_DIST);
			else if (Owner is GameNPC)
				Body.Follow(Owner, MIN_OWNER_FOLLOW_DIST, MAX_OWNER_FOLLOW_DIST);
		}

		#endregion

		#region AI

		/// <summary>
		/// The attack target ordered by the owner
		/// </summary>
		protected GameLiving m_orderAttackTarget;

		/// <summary>
		/// Starts the brain thinking and resets the inactivity countdown
		/// </summary>
		/// <returns>true if started</returns>
		public override bool Start()
		{
			if (!base.Start()) return false;
			if (WalkState == eWalkState.Follow)
				FollowOwner();
			// [Ganrod] On supprime la cible du pet au moment  du contrôle.
			Body.TargetObject = null;
			GameEventMgr.AddHandler(Owner, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(OnOwnerAttacked));

			return true;
		}

		/// <summary>
		/// Stops the brain thinking
		/// </summary>
		/// <returns>true if stopped</returns>
		public override bool Stop()
		{
			if (!base.Stop()) return false;
			GameEventMgr.RemoveHandler(Owner, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(OnOwnerAttacked));

			GameEventMgr.Notify(GameLivingEvent.PetReleased, Body);
			return true;
		}

		/// <summary>
		/// Do the mob AI
		/// </summary>
		public override void Think()
		{
			GamePlayer playerowner = GetPlayerOwner();

			long lastUpdate;
			if (!playerowner.Client.GameObjectUpdateArray.TryGetValue(new Tuple<ushort, ushort>(Body.CurrentRegionID, (ushort)Body.ObjectID), out lastUpdate))
				lastUpdate = 0;

			// Load abilities on first Think cycle.
			if (!checkAbility)
			{
				CheckAbilities();
				checkAbility = true;
			}

			if (playerowner != null && (GameTimer.GetTickCount() - lastUpdate) > ThinkInterval)
				playerowner.Out.SendObjectUpdate(Body);

			//See if the pet is too far away, if so release it!
			if (Owner is GamePlayer && IsMainPet && !Body.IsWithinRadius(Owner, MAX_OWNER_FOLLOW_DIST))
				(Owner as GamePlayer).CommandNpcRelease();

			// if pet is in agressive mode then check aggressive spells and attacks first
			if (!Body.AttackState && AggressionState == eAggressionState.Aggressive)
			{
				CheckPlayerAggro();
				CheckNPCAggro();
				AttackMostWanted();
			}

			// Check for buffs, heals, etc, interrupting melee if not being interrupted
			// Only prevent casting if we are ordering pet to come to us or go to target
			var petIsNotOrderedElsewhere = Owner is GamePlayer && WalkState != eWalkState.ComeHere && WalkState != eWalkState.GoTarget;
			if ((Owner is GameNPC || petIsNotOrderedElsewhere) && !Body.InCombat)
				CheckSpells(eCheckSpellType.Defensive);

			// Stop hunting player entering in steath
			if (Body.TargetObject != null && Body.TargetObject is GamePlayer)
			{
				GamePlayer player = Body.TargetObject as GamePlayer;
				if (Body.IsAttacking && player.IsStealthed && !previousIsStealthed)
				{
					Body.StopAttack();
					Body.StopCurrentSpellcast();
					RemoveFromAggroList(player);
					Body.TargetObject = null;
					FollowOwner();
				}
				previousIsStealthed = player.IsStealthed;
			}

			// Always check offensive spells, or pets in melee will keep blindly melee attacking,
			//	when they should be stopping to cast offensive spells.
			if (IsActive && m_aggressionState != eAggressionState.Passive)
				CheckSpells(eCheckSpellType.Offensive);

			if (!Body.AttackState && WalkState == eWalkState.Follow && Owner != null && !Body.InCombat)
				Follow(Owner);

			CheckStealth();
		}

		/// <summary>
		/// Checks the Abilities
		/// </summary>
		public override void CheckAbilities()
		{
			////load up abilities
			if (Body.Abilities != null && Body.Abilities.Count > 0)
			{
				foreach (Ability ab in Body.Abilities.Values)
				{
					switch (ab.KeyName)
					{
						case Abilities.Intercept:
							{
								if (GetPlayerOwner() is GamePlayer player)
									//the pet should intercept even if a player is till intercepting for the owner
									new InterceptEffect().Start(Body, player);
								break;
							}
						case Abilities.Guard:
							{
								if (GetPlayerOwner() is GamePlayer player)
									new GuardEffect().Start(Body, player);
								break;
							}
						case Abilities.Protect:
							{
								if (GetPlayerOwner() is GamePlayer player)
									new ProtectEffect().Start(player);
								break;
							}
						case Abilities.ChargeAbility:
							{
								if ( Body.TargetObject is GameLiving target
									&& GameServer.ServerRules.IsAllowedToAttack(Body, target, true) 
									&& !Body.IsWithinRadius( target, 500 ) )
								{
									ChargeAbility charge = Body.GetAbility<ChargeAbility>();
									if (charge != null && Body.GetSkillDisabledDuration(charge) <= 0)
									{
										charge.Execute(Body);
									}
								}
								break;
							}
					}
				}
			}
		}

		/// <summary>
		/// Checks if any spells need casting
		/// </summary>
		/// <param name="type">Which type should we go through and check for?</param>
		/// <returns>True if we are begin to cast or are already casting</returns>
		public override bool CheckSpells(eCheckSpellType type)
		{
			if (Body == null || Body.Spells == null || Body.Spells.Count < 1) return false;
			
			bool casted = false;
			if (type == eCheckSpellType.Defensive)
			{
				// Check instant spells, but only cast one of each type to prevent spamming
				if (Body.CanCastInstantHealSpells)
					foreach (Spell spell in Body.InstantHealSpells)
						if (CheckDefensiveSpells(spell))
							break;

				if (Body.CanCastInstantMiscSpells)
					foreach (Spell spell in Body.InstantMiscSpells)
						if (CheckDefensiveSpells(spell))
							break;

				if (!Body.IsCasting)
				{
					// Check spell lists, prioritizing healing
					if (Body.CanCastHealSpells)
						foreach (Spell spell in Body.HealSpells)
							if (CheckDefensiveSpells(spell))
							{
								casted = true;
								break;
							}

					if (!casted && Body.CanCastMiscSpells)
						foreach (Spell spell in Body.MiscSpells)
							if (CheckDefensiveSpells(spell))
							{
								casted = true;
								break;
							}
				}
			}
			else if (Body.TargetObject is GameLiving living && living.IsAlive)
			{
				// Check instant spells, but only cast one to prevent spamming
				if (Body.CanCastInstantHarmfulSpells)
					foreach (Spell spell in Body.InstantHarmfulSpells)
						if (CheckOffensiveSpells(spell))
							break;

				if (!Body.IsCasting && Body.CanCastHarmfulSpells)
					foreach (Spell spell in Body.HarmfulSpells)
						if (CheckOffensiveSpells(spell))
						{
							casted = true;
							break;
						}
			}

			return casted || Body.IsCasting;
		}

		/// <summary>
		/// Checks the Positive Spells.  Handles buffs, heals, etc.
		/// </summary>
		protected override bool CheckDefensiveSpells(Spell spell)
		{
			if (spell == null || spell.IsHarmful)
				return false;

			// Make sure we're currently able to cast the spell
			if (spell.CastTime > 0 && (Body.IsCasting || (Body.IsBeingInterrupted && !spell.Uninterruptible)))
				return false;

			// Make sure the spell isn't disabled
			if (spell.HasRecastDelay && Body.GetSkillDisabledDuration(spell) > 0)
				return false;

			bool casted = false;

			// clear current target, set target based on spell type, cast spell, return target to original target
			GameObject lastTarget = Body.TargetObject;
			Body.TargetObject = null;
			GamePlayer player = null;
			GameLiving owner = null;

			switch (spell.SpellType.ToUpper())
			{
                #region Buffs
                case "ACUITYBUFF":
                case "AFHITSBUFF":
                case "ALLMAGICRESISTSBUFF":
                case "ARMORABSORPTIONBUFF":
                case "ARMORFACTORBUFF":
                case "BODYRESISTBUFF":
                case "BODYSPIRITENERGYBUFF":
                case "BUFF":
                case "CELERITYBUFF":
                case "COLDRESISTBUFF":
                case "COMBATSPEEDBUFF":
                case "CONSTITUTIONBUFF":
                case "COURAGEBUFF":
                case "CRUSHSLASHTHRUSTBUFF":
                case "DEXTERITYBUFF":
                case "DEXTERITYQUICKNESSBUFF":
                case "EFFECTIVENESSBUFF":
                case "ENDURANCEREGENBUFF":
                case "ENERGYRESISTBUFF":
                case "FATIGUECONSUMPTIONBUFF":
                case "FELXIBLESKILLBUFF":
                case "HASTEBUFF":
                case "HEALTHREGENBUFF":
                case "HEATCOLDMATTERBUFF":
                case "HEATRESISTBUFF":
                case "HEROISMBUFF":
                case "KEEPDAMAGEBUFF":
                case "MAGICRESISTSBUFF":
                case "MATTERRESISTBUFF":
                case "MELEEDAMAGEBUFF":
                case "MESMERIZEDURATIONBUFF":
                case "MLABSBUFF":
                case "PALADINARMORFACTORBUFF":
                case "PARRYBUFF":
                case "POWERHEALTHENDURANCEREGENBUFF":
                case "POWERREGENBUFF":
                case "SAVAGECOMBATSPEEDBUFF":
                case "SAVAGECRUSHRESISTANCEBUFF":
                case "SAVAGEDPSBUFF":
                case "SAVAGEPARRYBUFF":
                case "SAVAGESLASHRESISTANCEBUFF":
                case "SAVAGETHRUSTRESISTANCEBUFF":
                case "SPIRITRESISTBUFF":
                case "STRENGTHBUFF":
                case "STRENGTHCONSTITUTIONBUFF":
                case "SUPERIORCOURAGEBUFF":
                case "TOHITBUFF":
                case "WEAPONSKILLBUFF":
                case "DAMAGEADD":
                case "OFFENSIVEPROC":
                case "DEFENSIVEPROC":
                case "DAMAGESHIELD":
				case "BLADETURN":
                    {
						String target;

						//Buff self
						if (!LivingHasEffect(Body, spell))
						{
                            Body.TargetObject = Body;
							break;
						}

						target = spell.Target.ToUpper();
							
						if (target == "SELF")
							break;
						
						if (target == "REALM" || target == "GROUP")
						{
							owner = (this as IControlledBrain).Owner;
							player = null;
							//Buff owner
							if (!LivingHasEffect(owner, spell) && Body.IsWithinRadius(owner, spell.Range))
							{
                                Body.TargetObject = owner;
								break;
							}

							if (owner is GameNPC npc)
							{
								//Buff other minions
								foreach (IControlledBrain icb in npc.ControlledNpcList)
								{
									if (icb != null && icb.Body != null && !LivingHasEffect(icb.Body, spell) 
										&& Body.IsWithinRadius(icb.Body, spell.Range))
									{
                                        Body.TargetObject = icb.Body;
										break;
									}
								}
							}

							player = GetPlayerOwner();

							//Buff player
							if (player != null)
							{
								if (!LivingHasEffect(player, spell))
								{
                                    Body.TargetObject = player;
									break;
								}

								if (player.Group != null)
								{
									foreach (GamePlayer p in player.Group.GetPlayersInTheGroup())
									{
										if (!LivingHasEffect(p, spell) && Body.IsWithinRadius(p, spell.Range))
										{
                                            Body.TargetObject = p;
											break;
										}
									}
								}
							}
						}
					}
					break;
					#endregion Buffs

					#region Disease Cure/Poison Cure/Summon
				case "CUREDISEASE":
					//Cure owner
					owner = (this as IControlledBrain).Owner;
					if (owner.IsDiseased)
					{
						Body.TargetObject = owner;
						break;
					}

					//Cure self
					if (Body.IsDiseased)
					{
						Body.TargetObject = Body;
						break;
					}

					// Cure group members

					player = GetPlayerOwner();

					if (player.Group != null)
					{
						foreach (GamePlayer p in player.Group.GetPlayersInTheGroup())
						{
							if (p.IsDiseased && Body.IsWithinRadius(p, spell.Range))
							{
								Body.TargetObject = p;
								break;
							}
						}
					}
					break;
				case "CUREPOISON":
					//Cure owner
					owner = (this as IControlledBrain).Owner;
					if (LivingIsPoisoned(owner))
					{
						Body.TargetObject = owner;
						break;
					}

					//Cure self
					if (LivingIsPoisoned(Body))
					{
						Body.TargetObject = Body;
						break;
					}

					// Cure group members

					player = GetPlayerOwner();

					if (player.Group != null)
					{
						foreach (GamePlayer p in player.Group.GetPlayersInTheGroup())
						{
							if (LivingIsPoisoned(p) && Body.IsWithinRadius(p, spell.Range))
							{
								Body.TargetObject = p;
								break;
							}
						}
					}
					break;
				case "SUMMON":
					Body.TargetObject = Body;
					break;
                #endregion

                #region Heals
                case "COMBATHEAL":
                case "HEAL":
                case "HEALOVERTIME":
                case "MERCHEAL":
                case "OMNIHEAL":
                case "PBAEHEAL":
                case "SPREADHEAL":
					String spellTarget = spell.Target.ToUpper();
					int bodyPercent = Body.HealthPercent;
					
					if (spellTarget == "SELF")
					{
						if (bodyPercent < GS.ServerProperties.Properties.NPC_HEAL_THRESHOLD && !spell.TargetHasEffect(Body))
							Body.TargetObject = Body;

						break;
					}

					// Heal seriously injured targets first
					int emergencyThreshold = GS.ServerProperties.Properties.NPC_HEAL_THRESHOLD / 2;

					//Heal owner
					owner = (this as IControlledBrain).Owner;
					int ownerPercent = owner.HealthPercent;
					if (ownerPercent < emergencyThreshold && !spell.TargetHasEffect(owner) && Body.IsWithinRadius(owner, spell.Range))
					{
						Body.TargetObject = owner;
						break;
					}

					//Heal self
					if (bodyPercent < emergencyThreshold
						&& !spell.TargetHasEffect(Body))
					{
						Body.TargetObject = Body;
						break;
					}

					// Heal group
					player = GetPlayerOwner();
					ICollection<GamePlayer> playerGroup = null;
					if (player.Group != null && (spellTarget == "REALM" || spellTarget == "GROUP"))
					{
						playerGroup = player.Group.GetPlayersInTheGroup();

						foreach (GamePlayer p in playerGroup)
						{
							if (p.HealthPercent < emergencyThreshold && !spell.TargetHasEffect(p)
								&& Body.IsWithinRadius(p, spell.Range))
							{
								Body.TargetObject = p;
								break;
							}
						}
					}

					// Now check for targets which aren't seriously injured

					if (spellTarget == "SELF")
					{
						// if we have a self heal and health is less than 75% then heal, otherwise return false to try another spell or do nothing
						if (bodyPercent < GS.ServerProperties.Properties.NPC_HEAL_THRESHOLD
							&& !spell.TargetHasEffect(Body))
						{
							Body.TargetObject = Body;
						}
						break;
					}

					//Heal owner
					owner = (this as IControlledBrain).Owner;
					if (ownerPercent < GS.ServerProperties.Properties.NPC_HEAL_THRESHOLD
						&& !spell.TargetHasEffect(owner) && Body.IsWithinRadius(owner, spell.Range))
					{
						Body.TargetObject = owner;
						break;
					}

					//Heal self
					if (bodyPercent < GS.ServerProperties.Properties.NPC_HEAL_THRESHOLD
						&& !spell.TargetHasEffect(Body))
					{
						Body.TargetObject = Body;
						break;
					}

					// Heal group
					if (playerGroup != null)
					{
						foreach (GamePlayer p in playerGroup)
						{
							if (p.HealthPercent < GS.ServerProperties.Properties.NPC_HEAL_THRESHOLD 
								&& !spell.TargetHasEffect(p) && Body.IsWithinRadius(p, spell.Range))
							{
								Body.TargetObject = p;
								break;
							}
						}
					}
					break;
				#endregion

				default:
					log.Warn($"CheckDefensiveSpells() encountered an unknown spell type [{spell.SpellType}], calling base method");
					return base.CheckDefensiveSpells(spell);
			}

			if (Body.TargetObject != null)
            {
				casted = Body.CastSpell(spell, m_mobSpellLine);

				if (casted && spell.CastTime > 0)
				{
					if (Body.IsMoving)
						Body.StopFollowing();

					if (Body.TargetObject != Body)
						Body.TurnTo(Body.TargetObject);
				}
			}

			Body.TargetObject = lastTarget;

			return casted;
		}

		// Temporary until StandardMobBrain is updated
		protected override bool CheckOffensiveSpells(Spell spell)
		{
			if (spell == null || spell.IsHelpful || !(Body.TargetObject is GameLiving living) || !living.IsAlive)
				return false;

			// Make sure we're currently able to cast the spell
			if (spell.CastTime > 0 && (Body.IsCasting || (Body.IsBeingInterrupted && !spell.Uninterruptible)))
				return false;

			// Make sure the spell isn't disabled
			if (spell.HasRecastDelay && Body.GetSkillDisabledDuration(spell) > 0)
				return false;

			if (!Body.IsWithinRadius(Body.TargetObject, spell.Range))
				return false;

			return base.CheckOffensiveSpells(spell);
		}

		/// <summary>
		/// Lost follow target event
		/// </summary>
		/// <param name="target"></param>
		protected override void OnFollowLostTarget(GameObject target)
		{
			if (target == Owner)
			{
				GameEventMgr.Notify(GameLivingEvent.PetReleased, Body);
				return;
			}

			FollowOwner();
		}

		/// <summary>
		/// Add living to the aggrolist
		/// aggroamount can be negative to lower amount of aggro
		/// </summary>
		/// <param name="living"></param>
		/// <param name="aggroamount"></param>
		public override void AddToAggroList(GameLiving living, int aggroamount)
		{
            GameNPC npc_owner = GetNPCOwner();
            if (npc_owner == null || !(npc_owner.Brain is StandardMobBrain))
                base.AddToAggroList(living, aggroamount);
            else
            {
                (npc_owner.Brain as StandardMobBrain).AddToAggroList(living, aggroamount);
            }
		}

		public override int CalculateAggroLevelToTarget(GameLiving target)
		{
			// only attack if target is green+ to OWNER; always attack higher levels regardless of CON
			if (GameServer.ServerRules.IsAllowedToAttack(Body, target, true) == false)
				return 0;

			return AggroLevel > 100 ? 100 : AggroLevel;
		}

		/// <summary>
		/// Returns the best target to attack
		/// </summary>
		/// <returns>the best target</returns>
		protected override GameLiving CalculateNextAttackTarget()
		{
			if (AggressionState == eAggressionState.Passive)
				return null;

			if (m_orderAttackTarget != null)
			{
				if (m_orderAttackTarget.IsAlive &&
				    m_orderAttackTarget.ObjectState == GameObject.eObjectState.Active &&
				    GameServer.ServerRules.IsAllowedToAttack(this.Body, m_orderAttackTarget, true))
				{
					return m_orderAttackTarget;
				}

				m_orderAttackTarget = null;
			}

			lock ((m_aggroTable as ICollection).SyncRoot)
			{
				IDictionaryEnumerator aggros = m_aggroTable.GetEnumerator();
				List<GameLiving> removable = new List<GameLiving>();
				while (aggros.MoveNext())
				{
					GameLiving living = (GameLiving)aggros.Key;

					if (living.IsMezzed ||
					    living.IsAlive == false ||
					    living.ObjectState != GameObject.eObjectState.Active ||
					    Body.GetDistanceTo(living.Position, 0) > MAX_AGGRO_LIST_DISTANCE ||
					    GameServer.ServerRules.IsAllowedToAttack(this.Body, living, true) == false)
					{
						removable.Add(living);
					}
					else
					{
						GameSpellEffect root = SpellHandler.FindEffectOnTarget(living, "SpeedDecrease");
						if (root != null && root.Spell.Value == 99)
						{
							removable.Add(living);
						}
					}
				}

				foreach (GameLiving living in removable)
				{
					RemoveFromAggroList(living);
					Body.RemoveAttacker(living);
				}
			}

			return base.CalculateNextAttackTarget();
		}

		/// <summary>
		/// Selects and attacks the next target or does nothing
		/// </summary>
		protected override void AttackMostWanted()
		{
			if (!IsActive || m_aggressionState == eAggressionState.Passive) return;

            GameNPC owner_npc = GetNPCOwner();
            if (owner_npc != null && owner_npc.Brain is StandardMobBrain)
            {
                if ((owner_npc.IsCasting || owner_npc.IsAttacking) &&
                    owner_npc.TargetObject != null &&
                    owner_npc.TargetObject is GameLiving &&
                    GameServer.ServerRules.IsAllowedToAttack(owner_npc, owner_npc.TargetObject as GameLiving, false))
                {

                    if (!CheckSpells(eCheckSpellType.Offensive))
                    {
                        Body.StartAttack(owner_npc.TargetObject);
                    }
                    return;
                }
            }

			GameLiving target = CalculateNextAttackTarget();

			if (target != null)
			{
				if (!Body.IsAttacking || target != Body.TargetObject)
				{
					Body.TargetObject = target;

					Body.LastAttackedTick = Body.CurrentRegion.Time;
					Owner.LastAttackedByEnemyTick = Body.CurrentRegion.Time;

					List<GameSpellEffect> effects = new List<GameSpellEffect>();

					lock (Body.EffectList)
					{
						foreach (IGameEffect effect in Body.EffectList)
						{
							if (effect is GameSpellEffect && (effect as GameSpellEffect).SpellHandler is SpeedEnhancementSpellHandler)
							{
								effects.Add(effect as GameSpellEffect);
							}
						}
					}

					lock (Owner.EffectList)
					{
						foreach (IGameEffect effect in Owner.EffectList)
						{
							if (effect is GameSpellEffect && (effect as GameSpellEffect).SpellHandler is SpeedEnhancementSpellHandler)
							{
								effects.Add(effect as GameSpellEffect);
							}
						}
					}

					foreach (GameSpellEffect effect in effects)
					{
						effect.Cancel(false);
					}

					if (!CheckSpells(eCheckSpellType.Offensive))
					{
						Body.StartAttack(target);
					}
				}
			}
			else
			{
				Body.TargetObject = null;

				if (Body.IsAttacking)
					Body.StopAttack();

				if (Body.SpellTimer != null && Body.SpellTimer.IsAlive)
					Body.SpellTimer.Stop();

				if (WalkState == eWalkState.Follow)
				{
					FollowOwner();
				}
				else if (!tempPosition.Equals(Coordinate.Nowhere))
				{
					Body.PathTo(tempPosition, Body.MaxSpeed);
				}
			}
		}

		/// <summary>
		/// Owner attacked event
		/// </summary>
		/// <param name="e"></param>
		/// <param name="sender"></param>
		/// <param name="arguments"></param>
		protected virtual void OnOwnerAttacked(GameEvent e, object sender, EventArgs arguments)
		{
			AttackedByEnemyEventArgs args = arguments as AttackedByEnemyEventArgs;
			if (args == null) return;
			if (args.AttackData.Target is GamePlayer && (args.AttackData.Target as GamePlayer).ControlledBrain != this)
				return;
			// react only on these attack results
			switch (args.AttackData.AttackResult)
			{
				case GameLiving.eAttackResult.Blocked:
				case GameLiving.eAttackResult.Evaded:
				case GameLiving.eAttackResult.Fumbled:
				case GameLiving.eAttackResult.HitStyle:
				case GameLiving.eAttackResult.HitUnstyled:
				case GameLiving.eAttackResult.Missed:
				case GameLiving.eAttackResult.Parried:
					AddToAggroList(args.AttackData.Attacker, args.AttackData.Attacker.EffectiveLevel + args.AttackData.Damage + args.AttackData.CriticalDamage);
					break;
			}
			AttackMostWanted();
		}

		protected override void BringFriends(GameLiving trigger)
		{
			// don't
		}

		public override bool CheckFormation(ref int x, ref int y, ref int z) { return false; }

		#endregion
	}