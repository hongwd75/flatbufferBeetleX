using System.Reflection;
using BeetleX;
using BeetleX.EventArgs;
using Flatbuffers.Messages.Packets.Server;
using Game.Logic.attribute;
using Game.Logic.Events;
using Game.Logic.managers;
using Game.Logic.network;
using Game.Logic.ServerProperties;
using Game.Logic.ServerRules;
using Game.Logic.Skills;
using Game.Logic.Utils;
using Game.Logic.World;
using log4net;
using log4net.Config;
using Logic.database;
using Logic.database.attribute;
using Server.Config;

namespace Game.Logic
{
    public class GameServer
    {
        protected const int MINUTE_CONV = 60000;
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        #region ========= DB ===========================================================================================
        protected IObjectDatabase m_database;
        public IObjectDatabase IDatabase => m_database;
        public static IObjectDatabase Database => Instance.IDatabase;
        #endregion

        #region ========= STATUS =======================================================================================
        protected eGameServerStatus mStatus = eGameServerStatus.GSS_Unknown;
        public eGameServerStatus ServerStatus { get => mStatus; }

        protected long mStartTick;
        protected Timer mTimer;
        public long TickCount { get => Environment.TickCount64 - mStartTick; }
        public int SaveInterval
        {
            get { return Configuration.SaveInterval; }
            set
            {
                Configuration.SaveInterval = value;
                if (mTimer != null)
                    mTimer.Change(value*MINUTE_CONV, Timeout.Infinite);
            }
        }        
        #endregion

        #region ========= NETWORK ======================================================================================        
        private IServer mServerSocket;
        public IServer ServerSocket { get => mServerSocket; }
        public ServerNetworkHandler NetworkHandler { get =>(ServerNetworkHandler)mServerSocket.Handler; }
        public static PacketMethodsManager SendPacketClassMethods = new PacketMethodsManager();
        #endregion

        protected GameClientManager mClientManager;
        public GameClientManager Clients => mClientManager;
        public static GameServer Instance { get; private set; } = null;
        public BaseServerConfiguration Configuration;
        
        public static IServerRules ServerRules => Instance.ServerRulesImpl;
        protected IServerRules m_serverRules;
        protected virtual IServerRules ServerRulesImpl
        {
            get
            {
                if (m_serverRules == null)
                {
                    m_serverRules = ScriptMgr.CreateServerRules(Configuration.ServerType);
                    if (m_serverRules != null)
                    {
                        m_serverRules.Initialize();
                    }
                    else
                    {
                        if (log.IsErrorEnabled)
                        {
                            log.Error("ServerRules null on access and failed to create.");
                        }
                    }
                }
                return m_serverRules;
            }
        }
        
        public GameServer() : this(new BaseServerConfiguration())
        {
        }

        protected GameServer(BaseServerConfiguration config)
        {
            Configuration = config;
            Directory.SetCurrentDirectory(Configuration.RootDirectory);
        }

        public static void CreateInstance(BaseServerConfiguration config)
        {
            //Only one intance
            if (Instance != null)
                return;

            //Try to find the log.config file, if it doesn't exist
            //we create it
            var logConfig = new FileInfo(config.LogConfigFile);
            if (!logConfig.Exists)
            {
                ResourceUtil.ExtractResource("logconfig.xml", logConfig.FullName);
            }

            //Configure and watch the config file
            XmlConfigurator.ConfigureAndWatch(logConfig);

            //Create the instance
            Instance = new GameServer(config);
        }

