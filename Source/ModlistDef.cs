using System.Collections.Generic;
using System.IO;
using System.Linq;
using ModlistQuickstart.ModlistManager;
using Verse;
namespace ModlistQuickstart;

public class ModlistDef : Def
{
    public ModlistDef()
    {
    }
    
    public ModlistDef(string defName, string modlistName, string saveFileName, string configVersion, ModContentPack modContentPack)
    {
        this.defName = defName;
        this.modlistName = modlistName;
        this.saveFileName = saveFileName;
        this.configVersion = configVersion;
        this.modContentPack = modContentPack;
    }

    
    public string modlistName;
    
    public string saveFileName;

    public string configVersion;
    
    public List<ModData> mods;

    public string GetSavePath()
    {
        if (saveFileName.Length == 0) return null;

        var saveLocation = Path.Combine(modContentPack.ModMetaData.RootDir.FullName, saveFileName);

        return !File.Exists(saveLocation) ? null : saveLocation;
    }

    public DirectoryInfo GetConfigPath()
    {
        var rootDir = modContentPack.ModMetaData.RootDir;

        return rootDir.GetDirectories().FirstOrDefault(dir => dir.Name == "Settings");
    }
}