using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Verse.Steam;
using Steamworks;
using Verse;

namespace ModlistQuickstart.ModlistManager;

public enum DownloadState
{
    NotStarted,
    Downloading,
    Completed
}

public struct DownloadStatus
{
    public DownloadState State;

    public int Progress;
}

public class WorkshopController
{
    Dictionary<string, DownloadStatus> downloadStatuses = new Dictionary<string, DownloadStatus>();
    Dictionary<string, int> checkDebouncer = new Dictionary<string, int>();
    
    public WorkshopController(List<ModData> alreadyInstalled, List<ModData> toInstall)
    {
        foreach (var mod in alreadyInstalled)
        {
            if (mod.PackageId == "mss.messiah") continue;
            if (mod.PackageId.StartsWith("ludeon.")) continue;
            downloadStatuses[mod.FileId] = new DownloadStatus { State = DownloadState.Completed, Progress = 100 };
        }
        
        foreach (var mod in toInstall)
        {
            if (mod.PackageId == "mss.messiah") continue;
            if (mod.PackageId.StartsWith("ludeon.")) continue;
            downloadStatuses[mod.FileId] = new DownloadStatus { State = DownloadState.NotStarted, Progress = 0 };
            checkDebouncer[mod.FileId] = 0;
        }
        
        Callback<ItemInstalled_t>.Create(new Callback<ItemInstalled_t>.DispatchDelegate(this.ItemDownloaded));
    }
    
    public void SubscribeToWorkshopFile(string fileId)
    {
        PublishedFileId_t publishedFileId = new PublishedFileId_t(ulong.Parse(fileId));
        SteamUGC.SubscribeItem(publishedFileId);
    }
    
    public void SubscribeToAllFiles(List<string> fileIds)
    {
        Task.Run(async () =>
        {
            foreach (var fileId in fileIds)
            {
                SubscribeToWorkshopFile(fileId);
                await Task.Delay(50);
            }
            
            foreach (var fileId in fileIds)
            {
                SubscribeToWorkshopFile(fileId);
                await Task.Delay(50);
            }
        }).Start();
    }

    public void ItemDownloaded(ItemInstalled_t result)
    {
        downloadStatuses[$"{result.m_nPublishedFileId}"] = new DownloadStatus
            { State = DownloadState.Completed, Progress = 100 };
    }
    
    public DownloadStatus? GetDownloadStatus(ModData mod)
    {
        if (!downloadStatuses.ContainsKey(mod.FileId))
        {
            return null;
        }
        
        if (downloadStatuses[mod.FileId].State == DownloadState.Completed)
        {
            return downloadStatuses[mod.FileId];
        }
        
        // Debounce the check to avoid spamming the Steam API. Check debouncer should contain next time to check in miliseconds
        if (checkDebouncer[mod.FileId] >= (int) DateTimeOffset.Now.ToUnixTimeMilliseconds())
        {
            return downloadStatuses[mod.FileId];
        }

        checkDebouncer[mod.FileId] = (int) DateTimeOffset.Now.ToUnixTimeMilliseconds() + 1000;
        
        // Using SteamUGC, get the download status of the file as a percentage integer
        // Return -1 if the file is not downloading

        var publishedFileId = new PublishedFileId_t(ulong.Parse(mod.FileId));
        
        if (!SteamUGC.GetItemDownloadInfo(publishedFileId, out var bytesDownloaded, out var bytesTotal))
        {
            return null;
        }

        if (bytesTotal == 0) return downloadStatuses[mod.FileId];

        var progress = (int)((double)bytesDownloaded / bytesTotal * 100);

        if (progress >= 99)
        {
            downloadStatuses[mod.FileId] = new DownloadStatus { State = DownloadState.Completed, Progress = 100 };
        }
        else
        {
            downloadStatuses[mod.FileId] = new DownloadStatus { State = DownloadState.Downloading, Progress = progress };
        }

        return downloadStatuses[mod.FileId];
    }
}