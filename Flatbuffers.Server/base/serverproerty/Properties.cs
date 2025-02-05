using System.Globalization;
using System.Reflection;
using Logic.database.table;

namespace Game.Logic.ServerProperties
{
	public class Properties
	{
		[ServerProperty("system", "db_language", "DB 기본 언어 설정", "EN")]
		public static string DB_LANGUAGE;		
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
		[ServerProperty("system", "command_spam_delay", "기본 1500ms으로 같은 명령을 또 보내는 경우 무시한다.", 1500)]
		public static int COMMAND_SPAM_DELAY;		
		[ServerProperty("system", "hours_uptime_between_shutdown", "예약된 서버 종료 사이의 시간 (-1 = 예약된 재시작 없음)(ms)", -1)]
		public static int HOURS_UPTIME_BETWEEN_SHUTDOWN;
		
		[ServerProperty("server", "use_dblanguage", "언어 데이터를 DB테이블에서 로딩 (테이블이 비어 있으면 파일에서 읽어서 작성)", false)]
		public static bool USE_DBLANGUAGE;
		[ServerProperty("server", "update_existing_db_system_sentences_from_files", "DBLanguageSystem 테이블에 있는 내용과 파일이 틀리면 업데이트", false)]
		public static bool UPDATE_EXISTING_DB_SYSTEM_SENTENCES_FROM_FILES;		
		[ServerProperty("server", "region_max_objects", "리전에서 허용되는 객체의 최대 개수를 설정합니다. 숫자가 작을수록 성능이 더 좋아집니다. 이 값은 서버가 실행 중일 때 변경할 수 없습니다. (256 - 65535)", (ushort)30000)]
		public static ushort REGION_MAX_OBJECTS;
		[ServerProperty("server", "disable_quit_timer", "대기하지 않고 바로 logout", false)]
		public static bool DISABLE_QUIT_TIMER;		
		[ServerProperty("server", "autoselect_caster", "현재 대상이 유효하지 않을 경우, 유익한 주문이 시전자에게 자동으로 적용되도록 설정합니다. 이를 통해 대상을 변경하지 않고도 자기 치유가 가능합니다.", false)]
		public static bool AUTOSELECT_CASTER;	
		
		[ServerProperty("account", "allow_auto_account_creation", "자동 계정 생성 허용 - 이 설정은 serverconfig.xml에서도 설정되며, 이 속성이 작동하려면 활성화되어 있어야 합니다.", true)]
		public static bool ALLOW_AUTO_ACCOUNT_CREATION;
		[ServerProperty("account", "time_between_account_creation_sameip", "동일 IP에서 계정을 생성한 후, 2개의 계정 생성 이후 계정 생성 간에 필요한 시간(분 단위)을 설정합니다.", 15)]
		public static int TIME_BETWEEN_ACCOUNT_CREATION_SAMEIP;
		
		[ServerProperty("world", "check_los_during_cast", "Perform a LOS check during a spell cast.", true)]
		public static bool CHECK_LOS_DURING_CAST;
		[ServerProperty("world", "always_check_los", "Perform a LoS check before aggroing. This can involve a huge lag, handle with care!", false)]
		public static bool ALWAYS_CHECK_LOS;
		[ServerProperty("world", "always_check_pet_los", "Should we perform LOS checks between controlled NPC's and players?", false)]
		public static bool ALWAYS_CHECK_PET_LOS;		
		[ServerProperty("world", "disabled_regions", "비활성화된 지역 ID의 직렬화된 목록, 세미콜론으로 구분되거나 대시로 범위 표시 (예: 1-5;7;9)", "")]
		public static string DISABLED_REGIONS = "";
		[ServerProperty("world", "world_day_increment", "일 증가 (0에서 512까지, 기본값은 24). 증가값이 클수록 하루의 길이가 짧아집니다.", (uint)24)]
		public static uint WORLD_DAY_INCREMENT;
		[ServerProperty("world", "world_item_decay_time", "드랍된 아이템이 월드에 남아 있는 시간 (단위: ms)", (uint)180000)]
		public static uint WORLD_ITEM_DECAY_TIME;		
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
		
		[ServerProperty("pve", "baf_initial_chance", "Percent chance for a mob to bring a friend when attacked by a single attacker.  Each multiples of 100 guarantee an add, so a cumulative chance of 250% guarantees two adds with a 50% chance of a third.", 0)]
		public static int BAF_INITIAL_CHANCE;
		[ServerProperty("pve", "baf_additional_chance", "Percent chance for a mob to bring a friend for each additional attacker.  Each multiples of 100 guarantee an add, so a cumulative chance of 250% guarantees two adds with a 50% chance of a third.", 50)]
		public static int BAF_ADDITIONAL_CHANCE;		
		
