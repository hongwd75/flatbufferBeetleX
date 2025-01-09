using System.Collections;
using System.Reflection;
using System.Text;
using Game.Logic.AI.Brain;
using Game.Logic.network;
using Game.Logic.ServerRules;
using Game.Logic.Skills;
using Game.Logic.Spells;
using Game.Logic.World;
using log4net;
using Server.Config;

namespace Game.Logic.Utils
{
public class ScriptMgr
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private static Dictionary<string, Assembly> m_compiledScripts = new Dictionary<string, Assembly>();
		private static Dictionary<string, ConstructorInfo> m_spellhandlerConstructorCache = new Dictionary<string, ConstructorInfo>();
		
		
		/// <summary>
		/// Get an array of all script assemblies
		/// </summary>
		public static Assembly[] Scripts
		{
			get
			{
				return m_compiledScripts.Values.ToArray();
			}
		}

		/// <summary>
		/// Get an array of GameServer Assembly with all scripts assemblies
		/// </summary>
		public static Assembly[] GameServerScripts
		{
			get
			{
				return m_compiledScripts.Values.Concat( new[] { typeof(GameServer).Assembly } ).ToArray();
			}
		}
		
		/// <summary>
		/// Get all loaded assemblies with Scripts Last
		/// </summary>
		public static Assembly[] AllAssembliesScriptsLast
		{
			get
			{
				return AppDomain.CurrentDomain.GetAssemblies().Where(asm => !Scripts.Contains(asm)).Concat(Scripts).ToArray();
			}
		}		
		
		/// <summary>
		/// Parses a directory for all source files
		/// </summary>
		/// <param name="path">The root directory to start the search in</param>
		/// <param name="filter">A filter representing the types of files to search for</param>
		/// <param name="deep">True if subdirectories should be included</param>
		/// <returns>An ArrayList containing FileInfo's for all files in the path</returns>
		private static IList<FileInfo> ParseDirectory(DirectoryInfo path, string filter, bool deep)
		{
			if (!path.Exists)
		    return new List<FileInfo>();
		
		   	return path.GetFiles(filter, SearchOption.TopDirectoryOnly).Union(deep ? path.GetDirectories().Where(di => !di.Name.Equals("obj", StringComparison.OrdinalIgnoreCase)).SelectMany(di => di.GetFiles(filter, SearchOption.AllDirectories)) : Array.Empty<FileInfo>() ).ToList();
		}
		

		/// <summary>
		/// Splits string to substrings
		/// </summary>
		/// <param name="cmdLine">string that should be split</param>
		/// <returns>Array of substrings</returns>
		private static string[] ParseCmdLine(string cmdLine)
		{
			if (cmdLine == null)
			{
				throw new ArgumentNullException("cmdLine");
			}

			List<string> args = new List<string>();
			int state = 0;
			StringBuilder arg = new StringBuilder(cmdLine.Length >> 1);

			for (int i = 0; i < cmdLine.Length; i++)
			{
				char c = cmdLine[i];
				switch (state)
				{
					case 0: // waiting for first arg char
						if (c == ' ') continue;
						arg.Length = 0;
						if (c == '"') state = 2;
						else
						{
							state = 1;
							i--;
						}
						break;
					case 1: // reading arg
						if (c == ' ')
						{
							args.Add(arg.ToString());
							state = 0;
						}
						arg.Append(c);
						break;
					case 2: // reading string
						if (c == '"')
						{
							args.Add(arg.ToString());
							state = 0;
						}
						arg.Append(c);
						break;
				}
			}
			if (state != 0) args.Add(arg.ToString());

			string[] pars = new string[args.Count];
			args.CopyTo(pars);

			return pars;
		}
		
		/// <summary>
		/// Load an Assembly from DLL path.
		/// </summary>
		/// <param name="dllName">path to Assembly DLL File</param>
		/// <returns>True if assembly is loaded</returns>
		public static bool LoadAssembly(string dllName)
		{
			try
			{
				Assembly asm = Assembly.LoadFrom(dllName);
				ScriptMgr.AddOrReplaceAssembly(asm);

				if (log.IsInfoEnabled)
					log.InfoFormat("Assembly {0} loaded successfully from path {1}", asm.FullName, dllName);
				
				return true;
			}
			catch (Exception e)
			{
				if (log.IsErrorEnabled)
					log.ErrorFormat("Error loading Assembly from path {0} - {1}", dllName, e);
			}
			
			return false;
		}

		/// <summary>
		/// Add or replace an assembly in the collection of compiled assemblies
		/// </summary>
		/// <param name="assembly"></param>
		public static void AddOrReplaceAssembly(Assembly assembly)
		{
			if (m_compiledScripts.ContainsKey(assembly.FullName))
			{
				m_compiledScripts[assembly.FullName] = assembly;
				if (log.IsDebugEnabled)
					log.Debug("Replaced assembly " + assembly.FullName);
			}
			else
			{
				m_compiledScripts.Add(assembly.FullName, assembly);
			}
		}

