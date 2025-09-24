using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using ModlistQuickstart.ModlistManager;
using UnityEngine;
using Verse;

namespace ModlistQuickstart;

enum Page
{
    None,
    GenerateDef,
    GenerationComplete,
    ValidateDef
}

[HotSwappable]
public class Settings : ModSettings
{
    private Page _page = Page.None;
    public string AutoloadSave = "";
    public StoredConfigs CurrentlyLoadedConfigs = new();

    private List<string> unpublishedMods = [];
    private bool checkedCanGenerate = false;
    private bool errorShown = false;

    private string GetModSteamId(ModContentPack mod)
    {
        if (mod.ModMetaData.GetPublishedFileId().m_PublishedFileId != 0)
        {
            var publishedFileId = $"{mod.ModMetaData.GetPublishedFileId()}";
            if (mod.ModMetaData.Source == ContentSource.SteamWorkshop && new Regex("^[0-9]+$").IsMatch(mod.ModMetaData.FolderName)) return mod.ModMetaData.FolderName;
            
            return publishedFileId;
        }
        
        if (mod.ModMetaData.Source != ContentSource.SteamWorkshop) return null;

        return mod.ModMetaData.FolderName;
    }

    public override void ExposeData()
    {
        Scribe_Values.Look(ref AutoloadSave, "autoloadSave", "");
        Scribe_Deep.Look(ref CurrentlyLoadedConfigs, "currentlyLoadedConfigs");
    }

    public void AutomaticSettingsImport(ModlistDef modlist)
    {
        if (CurrentlyLoadedConfigs is null) return;
        if (CurrentlyLoadedConfigs.Name != modlist.defName ||
            CurrentlyLoadedConfigs.Version == modlist.configVersion) return;

        CurrentlyLoadedConfigs = SettingsImporter.AutoUpdate(modlist, CurrentlyLoadedConfigs);

        Write();
    }

    public void ImportSettings(ModlistDef modlist)
    {
        CurrentlyLoadedConfigs = SettingsImporter.OverwriteSettings(modlist);
        Write();
    }

    public void DoWindowContents(Rect canvas)
    {
        if (checkedCanGenerate == false)
        {
            checkedCanGenerate = true;
            var thisPackageId = LoadedModManager.GetMod<ModlistQuickstart>().Content.PackageId;
            unpublishedMods = LoadedModManager.RunningModsListForReading.Where(
                    mod =>
                        !mod.PackageId.StartsWith("ludeon.rimworld") &&
                        mod.PackageId != thisPackageId &&
                        GetModSteamId(mod) == null
                )
                .Select(mod => mod.PackageId)
                .ToList();
        }

        var listing = new Listing_Standard();
        listing.Begin(canvas);

        var buttonPosition = listing.GetRect(34f);

        if (Widgets.ButtonText(buttonPosition.LeftHalf().ContractedBy(2f), "Generate Modlist Def"))
        {
            _page = Page.GenerateDef;
        }

        if (Widgets.ButtonText(buttonPosition.RightHalf().ContractedBy(2f), "Validate Modlist Def"))
        {
            _page = Page.ValidateDef;
        }

        switch (_page)
        {
            case Page.GenerateDef:
                ShowGenerateDef(canvas, listing);
                break;
            case Page.ValidateDef:
                ShowDefValidation(canvas, listing);
                break;
            case Page.GenerationComplete:
                ShowGenerationComplete(canvas, listing);
                break;
        }

        listing.End();
    }

    public string generateDefName = "";
    public string generateModlistName = "";
    public string generateSaveFileName = "";
    public string generateConfigVersion = "1";

