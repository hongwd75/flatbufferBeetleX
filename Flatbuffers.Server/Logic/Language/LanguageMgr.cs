using System.Collections;
using System.Reflection;
using System.Text;
using Game.Logic.datatable;
using Game.Logic.network;
using Game.Logic.Utils;
using Game.Logic.World;
using log4net;
using Logic.database.table;

namespace Game.Logic.Language;

public class LanguageMgr
{
    #region ==== 변수들 ================================================================================================
    private enum ArrayType : int
    {
        ID = 0,
        TEXT,
        LANGUAGE
    };
    private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    private const string TRANSLATION_ID_EMPTY = "Empty translation id.";
    private const string TRANSLATION_NULL = "NULL";
    
    private static LanguageMgr instance = new LanguageMgr();
    
    protected string LangPathImpl = "";
    private IDictionary<string, IDictionary<LanguageDataObject.eTranslationIdentifier, IList<LanguageDataObject>>> m_translations;
    #endregion
    
    #region ==== 속성 ==================================================================================================
    public static string DefaultLanguage => Game.Logic.ServerProperties.Properties.SERV_LANGUAGE;
    public static IEnumerable<string> Languages
    {
        get
        {
            foreach (string language in instance.m_translations.Keys)
            {
                yield return language;
            }
            yield break;
        }
    }
    
    private string LangPath { 
        get
        {
            if (LangPathImpl == "")
            {
                LangPathImpl = Path.Combine(GameServer.Instance.Configuration.RootDirectory, "languages");
            }
            return LangPathImpl;
        }
    }    
    #endregion
    
    #region ==== 초기화 ================================================================================================
    public static bool Init()
    {
        return instance.init();
    }