		[ServerProperty("spells", "spell_interrupt_duration", "공격 받은 후 주문 방해 지속 시간 (밀리초 단위), 기본값: 4500", 4500)]
		public static int SPELL_INTERRUPT_DURATION;
		[ServerProperty("spells", "spell_interrupt_recast", "주문 방해 후 재시전 대기 시간 (밀리초 단위), 기본값: 2000", 2000)]
		public static int SPELL_INTERRUPT_RECAST;
		[ServerProperty("spells", "spell_interrupt_again", "주문 방해 재적용 딜레이 (밀리초 단위), 기본값: 100", 100)]
		public static int SPELL_INTERRUPT_AGAIN;
		[ServerProperty( "spells", "spell_interrupt_maxstagelength", "1단계 및 3단계의 최대 지속 시간 (밀리초 단위), 1000 = 1초, 기본값: 1500", 1500 )]
		public static int SPELL_INTERRUPT_MAXSTAGELENGTH;
		[ServerProperty( "spells", "spell_interrupt_max_intermediate_stagelength", "2단계의 최대 지속 시간 (밀리초 단위), 1000 = 1초. 999999를 설정하면 비활성화됨, 기본값: 3000", 3000 )]
		public static int SPELL_INTERRUPT_MAX_INTERMEDIATE_STAGELENGTH;
		[ServerProperty("spells", "spell_charm_named_check", "네임드 몬스터에 매혹 주문이 적용되지 않도록 설정, 0 = 비활성화, 1 = 활성화, 기본값: 1", 1)]
		public static int SPELL_CHARM_NAMED_CHECK;

		[ServerProperty("npc", "gamenpc_default_classtype", "game npc typeclass 설정.", "Game.Logic.GameNPC")]
		public static string GAMENPC_DEFAULT_CLASSTYPE;
		[ServerProperty("npc", "allow_roam", "Allow mobs to roam on the server", true)]
		public static bool ALLOW_ROAM;
		[ServerProperty("npc", "npc_heal_threshold", "NPCs, including pets, heal targets whose health falls below this percentage.", 75)]
		public static int NPC_HEAL_THRESHOLD;
		[ServerProperty("npc", "gamenpc_randomwalk_chance", "Chance for NPC to random walk. Default is 20", 20)]
		public static int GAMENPC_RANDOMWALK_CHANCE;
		[ServerProperty("npc", "mob_autoset_str_base", "Base Value to use when auto-setting STR stat. ", (short)30)]
		public static short MOB_AUTOSET_STR_BASE;
		[ServerProperty("npc", "mob_autoset_str_multiplier", "Multiplier to use when auto-setting STR stat. Multiplied by 10 when applied. ", 1.0)]
		public static double MOB_AUTOSET_STR_MULTIPLIER;
		[ServerProperty("npc", "mob_autoset_con_base", "Base Value to use when auto-setting CON stat. ", (short)30)]
		public static short MOB_AUTOSET_CON_BASE;
		[ServerProperty("npc", "mob_autoset_con_multiplier", "Multiplier to use when auto-setting CON stat. ", 1.0)]
		public static double MOB_AUTOSET_CON_MULTIPLIER;
		[ServerProperty("npc", "mob_autoset_qui_base", "Base Value to use when auto-setting qui stat. ", (short)30)]
		public static short MOB_AUTOSET_QUI_BASE;
		[ServerProperty("npc", "mob_autoset_qui_multiplier", "Multiplier to use when auto-setting QUI stat. ", 1.0)]
		public static double MOB_AUTOSET_QUI_MULTIPLIER;
		[ServerProperty("npc", "mob_autoset_dex_base", "Base Value to use when auto-setting DEX stat. ", (short)30)]
		public static short MOB_AUTOSET_DEX_BASE;
		[ServerProperty("npc", "mob_autoset_dex_multiplier", "Multiplier to use when auto-setting DEX stat. ", 1.0)]
		public static double MOB_AUTOSET_DEX_MULTIPLIER;
		[ServerProperty("npc", "mob_autoset_int_base", "Base Value to use when auto-setting INT stat. ", (short)30)]
		public static short MOB_AUTOSET_INT_BASE;
		[ServerProperty("npc", "mob_autoset_int_multiplier", "Multiplier to use when auto-setting INT stat. ", 1.0)]
		public static double MOB_AUTOSET_INT_MULTIPLIER;		
		[ServerProperty("npc", "gamenpc_followcheck_time", "How often, in milliseconds, to check follow distance. Lower numbers make NPC follow closer but increase load on server.", 500)]
		public static int GAMENPC_FOLLOWCHECK_TIME;		
		