    public void ShowGenerateDef(Rect canvas, Listing_Standard listing)
    {
        var modPath = LoadedModManager.GetMod<ModlistQuickstart>().Content.RootDir;

        listing.GapLine();
        listing.Gap();

        if (unpublishedMods.Count > 0)
        {
            Widgets.Label(listing.GetRect(34f), "Not all mods have published FileId on the Steam Workshop");

            if (!errorShown)
            {
                unpublishedMods.ForEach(id => Log.Error($"The following mod is unpublished: {id}"));
                Log.TryOpenLogWindow();
                errorShown = true;
            }

            return;
        }

        var defNameLine = listing.GetRect(34f);

        Widgets.Label(defNameLine.LeftHalf().ContractedBy(2f), "Def Name");
        generateDefName = Widgets.TextField(defNameLine.RightHalf().ContractedBy(2f), generateDefName);

        listing.Gap();
        var modlistNameLine = listing.GetRect(34f);

        Widgets.Label(modlistNameLine.LeftHalf().ContractedBy(2f), "Modlist Name");
        generateModlistName = Widgets.TextField(modlistNameLine.RightHalf().ContractedBy(2f), generateModlistName);

        listing.Gap();
        var savePathLine = listing.GetRect(34f);

        Widgets.Label(savePathLine.LeftHalf().ContractedBy(2f), "Save File Name (Optional)");
        generateSaveFileName = Widgets.TextField(savePathLine.RightHalf().ContractedBy(2f), generateSaveFileName);

        listing.Gap();
        var configVersionLine = listing.GetRect(34f);

        Widgets.Label(configVersionLine.LeftHalf().ContractedBy(2f), "Config Version");
        generateConfigVersion =
            Widgets.TextField(configVersionLine.RightHalf().ContractedBy(2f), generateConfigVersion);

        listing.Gap();
        Widgets.Label(listing.GetRect(34f),
            "The list of mods will be generated from the mods you have enabled in the mod manager");

        listing.Gap();
        var generateLine = listing.GetRect(34f);

        if (Widgets.ButtonText(generateLine.LeftHalf().ContractedBy(2f), "Generate"))
        {
            GenerateFile();
        }

        if (generateDefName.Length > 0)
        {
            var writePath = Path.Combine(LoadedModManager.GetMod<ModlistQuickstart>().Content.RootDir, "Defs",
                $"{generateDefName}.xml");

            listing.Gap();
            Widgets.Label(listing.GetRect(34f), $"Generate will write the def XML into: {writePath}");

            if (File.Exists(writePath))
            {
                listing.Gap();
                Widgets.Label(listing.GetRect(34f),
                    "A file already exists at that location, generating will overwrite that file");
            }
        }
    }

    public void GenerateFile()
    {
        var modPath = LoadedModManager.GetMod<ModlistQuickstart>().Content.RootDir;
        var errors = new List<string>();
        
        if (generateDefName.Length == 0)
        {
            errors.Add("You need to specify a Def Name");
        }
        
        if (generateModlistName.Length == 0)
        {
            errors.Add("You need to specify a Modlist Name");
        }
        
        if (generateConfigVersion.Length == 0)
        {
            errors.Add("You need to specify a Config Version");
        }
        
        if (generateSaveFileName.Length > 0)
        {
            if (!File.Exists(Path.Combine(modPath, generateSaveFileName)))
            {
                errors.Add("The save file you specified can't be found in the root of your mod folder");
            }
            else if (!generateSaveFileName.EndsWith(".rws"))
            {
                errors.Add("Your save file doesn't end in .rws, are you sure you have the correct file?");
            }
        }
        
        if (errors.Count > 0)
        {
            Find.WindowStack.Add(new Dialog_MessageBox(string.Join("\n", errors)));
            return;
        }
        
        var document = new XmlDocument();
        var root = document.AppendChild(document.CreateElement("Defs"));

        var def = root.AppendChild(document.CreateElement("ModlistQuickstart.ModlistDef"));
        def.AppendChild(document.CreateElement("defName")).InnerText = generateDefName;
        def.AppendChild(document.CreateElement("modlistName")).InnerText = generateModlistName;
        def.AppendChild(document.CreateElement("saveFileName")).InnerText = generateSaveFileName;
        def.AppendChild(document.CreateElement("configVersion")).InnerText = generateConfigVersion;

        var modList = def.AppendChild(document.CreateElement("mods"));
        LoadedModManager.RunningModsListForReading.ForEach(mod =>
        {
            var modNode = modList.AppendChild(document.CreateElement("li"));
            modNode.AppendChild(document.CreateElement("Name")).InnerText = mod.Name;
            modNode.AppendChild(document.CreateElement("PackageId")).InnerText = mod.PackageId;

            var thisPackageId = LoadedModManager.GetMod<ModlistQuickstart>().Content.PackageId;

            if (!mod.PackageId.StartsWith("ludeon.rimworld") && mod.PackageId != thisPackageId)
            {
                modNode.AppendChild(document.CreateElement("FileId")).InnerText =
                    GetModSteamId(mod);
            }
        });

        var pathToWriteTo = Path.Combine(modPath, "Defs", $"{generateDefName}.xml");

        if (File.Exists(pathToWriteTo))
        {
            File.Delete(pathToWriteTo);
        }

        if (!Directory.Exists(Path.Combine(modPath, "Defs")))
        {
            Directory.CreateDirectory(Path.Combine(modPath, "Defs"));
        }
        
        var saveStream = new FileStream(pathToWriteTo, FileMode.Create, FileAccess.Write, FileShare.None);

        var writer = XmlWriter.Create(saveStream, new XmlWriterSettings
        {
            Indent = true,
            IndentChars = "\t"
        });

        document.WriteTo(writer);
        writer.Close();
        saveStream.Close();

        _page = Page.GenerationComplete;
    }

