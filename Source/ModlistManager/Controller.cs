﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using Verse;

namespace ModlistQuickstart.ModlistManager;

public class Controller
{
    private readonly ModlistDef _modlist;
    private readonly WorkshopController _workshopController;

    public Controller(ModlistDef modlist)
    {
        var installedMods = GetInstalledMods();

        var mods = modlist.mods.Select(mod =>
        {
            mod.PackageId = mod.PackageId.ToLower();
            return mod;
        }).ToList();
        
        if (!mods.Any(mod => mod.PackageId == "ludeon.rimworld"))
        {
            mods = mods.Prepend(new ModData("Core", "ludeon.rimworld", "")).ToList();
        }

        if (!mods.Any(mod => mod.PackageId == "brrainz.harmony"))
        {
            mods = mods.Prepend(new ModData("Harmony", "brrainz.harmony", "2009463077")).ToList();
        }
        
        modlist.mods = mods;
        
        _modlist = modlist;
        _workshopController = new WorkshopController(
            _modlist.mods.Where(mod =>
                installedMods.FirstOrDefault(installedMod => installedMod.packageIdLowerCase == mod.PackageId.ToLower())
                    is not null).ToList(),
            GetModsToSubscribeTo()
        );

        var a = 1 + 1;
    }

    public void SubScribeToMissingMods()
    {
        var mods = GetModsToSubscribeTo();
        SubscribeToMods(mods);
    }

    public List<ModData> GetDesiredItems() => _modlist.mods;
    
    private List<ModMetaData> GetInstalledMods()
    {
        return ModLister.AllInstalledMods.ToList();
    }
    
    public List<ModData> GetModsToSubscribeTo()
    {
        var installedMods = GetInstalledMods();
        var mods = _modlist.mods
            .Where(mod => installedMods.All(installedMod => installedMod.PackageId.ToLower() != mod.PackageId.ToLower()))
            .ToList();

        return mods;
    }
    
    public DownloadStatus? GetDownloadStatus(ModData mod)
    {
        return _workshopController.GetDownloadStatus(mod);
    }

    public bool HasSave()
    {
        return _modlist.GetSavePath() is not null;
    }
    
    public void CopySave()
    {
        var savePath = _modlist.GetSavePath();
        if (savePath is null) return;
        
        SaveManager.CopySave(savePath);
        LoadedModManager.GetMod<ModlistQuickstart>().SetAutoloadSave(Path.GetFileName(savePath).Replace(".rws", ""));
    }
    
    private void SubscribeToMods(List<ModData> mods)
    {
        _workshopController.SubscribeToAllFiles(mods.Select(mod => mod.FileId).ToList());
    }

    public void OpenQuickStartWindow()
    {
        Find.WindowStack.Add(new ModlistManagerWindow(this));
    }

    public void SetModlist()
    {
        ModsConfig.SaveFromList(_modlist.mods.Select(mod => mod.PackageId).ToList());
    }

    public void ImportSettings()
    {
        LoadedModManager.GetMod<ModlistQuickstart>().ImportSettings(_modlist);
    }
}