		/// <summary>
		/// Removes an assembly from the game servers list of usable assemblies
		/// </summary>
		/// <param name="fullName"></param>
		public static bool RemoveAssembly(string fullName)
		{
			if (m_compiledScripts.ContainsKey(fullName))
			{
				m_compiledScripts.Remove(fullName);
				return true;
			}

			return false;
		}

		/// <summary>
		/// searches the given assembly for AbilityActionHandlers
		/// </summary>
		/// <param name="asm">The assembly to search through</param>
		/// <returns>Hashmap consisting of keyName => AbilityActionHandler Type</returns>
		public static IList<KeyValuePair<string, Type>> FindAllAbilityActionHandler(Assembly asm)
		{
			List<KeyValuePair<string, Type>> abHandler = new List<KeyValuePair<string, Type>>();
			if (asm != null)
			{
				foreach (Type type in asm.GetTypes())
				{
					if (!type.IsClass)
						continue;
					if (type.GetInterface("DOL.GS.IAbilityActionHandler") == null)
						continue;
					if (type.IsAbstract)
						continue;

					object[] objs = type.GetCustomAttributes(typeof(SkillHandlerAttribute), false);
					for (int i = 0; i < objs.Length; i++)
					{
						if (objs[i] is SkillHandlerAttribute)
						{
							SkillHandlerAttribute attr = objs[i] as SkillHandlerAttribute;
							abHandler.Add(new KeyValuePair<string, Type>(attr.KeyName, type));
							//DOLConsole.LogLine("Found ability action handler "+attr.KeyName+": "+type);
							//									break;
						}
					}
				}
			}
			return abHandler;
		}

		/// <summary>
		/// searches the script directory for SpecActionHandlers
		/// </summary>
		/// <param name="asm">The assembly to search through</param>
		/// <returns>Hashmap consisting of keyName => SpecActionHandler Type</returns>
		public static IList<KeyValuePair<string, Type>> FindAllSpecActionHandler(Assembly asm)
		{
			List<KeyValuePair<string, Type>> specHandler = new List<KeyValuePair<string, Type>>();
			if (asm != null)
			{
				foreach (Type type in asm.GetTypes())
				{
					if (!type.IsClass)
						continue;
					if (type.GetInterface("Game.Logic.Skills.ISpecActionHandler") == null)
						continue;
					if (type.IsAbstract)
						continue;

					object[] objs = type.GetCustomAttributes(typeof(SkillHandlerAttribute), false);
					for (int i = 0; i < objs.Length; i++)
					{
						if (objs[i] is SkillHandlerAttribute)
						{
							SkillHandlerAttribute attr = objs[0] as SkillHandlerAttribute;
							specHandler.Add(new KeyValuePair<string, Type>(attr.KeyName, type));
							//DOLConsole.LogLine("Found spec action handler "+attr.KeyName+": "+type);
							break;
						}
					}
				}
			}
			return specHandler;
		}

		/// <summary>
		/// Searches for NPC guild scripts
		/// </summary>
		/// <param name="realm">Realm for searching handlers</param>
		/// <param name="asm">The assembly to search through</param>
		/// <returns>
		/// all handlers that were found, guildname(string) => classtype(Type)
		/// </returns>
		protected static Hashtable FindAllNPCGuildScriptClasses(eRealm realm, Assembly asm)
		{
			Hashtable ht = new Hashtable();
			if (asm != null)
			{
				foreach (Type type in asm.GetTypes())
				{
					// Pick up a class
					if (type.IsClass != true) continue;
					if (!type.IsSubclassOf(typeof(GameNPC))) continue;

					try
					{
						object[] objs = type.GetCustomAttributes(typeof(NPCGuildScriptAttribute), false);
						if (objs.Length == 0) continue;

						foreach (NPCGuildScriptAttribute attrib in objs)
						{
							if (attrib.Realm == eRealm.None || attrib.Realm == realm)
							{
								ht[attrib.GuildName] = type;
							}

						}
					}
					catch (Exception e)
					{
						if (log.IsErrorEnabled)
							log.Error("FindAllNPCGuildScriptClasses", e);
					}
				}
			}
			return ht;
		}

		protected static Hashtable[] m_gs_guilds = new Hashtable[(int)eRealm._Last + 1];
		protected static Hashtable[] m_script_guilds = new Hashtable[(int)eRealm._Last + 1];