        //----------------------------------------------------------------------------------------------------
        // 서버 시작
        public bool Start()
        {
            // 서버 상태 초기화
            mStatus = eGameServerStatus.GSS_Closed;
            
            Thread.CurrentThread.Priority = ThreadPriority.Normal;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            
            // 시간 틱 설정
            mStartTick = Environment.TickCount64;
            
            // GC 설정
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            
            //--------------------------------------------------------------------------------------
            if ( InitComponent(CheckAndInitDB, "DB 초기화") == false )
            {
                return false;
            }

            //--------------------------------------------------------------------------------------
            if ( InitComponent(Properties.InitProperties, "서버 설정값 DB 확인") == false )
            {
                return false;
            }

            //--------------------------------------------------------------------------------------
            GameEventManager.Notify(ScriptEvent.Loaded);
            
            //--------------------------------------------------------------------------------------
            if (InitComponent(() =>
                {
                    if (mTimer != null)
                    {
                        mTimer.Change(Timeout.Infinite, Timeout.Infinite);
                        mTimer.Dispose();
                    }
                    mTimer = new Timer(SaveTimerProc, null, SaveInterval*MINUTE_CONV, Timeout.Infinite);                    
                }, "월드 타이머 등록") == false)
            {
                return false;
            }
            
            //--------------------------------------------------------------------------------------
            if ( InitComponent(() =>
                {
                    // 클라이언트 메니저 설정
                    mClientManager = new GameClientManager(Configuration.MaxClientCount);

                    // 소켓 설정
                    ServerOptions options = new ServerOptions();
                    options.LogLevel = LogType.Info;
                    options.DefaultListen.Host = Configuration.IP.ToString();
                    options.DefaultListen.Port = Configuration.Port;

                    // 서버 소켓 시작
                    mServerSocket = SocketFactory.CreateTcpServer<ServerNetworkHandler, ServerPacket>(options);
                    SendPacketClassMethods.Register();
                    mServerSocket.Open();        
                }, "네트워크 설정") == false )
            {
                return false;
            }
            return true;
        }

        public void Open()
        {
            mStatus = eGameServerStatus.GSS_Open;
        }
        
        public void Stop()
        {
            mStatus = eGameServerStatus.GSS_Closed;
            
            GameEventManager.Notify(ScriptEvent.Unloaded);
            if (mTimer != null)
            {
                mTimer.Change(Timeout.Infinite, Timeout.Infinite);
                mTimer.Dispose();
                mTimer = null;
            }
            
            mServerSocket.Dispose();
        }

        #region InitComponent ==========================================================================================
        protected bool InitComponent(bool componentInitState, string text)
        {
            if (log.IsDebugEnabled)
                log.DebugFormat("Start Memory {0}: {1}MB", text, GC.GetTotalMemory(false)/1024/1024);
			
            if (log.IsInfoEnabled)
                log.InfoFormat("{0}: {1}", text, componentInitState);
			
            if (!componentInitState)
                Stop();
			
            if (log.IsDebugEnabled)
                log.DebugFormat("Finish Memory {0}: {1}MB", text, GC.GetTotalMemory(false)/1024/1024);
			
            return componentInitState;
        }

        protected bool InitComponent(Action componentInitMethod, string text)
        {
            if (log.IsDebugEnabled)
                log.DebugFormat("Start Memory {0}: {1}MB", text, GC.GetTotalMemory(false)/1024/1024);
			
            bool componentInitState = false;
            try
            {
                componentInitMethod();
                componentInitState = true;
            }
            catch (Exception ex)
            {
                if (log.IsErrorEnabled)
                    log.ErrorFormat("{0}: Error While Initialization\n{1}", text, ex);
            }

            if (log.IsInfoEnabled)
                log.InfoFormat("{0}: {1}", text, componentInitState);

            if (!componentInitState)
                Stop();
			
            if (log.IsDebugEnabled)
                log.DebugFormat("Finish Memory {0}: {1}MB", text, GC.GetTotalMemory(false)/1024/1024);
			
            return componentInitState;
        }
        #endregion

        #region 크래시 로그 =============================================================================================
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            log.Fatal("Unhandled exception!\n" + e.ExceptionObject);
            if (e.IsTerminating)
                LogManager.Shutdown();
        }
        #endregion

        #region DB 초기화 ==============================================================================================
        protected virtual void CheckAndInitDB()
        {
            if (!InitDB() || m_database == null)
            {
                if (log.IsErrorEnabled)
                    log.Error("Could not initialize DB, please check path/connection string");
                throw new ApplicationException("DB initialization error");
            }
        }

