using System.Collections.Generic;
using System.Linq;
using System.Xml;
using JetBrains.Annotations;
using ModlistQuickstart.ModlistManager;
using Verse;

namespace ModlistQuickstart;

public class EarlyModlistDefLoader
{
    [CanBeNull]
    private static XmlNode GetDefsNode(XmlDocument xmlDocument)
    {
        for (var i = 0; i < xmlDocument.ChildNodes.Count; i++)
        {
            var childNode = xmlDocument.ChildNodes[i];
            if (childNode is XmlDeclaration) continue;

            return childNode;
        }

        return null;
    }

    public static List<ModlistDef> GetModlistDefs()
    {
        var mods = LoadedModManager.RunningMods;
        List<ModlistDef> defs = new();

        var defClassName = new ModlistDef().GetType().FullName;

        var modContentPack = LoadedModManager.GetMod<ModlistQuickstart>().Content;

        var presetDefs = modContentPack.LoadDefs()
            .Where(loadableAsset => GetDefsNode(loadableAsset.xmlDoc)?.FirstChild is not null)
            .Select(loadableAsset => GetDefsNode(loadableAsset.xmlDoc)?.FirstChild)
            .Where(defNode => defNode.Name == defClassName)
            .ToList();

        foreach (var presetDef in presetDefs)
        {
            // For each `<ModlistConfigurator.ModlistPresetDef>` node, grab the defName, presetLabel and version
            var defName = XmlUtils.GetChildNodeByName(presetDef, "defName")?.InnerText;
            var modlistName = XmlUtils.GetChildNodeByName(presetDef, "modlistName")?.InnerText;
            var saveFileName = XmlUtils.GetChildNodeByName(presetDef, "saveFileName")?.InnerText;
            var configVersion = XmlUtils.GetChildNodeByName(presetDef, "configVersion")?.InnerText;

            if (defName is null || modlistName is null || configVersion is null) continue;

            defs.Add(new ModlistDef(defName, modlistName, saveFileName, configVersion, modContentPack));
        }

        return defs;
    }
}