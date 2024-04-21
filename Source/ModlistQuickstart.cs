using System.Linq;
using HarmonyLib;
using ModlistQuickstart.ModlistManager;
using RimWorld;
using UnityEngine;
using Verse;

namespace ModlistQuickstart;

public class ModlistQuickstart : Mod
{
    private Settings _settings;
    private bool autoloadAttempted = false;
    public static Controller Controller;

    public ModlistQuickstart(ModContentPack content) : base(content)
    {
        var modlists = EarlyModlistDefLoader.GetModlistDefs();

        if (modlists.Count == 1)
        {
            GetSettings<Settings>().AutomaticSettingsImport(modlists.First());
        }

        new Harmony("Garethp.rimworld.ModlistQuickstart.main").PatchAll();
    }

    public override void DoSettingsWindowContents(Rect inRect)
    {
        base.DoSettingsWindowContents(inRect);

        GetSettings<Settings>().DoWindowContents(inRect);
    }

    public override string SettingsCategory() => "Modlist Quickstart";

    public void AutoloadSave()
    {
        if (autoloadAttempted) return;

        var settings = GetSettings<Settings>();

        if (settings.AutoloadSave.Length == 0) return;

        autoloadAttempted = true;

        GameDataSaveLoader.LoadGame(settings.AutoloadSave);
        settings.AutoloadSave = "";

        settings.Write();
    }

    public void SetAutoloadSave(string saveName)
    {
        var settings = GetSettings<Settings>();
        settings.AutoloadSave = saveName;
        settings.Write();
    }
    
    public void ImportSettings(ModlistDef modlist)
    {
        GetSettings<Settings>().ImportSettings(modlist);
    }
}