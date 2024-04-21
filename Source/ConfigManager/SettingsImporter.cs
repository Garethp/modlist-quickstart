using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using JetBrains.Annotations;
using Verse;

namespace ModlistQuickstart.ModlistManager;

public class SettingsImporter
{
    private static string GetSettingsFilename(string modIdentifier, string modHandleName) => Path.Combine(
        GenFilePaths.ConfigFolderPath,
        GenText.SanitizeFilename(string.Format("Mod_{0}_{1}.xml", (object)modIdentifier, (object)modHandleName)));

    /**
     * This is our "safe" way of loading updated settings. How is it safe?
     *
     * First, we generate a diff between the last version of a preset and the new version
     * Then, we generate a diff between the current user settings and last version of the preset
     *
     * Given that, we know exactly which settings were changed in the update and which settings the user has changed
     * since the previous version.
     *
     * We can then call `NodesAreCompatible` which will check if our update will override any settings that the user has
     * changed manually. If it doesn't override settings which the user has changed, we can safely apply the update.
     *
     * If it does, we should probably let the user know that there's a conflict and let them decide what to do.
     * @TODO: Let the user know there's a conflict and let them decide what to do.
     */
    [CanBeNull]
    public static StoredConfigs AutoUpdate(ModlistDef modlistDef, StoredConfigs previousSettings)
    {
        var storedConfigs = new StoredConfigs(modlistDef.defName, modlistDef.configVersion);

        var configDirectory = modlistDef.GetConfigPath();
        if (configDirectory is null) return new StoredConfigs();
        
        foreach (var file in configDirectory.GetFiles())
        {
            var match = Regex.Match(file.Name, @"^Mod_(.*)_(.*).xml$");
            if (!match.Success) continue;

            var filePath = file.FullName;
            var modId = match.Groups[1].Value;
            var modName = match.Groups[2].Value;

            var currentPreset = previousSettings.Configs.First(config => config.ModId == modId)?.Settings;

            var currentUserSettings = GetSettingsFromFile(GetSettingsFilename(modId, modName));
            var newPresetSettings = GetSettingsFromFile(filePath)!.DocumentElement;

            var userDiff = XmlUtils.GenerateDiff(currentPreset, currentUserSettings!.DocumentElement);
            var updatesToApply = XmlUtils.GenerateDiff(currentPreset, newPresetSettings);

            if (updatesToApply is null) continue;
            if (userDiff is not null && !XmlUtils.NodesAreCompatible(userDiff, updatesToApply))
            {
                // @TODO: Store this and let the user know there's a conflict
                continue;
            }

            var newSettings = XmlUtils.MergeNodes(updatesToApply, currentUserSettings!.DocumentElement);

            currentUserSettings.ReplaceChild(currentUserSettings.ImportNode(newSettings, true),
                currentUserSettings.DocumentElement);

            storedConfigs.AddLoadedConfig(modId, currentUserSettings.DocumentElement);

            var saveStream =
                (Stream)new FileStream(GetSettingsFilename(modId, modName), FileMode.Create,
                    FileAccess.Write, FileShare.None);

            var writer = XmlWriter.Create(saveStream, new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "\t"
            });

            currentUserSettings.WriteTo(writer);
            writer.Close();
            saveStream.Close();
        }

        return storedConfigs;
    }

    /**
     * While `AutoUpdate` is our "safe" way of loading updated settings, this is our sledgehammer. Screw whatever
     * the user settings were, just write our own settings in.
     */
    [CanBeNull]
    public static StoredConfigs OverwriteSettings(ModlistDef modlist)
    {
        var storedConfigs = new StoredConfigs(modlist.defName, modlist.configVersion);
        var configDirectory = modlist.GetConfigPath();

        if (configDirectory is null) return new StoredConfigs();

        foreach (var file in configDirectory.GetFiles())
        {
            var match = Regex.Match(file.Name, @"^Mod_(.*)_(.*).xml$");
            if (!match.Success) continue;

            var filePath = file.FullName;
            var modId = match.Groups[1].Value;
            var modName = match.Groups[2].Value;

            using var input = new StreamReader(filePath);
            using var reader = new XmlTextReader(input);

            var xmlDocument = new XmlDocument();
            xmlDocument.Load(reader);
            var settings = xmlDocument.DocumentElement;

            storedConfigs.AddLoadedConfig(modId, settings);

            var saveStream =
                (Stream)new FileStream(GetSettingsFilename(modId, modName), FileMode.Create,
                    FileAccess.Write, FileShare.None);

            var writer = XmlWriter.Create(saveStream, new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "\t"
            });

            xmlDocument.WriteTo(writer);
            writer.Close();
            saveStream.Close();
        }

        return storedConfigs;
    }
    
    public bool ShouldImport(string importFilePath, string modId, string modName)
    {
        var importSettings = GetSettingsFromFile(importFilePath)!.DocumentElement;
        var currentSettings = GetSettingsFromFile(GetSettingsFilename(modId, modName))!.DocumentElement;

        if (importSettings is null) return false;
        if (currentSettings is null) return true;

        return !XmlUtils.NodesAreEqual(importSettings, currentSettings);
    }

    [CanBeNull]
    public static XmlDocument GetSettingsFromFile(string fileLocation)
    {
        if (!File.Exists(fileLocation)) return null;

        using var reader = new StreamReader(fileLocation);
        using var xmlReader = new XmlTextReader(reader);
        var document = new XmlDocument();
        document.Load(xmlReader);
        xmlReader.Close();
        reader.Close();

        return document;
    }
}