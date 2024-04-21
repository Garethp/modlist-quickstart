using System.Linq;
using HarmonyLib;
using ModlistQuickstart.ModlistManager;
using RimWorld;
using UnityEngine;
using Verse;

namespace ModlistQuickstart.Patches;

[HarmonyPatch(typeof(MainMenuDrawer), nameof(MainMenuDrawer.DoMainMenuControls))]
public class MainMenuPatch
{
    public static void Prefix(ref Rect rect, bool anyMapFiles)
    {
        if (Current.ProgramState != ProgramState.Entry) return;
        
        var modlists = DefDatabase<ModlistDef>.AllDefsListForReading;
        if (modlists.Count == 0) return;
        
        var modlist = modlists.First();
        var label = $"{modlist.modlistName} Quickstart";

        var mod = LoadedModManager.GetMod<ModlistQuickstart>();
        mod.AutoloadSave();
        
        var height = Mathf.Max(45, Text.CalcHeight(label, 170f));
        
        var drawerRect = new Rect(rect.x, rect.y, 170f, height);
        
        if (Widgets.ButtonText(drawerRect, label))
        {
            ModlistQuickstart.Controller ??= new Controller(modlist);
            
            ModlistQuickstart.Controller.OpenQuickStartWindow();
        }

        rect.y = rect.y + height + 7;
    }
}