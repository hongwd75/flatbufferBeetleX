using System.Globalization;
using System.Reflection;
using Logic.database.table;

namespace Game.Logic.ServerProperties
{
	public class Properties
	{
		[ServerProperty("system", "use_sync_timer", "sync timers utility 사용 설정", true)]
		public static bool USE_SYNC_UTILITY;		
		[ServerProperty("system", "staff_login", "직원 로그인 전용 - 직원만 로그인을 허용할지 여부를 설정합니다. 값: True, False", false)]
		public static bool STAFF_LOGIN;
		[ServerProperty("system", "max_players", "최대 플레이어 수 - 동시에 접속할 수 있는 최대 플레이어 수를 설정합니다. 무제한으로 설정하려면 0으로 설정하세요", 0)]
		public static int MAX_PLAYERS;
		[ServerProperty("system", "disabled_expansions", "비활성화된 확장 ID의 직렬화된 목록, 확장 ID는 클라이언트 유형으로 ;로 구분됨", "")]
		public static string DISABLED_EXPANSIONS = "";
		[ServerProperty("system", "server_language", "기본 언어 설정 (EN, KR)", "EN")]
		public static string SERV_LANGUAGE;	
		
		[ServerProperty("server", "use_dblanguage", "언어 데이터를 DB테이블에서 로딩 (테이블이 비어 있으면 파일에서 읽어서 작성)", false)]
		public static bool USE_DBLANGUAGE;
		[ServerProperty("server", "update_existing_db_system_sentences_from_files", "DBLanguageSystem 테이블에 있는 내용과 파일이 틀리면 업데이트", false)]
		public static bool UPDATE_EXISTING_DB_SYSTEM_SENTENCES_FROM_FILES;		
		[ServerProperty("server", "region_max_objects", "리전에서 허용되는 객체의 최대 개수를 설정합니다. 숫자가 작을수록 성능이 더 좋아집니다. 이 값은 서버가 실행 중일 때 변경할 수 없습니다. (256 - 65535)", (ushort)30000)]
		public static ushort REGION_MAX_OBJECTS;		
		
		[ServerProperty("account", "allow_auto_account_creation", "자동 계정 생성 허용 - 이 설정은 serverconfig.xml에서도 설정되며, 이 속성이 작동하려면 활성화되어 있어야 합니다.", true)]
		public static bool ALLOW_AUTO_ACCOUNT_CREATION;
		[ServerProperty("account", "time_between_account_creation_sameip", "동일 IP에서 계정을 생성한 후, 2개의 계정 생성 이후 계정 생성 간에 필요한 시간(분 단위)을 설정합니다.", 15)]
		public static int TIME_BETWEEN_ACCOUNT_CREATION_SAMEIP;
		
		[ServerProperty("world", "disabled_regions", "비활성화된 지역 ID의 직렬화된 목록, 세미콜론으로 구분되거나 대시로 범위 표시 (예: 1-5;7;9)", "")]
		public static string DISABLED_REGIONS = "";
		[ServerProperty("world", "world_day_increment", "일 증가 (0에서 512까지, 기본값은 24). 증가값이 클수록 하루의 길이가 짧아집니다.", (uint)24)]
		public static uint WORLD_DAY_INCREMENT;
		[ServerProperty("world", "world_npc_update_interval", "NPC가 클라이언트에 업데이트를 방송하는 주기(밀리초). 최소 허용값 = 1000(1초). 0으로 설정하면 이 업데이트가 비활성화됩니다", (uint)8000)]
		public static uint WORLD_NPC_UPDATE_INTERVAL;
		[ServerProperty("world", "world_object_update_interval", "객체(정적 객체, 주택, 문 등)가 클라이언트에 업데이트를 방송하는 주기(밀리초). 최소 허용값 = 10000(10초). 0으로 설정하면 이 업데이트가 비활성화됩니다.", (uint)30000)]
		public static uint WORLD_OBJECT_UPDATE_INTERVAL;
		[ServerProperty("world", "world_playertoplayer_update_interval", "다른 플레이어의 패킷이 클라이언트에 다시 방송되는 주기(밀리초). 최소 허용값 = 1000(1초). 0으로 설정하면 이 업데이트가 비활성화됩니다.", (uint)1000)]
		public static uint WORLD_PLAYERTOPLAYER_UPDATE_INTERVAL;
		[ServerProperty("world", "world_player_update_interval", "플레이어의 업데이트를 확인하는 주기(밀리초). 최소 허용값 = 100(100밀리초).", (uint)300)]
		public static uint WORLD_PLAYER_UPDATE_INTERVAL;
		[ServerProperty("world", "weather_check_interval", "날씨에서 폭풍이 시작될 가능성을 확인하는 주기(밀리초).", 5 * 60 * 1000)]
		public static int WEATHER_CHECK_INTERVAL;
		[ServerProperty("world", "weather_chance", "폭풍이 시작될 확률", 5)]
		public static int WEATHER_CHANCE;
		