    private bool init()
    {
        m_translations = new Dictionary<string, IDictionary<LanguageDataObject.eTranslationIdentifier, IList<LanguageDataObject>>>();
        return LoadTranslations();
    }
    private bool LoadTranslations()
    {
        #region Load system translations
        if (log.IsDebugEnabled)
            log.Info("[Language-Manager] Loading system sentences...");

        ArrayList fileSentences = new ArrayList();
        bool defaultLanguageDirectoryFound = false;
        bool defaultLanguageFilesFound = false;
        foreach (string langDir in Directory.GetDirectories(LangPath, "*", SearchOption.TopDirectoryOnly))
        {
            string language = (langDir.Substring(langDir.LastIndexOf(Path.DirectorySeparatorChar) + 1)).ToUpper();
            if (language != DefaultLanguage)
            {
                if (language != "CU") // Ignore the custom language folder. This check should be removed in the future! (code written: may 2012)
                    fileSentences.AddRange(ReadLanguageDirectory(Path.Combine(LangPath, language), language));
            }
            else
            {
                defaultLanguageDirectoryFound = true;
                ArrayList sentences = ReadLanguageDirectory(Path.Combine(LangPath, language), language);

                if (sentences.Count < 1)
                    break;
                else
                {
                    fileSentences.AddRange(sentences);
                    defaultLanguageFilesFound = true;
                }
            }
        }

        if (!defaultLanguageDirectoryFound)
        {
            log.Error("Could not find default '" + DefaultLanguage + "' language directory, server can't start without it!");
            return false;
        }

        if (!defaultLanguageFilesFound)
        {
            log.Error("Default '" + DefaultLanguage + "' language files missing, server can't start without those files!");
            return false;
        }

        if (Game.Logic.ServerProperties.Properties.USE_DBLANGUAGE)
        {
            int newEntries = 0;
            int updatedEntries = 0;

            IList<DBLanguageSystem> dbos = GameServer.Database.SelectAllObjects<DBLanguageSystem>();

            if (Game.Logic.ServerProperties.Properties.UPDATE_EXISTING_DB_SYSTEM_SENTENCES_FROM_FILES)
            {
                foreach (string[] sentence in fileSentences)
                {
                    bool found = false;
                    foreach (DBLanguageSystem dbo in dbos)
                    {
                        if (dbo.TranslationId != sentence[(int)ArrayType.ID])
                            continue;

                        if (dbo.Language != sentence[(int)ArrayType.LANGUAGE])
                            continue;

                        if (dbo.Text != sentence[(int)ArrayType.TEXT])
                        {
                            dbo.Text = sentence[(int)ArrayType.TEXT];
                            GameServer.Database.SaveObject(dbo); // Please be sure to use the UTF-8 format for your language files, otherwise
                            // some database rows will be updated on each server start, because one char
                            // differs from the one within the database.
                            updatedEntries++;

                            if (log.IsWarnEnabled)
                                log.Warn("[Language-Manager] Language <" + sentence[(int)ArrayType.LANGUAGE] + "> TranslationId <" + dbo.TranslationId + "> updated in database!");
                        }

                        found = true;
                        break;
                    }

                    if (!found)
                    {
                        DBLanguageSystem dbo = new DBLanguageSystem();
                        dbo.TranslationId = sentence[(int)ArrayType.ID];
                        dbo.Text = sentence[(int)ArrayType.TEXT];
                        dbo.Language = sentence[(int)ArrayType.LANGUAGE];

                        GameServer.Database.AddObject(dbo);
                        RegisterLanguageDataObject(dbo);
                        newEntries++;

                        if (log.IsWarnEnabled)
                            log.Warn("[Language-Manager] Language <" + dbo.Language + "> TranslationId <" + dbo.TranslationId + "> added into the database.");
                    }
                }
            }
            else // Add missing translations.
            {
                foreach (string[] sentence in fileSentences)
                {
                    bool found = false;
                    foreach (DBLanguageSystem lngObj in dbos)
                    {
                        if (lngObj.TranslationId != sentence[(int)ArrayType.ID])
                            continue;

                        if (lngObj.Language != sentence[(int)ArrayType.LANGUAGE])
                            continue;

                        found = true;
                        break;
                    }

                    if (!found)
                    {
                        DBLanguageSystem dbo = new DBLanguageSystem();
                        dbo.TranslationId = sentence[(int)ArrayType.ID];
                        dbo.Text = sentence[(int)ArrayType.TEXT];
                        dbo.Language = sentence[(int)ArrayType.LANGUAGE];

                        GameServer.Database.AddObject(dbo);
                        RegisterLanguageDataObject(dbo);
                        newEntries++;

                        if (log.IsWarnEnabled)
                            log.Warn("[Language-Manager] Language <" + dbo.Language + "> TranslationId <" + dbo.TranslationId + "> added into the database.");
                    }
                }
            }

            foreach (DBLanguageSystem dbo in dbos)
                RegisterLanguageDataObject(dbo);

            if (newEntries > 0)
            {
                if (log.IsWarnEnabled)
                    log.Warn("[Language-Manager] Added <" + newEntries + "> new entries into the Database.");
            }

            if (updatedEntries > 0)
            {
                if (log.IsWarnEnabled)
                    log.Warn("[Language-Manager] Updated <" + updatedEntries + "> entries in Database.");
            }
        }
        else
        {
            foreach (string[] sentence in fileSentences)
            {
                DBLanguageSystem obj = new DBLanguageSystem();
                obj.TranslationId = sentence[(int)ArrayType.ID];
                obj.Text = sentence[(int)ArrayType.TEXT];
                obj.Language = sentence[(int)ArrayType.LANGUAGE];
                RegisterLanguageDataObject(obj);
            }
        }

        fileSentences = null;
        #endregion Load system translations

        #region Load object translations
        if (log.IsDebugEnabled)
            log.Info("[Language-Manager] Loading object translations...");

        List<LanguageDataObject> lngObjs = new List<LanguageDataObject>();
        
        lngObjs.AddRange((IList<LanguageDataObject>)GameServer.Database.SelectAllObjects<DBLanguageArea>());
        lngObjs.AddRange((IList<LanguageDataObject>)GameServer.Database.SelectAllObjects<DBLanguageGameObject>());
        lngObjs.AddRange((IList<LanguageDataObject>)GameServer.Database.SelectAllObjects<DBLanguageNPC>());
        lngObjs.AddRange((IList<LanguageDataObject>)GameServer.Database.SelectAllObjects<DBLanguageZone>());

        foreach (LanguageDataObject lngObj in lngObjs)
            RegisterLanguageDataObject(lngObj);

        lngObjs = null;
        #endregion Load object translations
        return true;
    }
    