		/// <summary>
		/// searches for a npc guild script
		/// </summary>
		/// <param name="guild"></param>
		/// <param name="realm"></param>
		/// <returns>type of class for searched npc guild or null</returns>
		public static Type FindNPCGuildScriptClass(string guild, eRealm realm)
		{
			if (string.IsNullOrEmpty(guild)) return null;

			Type type = null;
			if (m_script_guilds[(int)realm] == null)
			{
				Hashtable allScriptGuilds = new Hashtable();

				foreach (Assembly asm in GameServerScripts)
				{
					Hashtable scriptGuilds = FindAllNPCGuildScriptClasses(realm, asm);
					if (scriptGuilds == null) continue;
					foreach (DictionaryEntry entry in scriptGuilds)
					{
						if (allScriptGuilds.ContainsKey(entry.Key)) continue; // guild is already found
						allScriptGuilds.Add(entry.Key, entry.Value);
					}
				}
				m_script_guilds[(int)realm] = allScriptGuilds;
			}

			//SmallHorse: First test if no realm-guild hashmap is null, then test further
			//Also ... you can not use "nullobject as anytype" ... this crashes!
			//You have to test against NULL result before casting it... read msdn doku
			if (m_script_guilds[(int)realm] != null && m_script_guilds[(int)realm][guild] != null)
				type = m_script_guilds[(int)realm][guild] as Type;

			if (type == null)
			{
				if (m_gs_guilds[(int)realm] == null)
				{
					Assembly gasm = Assembly.GetAssembly(typeof(GameServer));
					m_gs_guilds[(int)realm] = FindAllNPCGuildScriptClasses(realm, gasm);
				}
			}

			//SmallHorse: First test if no realm-guild hashmap is null, then test further
			//Also ... you can not use "nullobject as anytype" ... this crashes!
			//You have to test against NULL result before casting it... read msdn doku
			if (m_gs_guilds[(int)realm] != null && m_gs_guilds[(int)realm][guild] != null)
				type = m_gs_guilds[(int)realm][guild] as Type;

			return type;
		}


		private static Type m_defaultControlledBrainType = typeof(ControlledNpcBrain);
		public static Type DefaultControlledBrainType
		{
			get { return m_defaultControlledBrainType; }
			set { m_defaultControlledBrainType = value; }
		}

		/// <summary>
		/// Constructs a new brain for player controlled npcs
		/// </summary>
		/// <param name="owner"></param>
		/// <returns></returns>
		public static IControlledBrain CreateControlledBrain(GamePlayer owner)
		{
			Type[] constructorParams = new Type[] { typeof(GamePlayer) };
			ConstructorInfo handlerConstructor = m_defaultControlledBrainType.GetConstructor(constructorParams);
			return (IControlledBrain)handlerConstructor.Invoke(new object[] { owner });
		}


		/// <summary>
		/// Create a spell handler for caster with given spell
		/// </summary>
		/// <param name="caster">caster that uses the spell</param>
		/// <param name="spell">the spell itself</param>
		/// <param name="line">the line that spell belongs to or null</param>
		/// <returns>spellhandler or null if not found</returns>
		public static ISpellHandler CreateSpellHandler(GameLiving caster, Spell spell, SpellLine line)
		{
			if (spell == null || spell.SpellType.Length == 0) return null;

			ConstructorInfo handlerConstructor = null;

			if (m_spellhandlerConstructorCache.ContainsKey(spell.SpellType))
				handlerConstructor = m_spellhandlerConstructorCache[spell.SpellType];

			// try to find it in assemblies when not in cache
			if (handlerConstructor == null)
			{
				Type[] constructorParams = new Type[] { typeof(GameLiving), typeof(Spell), typeof(SpellLine) };

				foreach (Assembly script in GameServerScripts)
				{
					foreach (Type type in script.GetTypes())
					{
						if (type.IsClass != true) continue;
						if (type.GetInterface("Game.Logic.Spells.ISpellHandler") == null) continue;

						// look for attribute
						try
						{
							object[] objs = type.GetCustomAttributes(typeof(SpellHandlerAttribute), false);
							if (objs.Length == 0) continue;

							foreach (SpellHandlerAttribute attrib in objs)
							{
								if (attrib.SpellType == spell.SpellType)
								{
									handlerConstructor = type.GetConstructor(constructorParams);
									if (log.IsDebugEnabled)
										log.Debug("Found spell handler " + type);
									break;
								}
							}
						}
						catch (Exception e)
						{
							if (log.IsErrorEnabled)
								log.Error("CreateSpellHandler", e);
						}

						if (handlerConstructor != null)
							break;
					}
				}

				if (handlerConstructor != null)
				{
					m_spellhandlerConstructorCache.Add(spell.SpellType, handlerConstructor);
				}
			}

			if (handlerConstructor != null)
			{
				try
				{
					return (ISpellHandler)handlerConstructor.Invoke(new object[] { caster, spell, line });
				}
				catch (Exception e)
				{
					if (log.IsErrorEnabled)
						log.Error("Failed to create spellhandler " + handlerConstructor, e);
				}
			}
			else
			{
				if (log.IsErrorEnabled)
					log.Error("Couldn't find spell handler for spell type " + spell.SpellType);
			}
			return null;
		}

