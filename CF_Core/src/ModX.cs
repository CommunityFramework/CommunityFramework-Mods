using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;
using System.Xml;
using System.Security;

public class ModX
{
    public static List<ModX> mods = new List<ModX>();

    public string settingsFilename = "Settings.xml";
    public string phrasesFilename = "Phrases.xml";

    private readonly string modName;
    private bool enabled;

    public string modConfigPath;
    public string modDatabasePath;

    private string settingsFilePath;
    private FileSystemWatcher settingsFileWatcher;

    private string phrasesFilePath;
    private FileSystemWatcher phrasesFileWatcher;

    Action OnModConfigLoaded;
    Action OnModPhrasesLoaded;

    public SortedDictionary<string, string> settings;
    public Dictionary<string, string> settingsDes;

    public SortedDictionary<string, string> phrases;
    public Dictionary<string, string> phrasesDes;

    public ModX(string Name, Action OnModConfigLoaded = null, Action OnModPhrasesLoaded = null)
    {
        this.modName = Name;

        this.modConfigPath = $"{Directory.GetCurrentDirectory()}/Mod_Configs/{modName}";
        this.modDatabasePath = $"{Directory.GetCurrentDirectory()}/Mod_Saves/{modName}";

        this.settings = new SortedDictionary<string, string>();
        this.settingsDes = new Dictionary<string, string>();

        this.phrases = new SortedDictionary<string, string>();
        this.phrasesDes = new Dictionary<string, string>();

        this.OnModConfigLoaded = OnModConfigLoaded;
        this.OnModPhrasesLoaded = OnModPhrasesLoaded;

        mods.Add(this);
    }
    public string UniqueName
    {
        get { return modName; }
    }
    public bool Enabled
    {
        get { return enabled; }
        set { enabled = value; }
    }
    public string Path
    {
        get { return modConfigPath; }
        set { modConfigPath = value; }
    }
    public string SettingsFilePath
    {
        get { return settingsFilePath; }
        set { settingsFilePath = value; }
    }
    public string PhrasesFilePath
    {
        get { return phrasesFilePath; }
        set { phrasesFilePath = value; }
    }
    public bool AddSetting(string key, string defValue, string regex, string description, out string value)
    {
        settingsDes[key] = description;

        if (settings.TryGetValue(key, out string valueRaw))
        {
            if (string.IsNullOrEmpty(regex) || Regex.Match(valueRaw, regex).Success)
            {
                value = valueRaw;
                return true;
            }

            Log.Error(string.Format("ModX.AddSetting reported: Can't regex pattern didn't match setting. Mod: {1} Setting: {2} Regex: {3}. Restoring default value: {4}", valueRaw, modName, key, regex, defValue));
            value = defValue;
            return false;
        }

        settings[key] = defValue;
        value = defValue;

        return true;
    }
    public bool AddSetting(string key, int defValue, int min, int max, string description, out int value)
    {
        settingsDes[key] = description;

        if (settings.TryGetValue(key, out string valueRaw))
        {
            if (int.TryParse(valueRaw, out int valueParsed))
            {
                if (min <= valueParsed && valueParsed <= max)
                {
                    value = valueParsed;
                    return true;
                }

                if (min > valueParsed)
                    Log.Error(string.Format("ModX.AddSetting reported: Error while checking values of {0}. Min: {1} Current: {2}. Using default: {3}", key, min, valueParsed, defValue));
                else Log.Error(string.Format("ModX.AddSetting reported: Error while checking values of {0}. Max: {1} Current: {2}. Using default: {3}", key, max, valueParsed, defValue));

                value = defValue;
                return false;
            }

            Log.Error(string.Format("ModX.AddSetting reported: Can't parse {0} to integer. Mod: {1} Setting: {2}. Restoring default value: {3}", valueRaw, modName, key, defValue));
            value = defValue;
            return false;
        }

        settings[key] = defValue.ToString();
        value = defValue;

        return true;
    }
    public bool AddSetting(string key, bool defValue, string description, out bool value)
    {
        settingsDes[key] = description;

        if (settings.TryGetValue(key, out string valueRaw))
        {
            if (bool.TryParse(valueRaw, out bool valueParsed))
            {
                value = valueParsed;
                return true;
            }

            Log.Error(string.Format("ModX.AddSetting reported: Can't parse {0} to bool. Mod: {1} Setting: {2}. Restoring default value: {3}", valueRaw, modName, key, defValue));
            value = defValue;
            return false;
        }

        settings[key] = defValue.ToString();
        value = defValue;

        return true;
    }
    public bool AddPhrase(string key, string defValue, string description, out string value)
    {
        phrasesDes[key] = description;

        if (phrases.TryGetValue(key, out string valueRaw))
        {
            value = valueRaw;
            return true;
        }

        phrases[key] = defValue;
        value = defValue;

        return true;
    }
    public bool Activate(bool _useDataBaseDirectory = false)
    {
        this.settingsFilePath = $"{modConfigPath}/{settingsFilename}";
        this.phrasesFilePath = $"{modConfigPath}/{phrasesFilename}";

        if (!string.IsNullOrEmpty(modName))
            Log.Out($"Load: {modName}");

        try
        {
            if (_useDataBaseDirectory)
                Directory.CreateDirectory(modDatabasePath);

            if (!File.Exists(settingsFilePath) && OnModConfigLoaded != null)
                OnModConfigLoaded();

            if (File.Exists(settingsFilePath))
                LoadSettingsFile();
            else WriteSettingsFile();

            if (File.Exists(settingsFilePath))
            {
                settingsFileWatcher = new FileSystemWatcher(modConfigPath, settingsFilename);
                settingsFileWatcher.Changed += new FileSystemEventHandler(OnSettingsFileChanged);
                settingsFileWatcher.Created += new FileSystemEventHandler(OnSettingsFileChanged);
                settingsFileWatcher.Deleted += new FileSystemEventHandler(OnSettingsFileChanged);
                settingsFileWatcher.EnableRaisingEvents = true;
            }

            if (!File.Exists(phrasesFilePath) && OnModPhrasesLoaded != null)
                OnModPhrasesLoaded();

            if (File.Exists(phrasesFilePath))
                LoadPhrasesFile();
            else WritePhrasesFile();

            if (File.Exists(phrasesFilePath))
            {
                phrasesFileWatcher = new FileSystemWatcher(modConfigPath, phrasesFilename);
                phrasesFileWatcher.Changed += new FileSystemEventHandler(OnPhrasesFileChanged);
                phrasesFileWatcher.Created += new FileSystemEventHandler(OnPhrasesFileChanged);
                phrasesFileWatcher.Deleted += new FileSystemEventHandler(OnPhrasesFileChanged);
                phrasesFileWatcher.EnableRaisingEvents = true;
            }

            enabled = true;

            return true;
        }
        catch (XmlException e)
        {
            Log.Error($"Loading mod {modName} failed: {e}");
            return false;
        }
    }
    private void OnSettingsFileChanged(object source, FileSystemEventArgs e)
    {
        if (File.Exists(settingsFilePath))
            LoadSettingsFile();
    }
    public bool LoadSettingsFile()
    {
        if (!File.Exists(settingsFilePath))
        {
            OnModConfigLoaded();
            return false;
        }

        XmlDocument xmlDoc = new XmlDocument();

        try
        {
            xmlDoc.Load(settingsFilePath);

            XmlNode _XmlNode = xmlDoc.DocumentElement;
            foreach (XmlNode childNode in _XmlNode.ChildNodes)
            {
                if (childNode.NodeType != XmlNodeType.Comment && childNode.NodeType == XmlNodeType.Element)
                    settings[childNode.Name] = childNode.InnerText;
            }

            OnModConfigLoaded();

            return true;
        }
        catch (XmlException e)
        {
            Log.Error($"Failed loading settings from {settingsFilePath}: {e}");
            return false;
        }
    }
    public bool WriteSettingsFile()
    {
        if (settings.Count < 1)
            return true;

        Log.Out($"Writing {settingsFilePath}");

        try
        {
            Directory.CreateDirectory(modConfigPath);
            using (StreamWriter sw = new StreamWriter(settingsFilePath))
            {
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                sw.WriteLine("<Settings>");

                string lastSection = "";
                foreach (KeyValuePair<string, string> kvp in settings)
                {
                    // Add section seperators as comment
                    if (kvp.Key.Contains("_"))
                    {
                        string section = kvp.Key.Substring(0, kvp.Key.IndexOf("_", StringComparison.Ordinal));

                        if (!section.Equals(lastSection))
                        {
                            sw.WriteLine("<!--");
                            sw.WriteLine("\t\t\t{0}", section);
                            sw.WriteLine("-->");
                        }

                        lastSection = section;
                    }

                    // Add settings description as comment
                    if (settingsDes.TryGetValue(kvp.Key, out string description))
                        sw.WriteLine("<!-- {0} -->", description.Replace("-->", "->"));

                    sw.WriteLine("<{0}>{1}</{0}>", kvp.Key, SecurityElement.Escape(kvp.Value), kvp.Key);
                }

                sw.WriteLine("</Settings>");

                sw.Flush();
                sw.Close();
            }
        }
        catch (XmlException e)
        {
            Log.Error($"Failed writing to {settingsFilePath}: {e.Message}");
        }

        return false;
    }
    private void OnPhrasesFileChanged(object source, FileSystemEventArgs e)
    {
        if (File.Exists(settingsFilePath))
            LoadPhrasesFile();
    }
    public bool LoadPhrasesFile()
    {
        if (!File.Exists(phrasesFilePath))
            return false;

        XmlDocument xmlDoc = new XmlDocument();

        try
        {
            xmlDoc.Load(phrasesFilePath);

            XmlNode _XmlNode = xmlDoc.DocumentElement;
            foreach (XmlNode childNode in _XmlNode.ChildNodes)
            {
                if (childNode.NodeType != XmlNodeType.Comment && childNode.NodeType == XmlNodeType.Element)
                    phrases[childNode.Name] = childNode.InnerText;
            }

            OnModPhrasesLoaded();

            return true;
        }
        catch (XmlException e)
        {
            Log.Error($"Failed loading from {phrasesFilePath}: {e.Message}");
            return false;
        }
    }
    public bool WritePhrasesFile()
    {
        if (phrases.Count < 1)
            return true;

        Log.Out($"Writing {phrasesFilePath}");

        try
        {
            Directory.CreateDirectory(modConfigPath);
            using (StreamWriter sw = new StreamWriter(phrasesFilePath))
            {
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                sw.WriteLine("<Phrases>");

                string lastSection = "";
                foreach (KeyValuePair<string, string> kvp in phrases)
                {
                    // Add section seperators as comment
                    if (kvp.Key.Contains("_"))
                    {
                        string section = kvp.Key.Substring(0, kvp.Key.IndexOf("_", StringComparison.Ordinal));

                        if (!section.Equals(lastSection))
                        {
                            sw.WriteLine("<!--");
                            sw.WriteLine("\t\t\t{0}", section);
                            sw.WriteLine("-->");
                        }

                        lastSection = section;
                    }

                    // Add settings description as comment
                    if (phrasesDes.TryGetValue(kvp.Key, out string description))
                        sw.WriteLine("<!-- {0} -->", description.Replace("-->", "->"));

                    sw.WriteLine("<{0}>{1}</{0}>", kvp.Key, SecurityElement.Escape(kvp.Value), kvp.Key);
                }

                sw.WriteLine("</Phrases>");

                sw.Flush();
                sw.Close();
            }
        }
        catch (XmlException e)
        {
            Log.Error($"Failed writing to {phrasesFilePath}: {e.Message}");
        }

        return false;
    }
}