		[ServerProperty("npc", "pet_scale_spell_max_level", "Disabled if 0 or less.  If greater than 0, this value is the level at which pets cast their spells at 100% effectivness, so choose spells for pets assuming they're at the level set here.  Live is max pet level, 44 or 50 depending on patch.", 0)]
		public static int PET_SCALE_SPELL_MAX_LEVEL;
		[ServerProperty("npc", "pet_levels_with_owner", "Do pets level up with their owner? ", false)]
		public static bool PET_LEVELS_WITH_OWNER;
		[ServerProperty("npc", "pet_autoset_str_base", "Base Value to use when auto-setting Pet STR stat. ", (short)30)]
		public static short PET_AUTOSET_STR_BASE;
		[ServerProperty("npc", "pet_autoset_str_multiplier", "Multiplier to use when auto-setting Pet STR stat. Multiplied by 10 when applied. ", 1.0)]
		public static double PET_AUTOSET_STR_MULTIPLIER;
		[ServerProperty("npc", "pet_autoset_con_base", "Base Value to use when auto-setting Pet CON stat. ", (short)30)]
		public static short PET_AUTOSET_CON_BASE;
		[ServerProperty("npc", "pet_autoset_con_multiplier", "Multiplier to use when auto-setting Pet CON stat. ", 1.0)]
		public static double PET_AUTOSET_CON_MULTIPLIER;
		[ServerProperty("npc", "pet_autoset_dex_base", "Base Value to use when auto-setting Pet DEX stat. ", (short)30)]
		public static short PET_AUTOSET_DEX_BASE;
		[ServerProperty("npc", "pet_autoset_dex_multiplier", "Multiplier to use when auto-setting Pet DEX stat. ", 1.0)]
		public static double PET_AUTOSET_DEX_MULTIPLIER;
		[ServerProperty("npc", "pet_autoset_qui_base", "Base Value to use when auto-setting Pet QUI stat. ", (short)30)]
		public static short PET_AUTOSET_QUI_BASE;
		[ServerProperty("npc", "pet_autoset_qui_multiplier", "Multiplier to use when auto-setting Pet QUI stat. ", 1.0)]
		public static double PET_AUTOSET_QUI_MULTIPLIER;
		[ServerProperty("npc", "pet_autoset_int_base", "Multiplier to use when auto-setting Pet INT stat. ", (short)30)]
		public static short PET_AUTOSET_INT_BASE;
		[ServerProperty("npc", "pet_autoset_int_multiplier", "Multiplier to use when auto-setting Pet INT stat. ", 1.0)]
		public static double PET_AUTOSET_INT_MULTIPLIER;
		[ServerProperty("npc", "pet_2h_bonus_damage", "If true, pets that use a 2H weapon and have a block chance get bonus damage equal to their block chance to compensate for not being able to block. ", true)]
		public static bool PET_2H_BONUS_DAMAGE;		
		
		[ServerProperty("rates", "item_sell_ratio", "상인한테 팔 때 아이템 가격할인 비율 %", 50)]
		public static int ITEM_SELL_RATIO;		
		[ServerProperty("rates", "cs_opening_effectiveness", "스텔스 스타일 고정 스타일 성장율", 1.0)]
		public static double CS_OPENING_EFFECTIVENESS;
		[ServerProperty("rates", "block_cap", "공격 블럭 확률 캡 (0.60 = 60%)", 0.60)]
		public static double BLOCK_CAP;
		[ServerProperty("rates", "evade_cap", "공격 회피 확률 캡 (0.50 = 50%)", 0.50)]
		public static double EVADE_CAP;
		[ServerProperty("rates", "parry_cap", "공격 무기 막기 확률 캡 (0.50 = 50%)", 0.50)]
		public static double PARRY_CAP;
		
		[ServerProperty("classes", "allow_cross_realm_items", "다른 랠름의 아이템 장착 허용", false)]
		public static bool ALLOW_CROSS_REALM_ITEMS;		
		
	
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