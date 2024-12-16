using System.Globalization;
using System.Reflection;
using Logic.database.table;

namespace Game.Logic.ServerProperties
{
	public class Properties
	{
		[ServerProperty("system", "staff_login", "직원 로그인 전용 - 직원만 로그인을 허용할지 여부를 설정합니다. 값: True, False", false)]
		public static bool STAFF_LOGIN;
		[ServerProperty("system", "max_players", "최대 플레이어 수 - 동시에 접속할 수 있는 최대 플레이어 수를 설정합니다. 무제한으로 설정하려면 0으로 설정하세요", 0)]
		public static int MAX_PLAYERS;		
		[ServerProperty("account", "allow_auto_account_creation", "자동 계정 생성 허용 - 이 설정은 serverconfig.xml에서도 설정되며, 이 속성이 작동하려면 활성화되어 있어야 합니다.", true)]
		public static bool ALLOW_AUTO_ACCOUNT_CREATION;
		[ServerProperty("account", "time_between_account_creation_sameip", "동일 IP에서 계정을 생성한 후, 2개의 계정 생성 이후 계정 생성 간에 필요한 시간(분 단위)을 설정합니다.", 15)]
		public static int TIME_BETWEEN_ACCOUNT_CREATION_SAMEIP;		
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