    private ArrayList ReadLanguageDirectory(string path, string language)
    {
		ArrayList sentences = new ArrayList();
        foreach (string languageFile in Directory.GetFiles(path, "*", SearchOption.AllDirectories))
        {
            if (!languageFile.EndsWith(".txt"))
                continue;

            string[] lines = File.ReadAllLines(languageFile, Encoding.GetEncoding("utf-8"));
            IList textList = new ArrayList(lines);

            foreach (string line in textList)
            {
                // do not read comments
                if (line.StartsWith("#"))
                    continue;

                // ignore any line that is not formatted  'identifier: sentence'
                if (line.IndexOf(':') == -1)
                    continue;

                string[] translation = new string[3];

                // 0 is the identifier for the sentence
                translation[(int)ArrayType.ID] = line.Substring(0, line.IndexOf(':'));
                translation[(int)ArrayType.TEXT] = line.Substring(line.IndexOf(':') + 1);

                // 1 is the sentence with any tabs (used for readability in language file) removed
                translation[(int)ArrayType.TEXT] = translation[(int)ArrayType.TEXT].Replace("\t", " ");
                translation[(int)ArrayType.TEXT] = translation[(int)ArrayType.TEXT].Trim();

                // 2 is the language of the sentence
                translation[(int)ArrayType.LANGUAGE] = language;

                // Ignore duplicates
                bool ignore = false;
                foreach (string[] sentence in sentences)
                {
                    if (sentence[(int)ArrayType.ID] != translation[(int)ArrayType.ID])
                        continue;

                    if (sentence[(int)ArrayType.LANGUAGE] != translation[(int)ArrayType.LANGUAGE])
                        continue;

                    ignore = true;
                    break;
                }

                if (ignore)
                    continue;

                sentences.Add(translation);
            }
        }
        return sentences;
    }    
    public bool RegisterLanguageDataObject(LanguageDataObject obj)
    {
        if (obj != null)
        {
            lock (m_translations)
            {
                if (!m_translations.ContainsKey(obj.Language))
                {
                    IDictionary<LanguageDataObject.eTranslationIdentifier, IList<LanguageDataObject>> col = new Dictionary<LanguageDataObject.eTranslationIdentifier, IList<LanguageDataObject>>();
                    IList<LanguageDataObject> objs = new List<LanguageDataObject>();
                    objs.Add(obj);
                    col.Add(obj.TranslationIdentifier, objs);
                    m_translations.Add(obj.Language, col);
                    return true;
                }
                else if (m_translations[obj.Language] == null)
                {
                    IDictionary<LanguageDataObject.eTranslationIdentifier, IList<LanguageDataObject>> col = new Dictionary<LanguageDataObject.eTranslationIdentifier, IList<LanguageDataObject>>();
                    IList<LanguageDataObject> objs = new List<LanguageDataObject>();
                    objs.Add(obj);
                    col.Add(obj.TranslationIdentifier, objs);
                    m_translations[obj.Language] = col;
                    return true;
                }
                else if (!m_translations[obj.Language].ContainsKey(obj.TranslationIdentifier))
                {
                    IDictionary<LanguageDataObject.eTranslationIdentifier, IList<LanguageDataObject>> col = new Dictionary<LanguageDataObject.eTranslationIdentifier, IList<LanguageDataObject>>();
                    IList<LanguageDataObject> objs = new List<LanguageDataObject>();
                    objs.Add(obj);
                    m_translations[obj.Language].Add(obj.TranslationIdentifier, objs);
                    return true;
                }
                else if (m_translations[obj.Language][obj.TranslationIdentifier] == null)
                {
                    IList<LanguageDataObject> objs = new List<LanguageDataObject>();
                    objs.Add(obj);
                    m_translations[obj.Language][obj.TranslationIdentifier] = objs;
                }
                else if (!m_translations[obj.Language][obj.TranslationIdentifier].Contains(obj))
                {
                    lock (m_translations[obj.Language][obj.TranslationIdentifier])
                    {
                        if (!m_translations[obj.Language][obj.TranslationIdentifier].Contains(obj))
                        {
                            m_translations[obj.Language][obj.TranslationIdentifier].Add(obj);
                            return true;
                        }
                    }
                }
            }
        }
        return false; // Object is 'NULL' or already in list.
    }        
    #endregion
    
    #region ## Get language data object
    public LanguageDataObject GetLanguageDataObject(string language, string translationId, LanguageDataObject.eTranslationIdentifier translationIdentifier)
    {
        if (string.IsNullOrEmpty(language) || string.IsNullOrEmpty(translationId))
            return null;

        if (!m_translations.ContainsKey(language))
            return null;

        if (m_translations[language] == null)
        {
            lock (m_translations)
                m_translations.Remove(language);

            return null;
        }

        if (!m_translations[language].ContainsKey(translationIdentifier))
            return null;

        if (m_translations[language][translationIdentifier] == null)
        {
            lock (m_translations)
                m_translations[language].Remove(translationIdentifier);

            return null;
        }

        LanguageDataObject result = null;
        foreach (LanguageDataObject colObj in m_translations[language][translationIdentifier])
        {
            if (colObj.TranslationIdentifier != translationIdentifier)
                continue;

            if (colObj.TranslationId != translationId)
                continue;

            if (colObj.Language != language)
                continue;

            result = colObj;
            break;
        }

        return result;
    }
    #endregion

    #region ## Get translation
    public static LanguageDataObject GetTranslation(GameClient client, ITranslatableObject obj)
    {
        LanguageDataObject translation;
        instance.TryGetTranslation(out translation, client, obj);
        return translation;
    }
        
    public static LanguageDataObject GetTranslation(GamePlayer player, ITranslatableObject obj)
    {
        return GetTranslation(player.Network, obj);
    }

