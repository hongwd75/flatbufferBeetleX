using System.Diagnostics;
using System.Reflection;
using Game.Logic.network;
using Game.Logic.attribute;
using Game.Logic.Events;
using log4net;

namespace Game.Logic.World.Timer
{
public static class RegionTimersResynch
	{
		const int UPDATE_INTERVAL = 15 * 1000; // 15 seconds to check freeze

		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		static System.Threading.Timer m_timer;
		public static Stopwatch watch { get; set; }
		static Dictionary<GameTimer.TimeManager, long> old_time = new Dictionary<GameTimer.TimeManager, long>();

		#region Initialization/Teardown

		[ScriptLoadedEvent]
		public static void OnScriptCompiled(GameEvent e, object sender, EventArgs args)
		{
			if (ServerProperties.Properties.USE_SYNC_UTILITY)
				Init();
		}
		[ScriptUnloadedEvent]
		public static void OnScriptUnloaded(GameEvent e, object sender, EventArgs args) 
		{
			if (ServerProperties.Properties.USE_SYNC_UTILITY)
				Stop();
		}

		public static void Init()
		{
			watch = new Stopwatch();
			watch.Start();
			foreach (GameTimer.TimeManager mgr in WorldManager.GetRegionTimeManagers())
				old_time.Add(mgr, 0);

			m_timer = new System.Threading.Timer(new TimerCallback(Resynch), null, 0, UPDATE_INTERVAL);
		}

		public static void Stop()
		{
			if (m_timer != null)
				m_timer.Dispose();
		}

		#endregion

		private static void Resynch(object nullValue)
		{
			long syncTime = watch.ElapsedMilliseconds;

			/*
			//Check alive
			foreach (GameTimer.TimeManager mgr in WorldManager.GetRegionTimeManagers())
			{
				if (old_time.ContainsKey(mgr) && old_time[mgr] > 0 && old_time[mgr] == mgr.CurrentTime)
				{
					if (log.IsErrorEnabled)
					{
						// Tolakram: Can't do StackTrace call here.  If thread is stopping will result in UAE app stop
						log.ErrorFormat("----- Found Frozen Region Timer -----\nName: {0} - Current Time: {1}", mgr.Name, mgr.CurrentTime);
					}

					//if(mgr.Running)
					try
					{
						if (!mgr.Stop())
						{
							log.ErrorFormat("----- Failed to Stop the TimeManager: {0}", mgr.Name);
						}
					}
					catch(Exception mex)
					{
						log.ErrorFormat("----- Errors while trying to stop the TimeManager: {0}\n{1}", mgr.Name, mex);
					}

					foreach (GameClient clients in WorldManager.GetAllClients())
					{
						if (clients.Player == null || clients.ClientState == GameClient.eClientState.Linkdead)
						{
							if(log.IsErrorEnabled)
								log.ErrorFormat("----- Disconnected Client: {0}", clients.Account.Name);
							if (clients.Player != null)
							{
								clients.Player.SaveIntoDatabase();
								clients.Player.Quit(true);
							}
							clients.Out.SendPlayerQuit(true);
							clients.Disconnect();
							GameServer.Instance.Disconnect(clients);
							WorldManager.RemoveClient(clients);
						}
					}

					if (!mgr.Start())
					{
						log.ErrorFormat("----- Failed to (re)Start the TimeManager: {0}", mgr.Name);
					}
					
                    foreach (Region reg in WorldManager.GetAllRegions())
					{
						if (reg.TimeManager == mgr)
						{
							foreach (GameObject obj in reg.Objects)
							{
								//Restart Player regen & remove PvP immunity
								if (obj is GamePlayer)
								{
									GamePlayer plr = obj as GamePlayer;
									if (plr.IsAlive)
									{
										plr.StopHealthRegeneration();
										plr.StopPowerRegeneration();
										plr.StopEnduranceRegeneration();
										plr.StopCurrentSpellcast();
										plr.StartHealthRegeneration();
										plr.StartPowerRegeneration();
										plr.StartEnduranceRegeneration();
										plr.StartInvulnerabilityTimer(1000, null);

                                        
										try
										{
											foreach (IGameEffect effect in plr.EffectList)
											{
												var gsp = effect as GameSpellEffect;
												if (gsp != null)
													gsp.RestartTimers();
											}
										}
										catch(Exception e)
										{
											log.Error("Can't cancel immunty effect : "+e);
										}
									}
								}
								
								//Restart Brains & Paths
								if (obj is GameNPC && (obj as GameNPC).Brain != null)
                                {
									GameNPC npc = obj as GameNPC;
									
									if(npc.Brain is IControlledBrain)
									{
										npc.Die(null);
									}
									else if(!(npc.Brain is BlankBrain))
									{
                                        npc.Brain.Stop();
										DOL.AI.ABrain brain = npc.Brain;
                                        npc.RemoveBrain(npc.Brain);
                                        //npc.Brain.Stop();
										if (npc.MaxSpeedBase > 0 && npc.PathID != null && npc.PathID != "" && npc.PathID != "NULL")
										{
											npc.StopMovingOnPath();
											PathPoint path = MovementMgr.LoadPath(npc.PathID);
											if (path != null)
											{
												npc.CurrentWayPoint = path;
												npc.MoveOnPath((short)path.MaxSpeed);
											}
										}
                                        try
										{
											npc.SetOwnBrain(brain);
											npc.Brain.Start();
										}
										catch(Exception e)
										{
											log.Error("Can't restart Brain in RegionTimerResynch, NPC Name = "+npc.Name
                                                +" X="+npc.Position.X+"/Y="+npc.Position.Y+"/Z="+npc.Position.Z+"/R="+npc.Position.RegionID+" "+e);
											try
											{
												npc.Die(null);
											}
											catch(Exception ee)
											{
												log.Error("Can't restart Brain and Kill NPC in RegionTimerResynch, NPC Name = "+npc.Name
                                                    +" X="+npc.Position.X+"/Y="+npc.Position.Y+"/Z="+npc.Position.Z+"/R="+npc.Position.RegionID+" "+ee);
											}
										}
									}
								}
							}
							
							//Restart Respawn Timers
							List<GameNPC> respawnings = new List<GameNPC>(reg.MobsRespawning.Keys);
							foreach(GameNPC deadMob in respawnings)
							{
								GameNPC mob = deadMob;
								if(mob != null)
									mob.StartRespawn();
							}
						}					
					}
				}

				if (old_time.ContainsKey(mgr))
					old_time[mgr] = mgr.CurrentTime;
				else
					old_time.Add(mgr, mgr.CurrentTime);
			}
			*/
		}

		public delegate void RegionTimerHandler(GameTimer.TimeManager RestartedTimer, long SyncTime);
	}    
}