		/// <summary>
		/// Clear all spell handlers from the cashe, forcing a reload when a spell is cast
		/// </summary>
		public static void ClearSpellHandlerCache()
		{
			m_spellhandlerConstructorCache.Clear();
		}

		/// <summary>
		/// Create server rules handler for specified server type
		/// </summary>
		/// <param name="serverType">server type used to look for rules handler</param>
		/// <returns>server rules handler or normal server type handler if errors</returns>
		public static IServerRules CreateServerRules(eGameServerType serverType)
		{
			Type rules = null;

			// first search in scripts
			foreach (Assembly script in Scripts)
			{
				foreach (Type type in script.GetTypes())
				{
					if (type.IsClass == false) continue;
					if (type.GetInterface("Game.Logic.ServerRules.IServerRules") == null) continue;

					// look for attribute
					try
					{
						object[] objs = type.GetCustomAttributes(typeof(ServerRulesAttribute), false);
						if (objs.Length == 0) continue;

						foreach (ServerRulesAttribute attrib in objs)
						{
							if (attrib.ServerType == serverType)
							{
								rules = type;
								break;
							}
						}
					}
					catch (Exception e)
					{
						if (log.IsErrorEnabled)
							log.Error("CreateServerRules", e);
					}
					if (rules != null) break;
				}
			}

			if (rules == null)
			{
				// second search in gameserver
				foreach (Type type in Assembly.GetAssembly(typeof(GameServer)).GetTypes())
				{
					if (type.IsClass == false) continue;
					if (type.GetInterface("Game.Logic.ServerRules.IServerRules") == null) continue;

					// look for attribute
					try
					{
						object[] objs = type.GetCustomAttributes(typeof(ServerRulesAttribute), false);
						if (objs.Length == 0) continue;

						foreach (ServerRulesAttribute attrib in objs)
						{
							if (attrib.ServerType == serverType)
							{
								rules = type;
								break;
							}
						}
					}
					catch (Exception e)
					{
						if (log.IsErrorEnabled)
							log.Error("CreateServerRules", e);
					}
					if (rules != null) break;
				}

			}

			if (rules != null)
			{
				try
				{
					IServerRules rls = (IServerRules)Activator.CreateInstance(rules, null);
					if (log.IsInfoEnabled)
						log.Info("Found server rules for " + serverType + " server type (" + rls.RulesDescription() + ").");
					return rls;
				}
				catch (Exception e)
				{
					if (log.IsErrorEnabled)
						log.Error("CreateServerRules", e);
				}
			}
			if (log.IsWarnEnabled)
				log.Warn("Rules for " + serverType + " server type not found, using \"normal\" server type rules.");
			return new NormalServerRules();
		}

		/// <summary>
		/// Search for a type by name; first in GameServer assembly then in scripts assemblies
		/// </summary>
		/// <param name="name">The type name</param>
		/// <returns>Found type or null</returns>
		public static Type GetType(string name)
		{
			Type t = typeof(GameServer).Assembly.GetType(name);
			if (t == null)
			{
				foreach (Assembly asm in Scripts)
				{
					t = asm.GetType(name);
					if (t == null) continue;
					return t;
				}
			}
			else
			{
				return t;
			}
			return null;
		}

		/// <summary>
		/// Finds all classes that derive from given type.
		/// First check scripts then GameServer assembly.
		/// </summary>
		/// <param name="baseType">The base class type.</param>
		/// <returns>Array of types or empty array</returns>
		public static Type[] GetDerivedClasses(Type baseType)
		{
			if (baseType == null)
				return Array.Empty<Type>();

			List<Type> types = new List<Type>();

			foreach (Assembly asm in GameServerScripts)
			{
				foreach (Type t in asm.GetTypes())
				{
					if (t.IsClass && !t.IsAbstract && baseType.IsAssignableFrom(t))
						types.Add(t);
				}
			}

			return types.ToArray();
		}
		
		/// <summary>
		/// Create new instance of ClassType, Looking through Assemblies and Scripts with given param
		/// </summary>
		/// <param name="classType"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		public static C CreateObjectFromClassType<C, T>(string classType, T args)
			where C : class
		{
			foreach (Assembly assembly in AllAssembliesScriptsLast)
			{
				try
				{
					C instance = assembly.CreateInstance(classType, false, BindingFlags.CreateInstance, null, new object[] { args }, null, null) as C;
					if (instance != null)
						return instance;
				}
				catch (Exception)
				{
				}

			}
			
			return null;
		}
		

	}    
}