    public static LanguageDataObject GetTranslation(string language, ITranslatableObject obj)
    {
        LanguageDataObject translation; 
        instance.TryGetTranslation(out translation, language, obj);
        return translation;
    }

    public static string GetTranslation(GameClient client, string translationId, params object[] args)
    {
        string translation; 
        instance.TryGetTranslation(out translation, client, translationId, args);
        return translation;
    }

    public static string GetTranslation(string language, string translationId, params object[] args)
    {
        string translation; 
        TryGetTranslation(out translation, language, translationId, args);
        return translation;
    }
    
    protected virtual bool TryGetTranslationImpl(out string translation, ref string language, string translationId, ref object[] args)
    {
        translation = "";

        if (string.IsNullOrEmpty(translationId))
        {
            translation = TRANSLATION_ID_EMPTY;
            return false;
        }

        if (string.IsNullOrEmpty(language) || !m_translations.ContainsKey(language))
        {
            language = DefaultLanguage;
        }

        LanguageDataObject result = GetLanguageDataObject(language, translationId, LanguageDataObject.eTranslationIdentifier.eSystem);
        if (result == null)
        {
            translation = GetTranslationErrorText(language, translationId);
            return false;
        }
        else
        {
            if (!string.IsNullOrEmpty(((DBLanguageSystem)result).Text))
            {
                translation = ((DBLanguageSystem)result).Text;
            }
            else
            {
                translation = GetTranslationErrorText(language, translationId);
                return false;
            }
        }

        if (args == null)
        {
            args = Array.Empty<object>();
        }

        try
        {
            if (args.Length > 0)
                translation = string.Format(translation, args);
        }
        catch
        {
            log.ErrorFormat("[Language-Manager] Parameter number incorrect: {0} for language {1}, Arg count = {2}, sentence = '{3}', args[0] = '{4}'", translationId, language, args.Length, translation, args.Length > 0 ? args[0] : "null");
        }
        return true;
    }
    public string GetTranslationErrorText(string lang, string TranslationID)
    {
        try
        {
            if (TranslationID.Contains(".") && TranslationID.TrimEnd().EndsWith(".") == false && TranslationID.StartsWith("'") == false)
            {
                return lang + " " + TranslationID.Substring(TranslationID.LastIndexOf(".") + 1);
            }
            else
            {
                // Odds are a literal string was passed with no translation, so just return the string unmodified
                return TranslationID;
            }
        }
        catch (Exception ex)
        {
            log.Error("Error Getting Translation Error Text for " + lang + ":" + TranslationID, ex);
        }

        return lang + " Translation Error!";
    }
    #endregion

    #region ## Try GetTranslation
    public bool TryGetTranslation(out LanguageDataObject translation, GameClient client, ITranslatableObject obj)
    {
        if (client == null)
        {
            translation = null;
            return false;
        }

        return TryGetTranslation(out translation, (client.Account == null ? String.Empty : client.Account.Language), obj);
    }
    public bool TryGetTranslation(out LanguageDataObject translation, string language, ITranslatableObject obj)
    {
        if (obj == null)
        {
            translation = null;
            return false;
        }

        if (string.IsNullOrEmpty(language) || language == DefaultLanguage )
        {
            translation = null;
            return false;
        }

        translation = GetLanguageDataObject(language, obj.TranslationId, obj.TranslationIdentifier);
        return (translation == null ? false : true);
    }
    public bool TryGetTranslation(out string translation, GameClient client, string translationId, params object[] args)
    {
        if (client == null)
        {
            translation = TRANSLATION_NULL;
            return true;
        }

        bool result = TryGetTranslation(out translation, (client.Account == null ? DefaultLanguage : client.Account.Language), translationId, args);

        if (client.Account != null)
        {
            if (client.Account.PrivLevel > 1 && client.Player != null && result)
            {
                if (client.ClientState == GameClient.eClientState.Playing)
                {
                    bool debug = client.Player.TempProperties.getProperty("LANGUAGEMGR-DEBUG", false);
                    if (debug)
                        translation = ("Id is " + translationId + " " + translation);
                }
            }
        }

        return result;
    }    
    public static bool TryGetTranslation(out string translation, string language, string translationId, params object[] args)
    {
        return instance.TryGetTranslationImpl(out translation, ref language, translationId, ref args);
    }
    public static string TryTranslateOrDefault(GamePlayer player, string missingDefault, string translationId, params object[] args)
    {
        string missing = missingDefault;
        	
        if (args.Length > 0)
        {
            try
            {
                missing = string.Format(missingDefault, args);
            }
            catch
            {
            }
        }
        	
        if (player == null || player.Network == null)
            return missing;
        	
        string retval;
        if (TryGetTranslation(out retval, player.Network.Account.Language, translationId, args))
        {
            return retval;
        }
        	
        return missing;
    }    

    #endregion
}