    public void ShowGenerationComplete(Rect canvas, Listing_Standard listing)
    {
        listing.GapLine();
        listing.Gap();

        Widgets.Label(listing.GetRect(34f), "Generation complete");
    }
    
    public void ShowDefValidation(Rect canvas, Listing_Standard listing)
    {
        var allDefs = DefDatabase<ModlistDef>.AllDefsListForReading;
        var modPath = LoadedModManager.GetMod<ModlistQuickstart>().Content.RootDir;
        
        listing.GapLine();
        listing.Gap();
        
        if (allDefs.Count == 0)
        {
            Widgets.Label(listing.GetRect(34f),
                "No modlist defs found. If you just generated one, you need to restart the game");
            return;
        }

        if (allDefs.Count > 1)
        {
            Widgets.Label(listing.GetRect(34f), "More than one defs defined, only one is allowed");
            return;
        }

        var def = allDefs.First();
        var errors = new List<string>();

        if (!Directory.Exists(Path.Combine(modPath, "Settings")))
        {
            errors.Add($"No settings folder found at {Path.Combine(modPath, "Settings")}");
        }
        else if (Directory.GetFiles(Path.Combine(modPath, "Settings")).Length == 0)
        {
            errors.Add($"There are no config files found in {Path.Combine(modPath, "Settings")}");
        }
        
        if (def.saveFileName.Length > 0)
        {
            if (!def.saveFileName.EndsWith(".rws"))
            {
                errors.Add("The save file name must end in .rws");
            }

            if (def.saveFileName.Contains("/"))
            {
                errors.Add("The save file name can't contain a path");
            }

            if (def.GetSavePath() is null)
            {
                errors.Add("You've specified a save file, but it can't be found in the root of your mod folder");
            }
        }

        if (def.mods.Count == 0)
        {
            errors.Add("You haven't specified any mods");
        }

        if (def.mods.Any(mod =>
            {
                if (mod.PackageId.StartsWith("ludeon.rimworld")) return false;
                if (mod.PackageId == def.modContentPack.PackageId) return false;
                if (mod.PackageId.NullOrEmpty()) return true;
                if (mod.Name.NullOrEmpty()) return true;
                if (mod.FileId.NullOrEmpty()) return true;

                return false;
            }))
        {
            errors.Add("Not all mods have a Name, PackageId and FileId specified");
        }

        if (!def.mods.Any(mod => mod.PackageId == "ludeon.rimworld"))
        {
            errors.Add("Core is not specified. It will be added automatically, but you should consider adding it in anyway");
        }

        if (!def.mods.Any(mod => mod.PackageId == "brrainz.harmony"))
        {
            errors.Add("Harmony is not specified. It will be added automatically, but you should consider adding it in anyway");
        }

        var thisPackageId = LoadedModManager.GetMod<ModlistQuickstart>().Content.PackageId;

        if (!def.mods.Any(mod => mod.PackageId == thisPackageId))
        {
            errors.Add("This mod, the Modlist quickstart, is not defined in your modlist. While it is optional, quickstart saves won't work without it");
        }

        if (errors.Count > 0)
        {
            foreach (var error in errors)
            {
                Widgets.Label(listing.GetRect(34f), error);
                listing.Gap();
            }

            return;
        }
        
        Widgets.Label(listing.GetRect(34f), "The current loaded def looks valid");
    }
}