		[ServerProperty("pvp", "Timer_Killed_By_Mob", "Immunity Timer When player killed in PvP, in seconds", 30)] //30 seconds default
		public static int TIMER_KILLED_BY_MOB;
		[ServerProperty("pvp", "Timer_Killed_By_Player", "Immunity Timer When player killed in PvP, in seconds", 120)] //2 min default
		public static int TIMER_KILLED_BY_PLAYER;
		[ServerProperty("pvp", "Timer_Region_Changed", "Immunity Timer when player changes regions, in seconds", 30)] //30 seconds default
		public static int TIMER_REGION_CHANGED;
		[ServerProperty("pvp", "Timer_Game_Entered", "Immunity Timer when player enters the game, in seconds", 10)] //10 seconds default
		public static int TIMER_GAME_ENTERED;
		[ServerProperty("pvp", "Timer_PvP_Teleport", "Immunity Timer when player teleports within the same region, in seconds", 30)] //30 seconds default
		public static int TIMER_PVP_TELEPORT;
		
		//----------------------------------------------------------------------------------------------
		#region #### 설정 함수 ####
		public static void InitProperties()
		{
			var propDict = AllDomainProperties;
			foreach (var prop in propDict)
			{
				Load(prop.Value.Item1, prop.Value.Item2, prop.Value.Item3);
			}

			// Refresh static dict values for display
			AllCurrentProperties = propDict.ToDictionary(k => k.Key, v => v.Value.Item2.GetValue(null));
		}

		public static IDictionary<string, object> AllCurrentProperties { get; private set; }
		public static IDictionary<string, Tuple<ServerPropertyAttribute, FieldInfo, ServerProperty>> AllDomainProperties
		{
			get
			{
				var result = new Dictionary<string, Tuple<ServerPropertyAttribute, FieldInfo, ServerProperty>>();
				var allProperties = GameServer.Database.SelectAllObjects<ServerProperty>();

				foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
				{
					foreach (Type type in asm.GetTypes())
					{
						foreach (FieldInfo field in type.GetFields())
						{
							// Properties are Static
							if (!field.IsStatic)
								continue;

							// Properties shoud contain a property attribute
							object[] attribs = field.GetCustomAttributes(typeof(ServerPropertyAttribute), false);
							if (attribs.Length == 0)
								continue;

							ServerPropertyAttribute att = (ServerPropertyAttribute)attribs[0];

							ServerProperty serverProp = allProperties
								.Where(p => p.Key.Equals(att.Key, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

							if (serverProp == null)
							{
								// Init DB Object
								serverProp = new ServerProperty();
								serverProp.Category = att.Category;
								serverProp.Key = att.Key;
								serverProp.Description = att.Description;
								if (att.DefaultValue is double)
								{
									CultureInfo myCIintl = new CultureInfo("en-US", false);
									IFormatProvider provider = myCIintl.NumberFormat;
									serverProp.DefaultValue = ((double)att.DefaultValue).ToString(provider);
								}
								else
								{
									serverProp.DefaultValue = att.DefaultValue.ToString();
								}

								serverProp.Value = serverProp.DefaultValue;
							}

							result[att.Key] =
								new Tuple<ServerPropertyAttribute, FieldInfo, ServerProperty>(att, field, serverProp);
						}
					}
				}

				return result;
			}
		}
		public static void Load(ServerPropertyAttribute attrib, FieldInfo field, ServerProperty prop)
		{
			string key = attrib.Key;
			
			// Not Added to database...
			if (!prop.IsPersisted)
			{
				GameServer.Database.AddObject(prop);
			}
		
			try
			{
				CultureInfo myCIintl = new CultureInfo("en-US", false);
				IFormatProvider provider = myCIintl.NumberFormat;
				field.SetValue(null, Convert.ChangeType(prop.Value, attrib.DefaultValue.GetType(), provider));
			}
			catch (Exception e)
			{
			}
		}
		#endregion
	}
}