        public bool InitDB()
        {
            if (m_database == null)
            {
                m_database = ObjectDatabase.GetObjectDatabase(Configuration.DBType, Configuration.DBConnectionString);

                try
                {
                    //We will search our assemblies for DataTables by reflection so
                    //it is not neccessary anymore to register new tables with the
                    //server, it is done automatically!
                    foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        // Walk through each type in the assembly
                        assembly.GetTypes().AsParallel().ForAll(type =>
                        {
                            if (!type.IsClass || type.IsAbstract)
                            {
                                return;
                            }

                            var attrib = type.GetCustomAttributes<DataTable>(false);
                            if (attrib.Any())
                            {
                                m_database.RegisterDataObject(type);
                            }
                        });
                    }
                }
                catch (DatabaseException e)
                {
                    if (log.IsErrorEnabled)
                        log.Error("Error registering Tables", e);
                    return false;
                }
            }

            if (log.IsInfoEnabled)
                log.Info("Database Initialization: true");
            return true;
        }

        #endregion

        #region 메니저 초기화 ===========================================================================================

        protected bool StartScriptComponents()
        {
            try
            {
                SkillBase.LoadSkills();
                if (log.IsInfoEnabled)
                {
                    log.Info("스킬 로딩 완료");
                }
                
                foreach (Assembly asm in ScriptMgr.GameServerScripts)
                {
                    GameEventManager.RegisterGlobalEvents(asm, typeof (GameServerStartedEventAttribute), GameServerEvent.Started);
                    GameEventManager.RegisterGlobalEvents(asm, typeof (GameServerStoppedEventAttribute), GameServerEvent.Stopped);
                    GameEventManager.RegisterGlobalEvents(asm, typeof (ScriptLoadedEventAttribute), ScriptEvent.Loaded);
                    GameEventManager.RegisterGlobalEvents(asm, typeof (ScriptUnloadedEventAttribute), ScriptEvent.Unloaded);
                }                
            }
            catch (Exception e)
            {
                if (log.IsErrorEnabled)
                    log.Error("StartScriptComponents 함수 내에서 오류 발생", e);                
                return false;
            }

            return true;
        }
        

        #endregion
        #region Save Timer Fuction
        protected void SaveTimerProc(object sender)
        {
            try
            {
                long startTick = Environment.TickCount64;
                if (log.IsInfoEnabled)
                    log.Info("Saving database...");
                if (log.IsDebugEnabled)
                    log.Debug("Save ThreadId=" + Thread.CurrentThread.ManagedThreadId);
                int saveCount = 0;
                if (m_database != null)
                {
                    ThreadPriority oldprio = Thread.CurrentThread.Priority;
                    Thread.CurrentThread.Priority = ThreadPriority.Lowest;

                    // //Only save the players, NOT any other object!
                    // saveCount = WorldManager.SavePlayers();
                    //
                    // //The following line goes through EACH region and EACH object
                    // //is tested for savability. A real waste of time, so it is commented out
                    // //WorldManager.SaveToDatabase();
                    //
                    // GuildMgr.SaveAllGuilds();
                    // BoatMgr.SaveAllBoats();
                    //
                    // FactionMgr.SaveAllAggroToFaction();

                    Thread.CurrentThread.Priority = oldprio;
                }
                if (log.IsInfoEnabled)
                    log.Info("Saving database complete!");
                startTick = Environment.TickCount64 - startTick;
                if (log.IsInfoEnabled)
                    log.Info("Saved all databases and " + saveCount + " players in " + startTick + "ms");
            }
            catch (Exception e1)
            {
                if (log.IsErrorEnabled)
                    log.Error("SaveTimerProc", e1);
            }
            finally
            {
                if (mTimer != null)
                    mTimer.Change(SaveInterval*MINUTE_CONV, Timeout.Infinite);
            }
        }
        

        #endregion
    }
}