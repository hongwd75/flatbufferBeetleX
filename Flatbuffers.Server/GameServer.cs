using System.Reflection;
using BeetleX;
using BeetleX.EventArgs;
using Flatbuffers.Messages.Packets.Server;
using Game.Logic.managers;
using Game.Logic.network;
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
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        #region ========= DB ===========================================================================================

        protected IObjectDatabase m_database;
        public IObjectDatabase IDatabase => m_database;
        public static IObjectDatabase Database => Instance.IDatabase;

        #endregion

        #region ========= STATUS =======================================================================================
        protected eGameServerStatus mStatus = eGameServerStatus.GSS_Unknown;
        public eGameServerStatus ServerStatus => mStatus;
        #endregion


        protected GameClientManager mClientManager;
        public static GameServer Instance { get; private set; } = null;
        public BaseServerConfiguration Configuration;

        #region ========= NETWORK ======================================================================================        
        private IServer mServerSocket;
        public IServer ServerSocket { get => mServerSocket; }
        public ServerNetworkHandler NetworkHandler { get =>(ServerNetworkHandler)mServerSocket.Handler; }
        #endregion
        
        public GameServer() : this(new BaseServerConfiguration())
        {
        }

        protected GameServer(BaseServerConfiguration config)
        {
            Configuration = config;

            Directory.SetCurrentDirectory(Configuration.RootDirectory);

            // DB 초기화
            try
            {
                CheckAndInitDB();
            }
            catch (Exception e)
            {
                throw;
            }
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
        public void Start()
        {
            mStatus = eGameServerStatus.GSS_Closed;

            // 클라이언트 메니저 설정
            mClientManager = new GameClientManager(Configuration.MaxClientCount);

            // GC 설정
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);

            // 소켓 설정
            ServerOptions options = new ServerOptions();
            options.LogLevel = LogType.Info;
            options.DefaultListen.Host = Configuration.IP.ToString();
            options.DefaultListen.Port = Configuration.Port;

            // 서버 소켓 시작
            mServerSocket = SocketFactory.CreateTcpServer<ServerNetworkHandler, ServerPacket>(options);
            GameClient.SendPacketClassMethods.Register();
            mServerSocket.Open();

            mStatus = eGameServerStatus.GSS_Open;
        }

        public void Stop()
        {
            mServerSocket.Dispose();
        }


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
                                if (log.IsInfoEnabled)
                                {
                                    log.InfoFormat("Registering table: {0}", type.FullName);
                                }

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
    }
}