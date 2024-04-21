using System.Collections.Generic;
using System.Xml;
using Verse;

namespace ModlistQuickstart.ModlistManager;

public class StoredConfigs: IExposable
{
    public StoredConfigs()
    {
    }

    public StoredConfigs(string name, string version)
    {
        Name = name;
        Version = version;
        Configs = new();
    }

    public string Name;
    public string Version;
    public List<StoredConfig> Configs = new();

    public void AddLoadedConfig(string modId, XmlNode config)
    {
        Configs.Add(new StoredConfig(modId, config));
    }

    public void ExposeData()
    {
        Scribe_Values.Look(ref Name, "Name");
        Scribe_Values.Look(ref Version, "Version");
        Scribe_Collections.Look(ref Configs, "Configs", LookMode.Deep);
    }
}

public class StoredConfig: IExposable
{
    public string Version;
    public List<StoredConfig> Configs = new();
    
    public StoredConfig()
    {
    }

    public StoredConfig(string modId, XmlNode settings)
    {
        ModId = modId;
        settingsString = settings.OuterXml;
    }

    private string settingsString;

    public string ModId;

    public XmlNode Settings
    {
        get
        {
            if (settingsString == null) return null;
            
            var document = new XmlDocument();
            document.LoadXml(settingsString);
            return document.ChildNodes[0];
        }
    }

    public void ExposeData()
    {
        Scribe_Values.Look(ref ModId, "ModId");
        Scribe_Values.Look(ref settingsString, "Settings");
    }
}