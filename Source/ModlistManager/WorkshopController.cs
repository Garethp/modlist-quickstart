using System;
using System.Collections;
using System.Collections.Generic;
using Verse.Steam;
using Steamworks;

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
            downloadStatuses[mod.PackageId] = new DownloadStatus { State = DownloadState.Completed, Progress = 100 };
        }
        
        foreach (var mod in toInstall)
        {
            downloadStatuses[mod.PackageId] = new DownloadStatus { State = DownloadState.NotStarted, Progress = 0 };
            checkDebouncer[mod.PackageId] = 0;
        }
    }
    
    public void SubscribeToWorkshopFile(string fileId)
    {
        PublishedFileId_t publishedFileId = new PublishedFileId_t(ulong.Parse(fileId));
        SteamUGC.SubscribeItem(publishedFileId);
    }
    
    public void SubscribeToAllFiles(List<string> fileIds)
    {
        fileIds.ForEach(SubscribeToWorkshopFile);
    }

    public DownloadStatus? GetDownloadStatus(ModData mod)
    {
        if (!downloadStatuses.ContainsKey(mod.PackageId))
        {
            return null;
        }
        
        if (downloadStatuses[mod.PackageId].State == DownloadState.Completed)
        {
            return downloadStatuses[mod.PackageId];
        }
        
        // Debounce the check to avoid spamming the Steam API. Check debouncer should contain next time to check in miliseconds
        if (checkDebouncer[mod.PackageId] >= DateTimeOffset.Now.ToUnixTimeMilliseconds())
        {
            return downloadStatuses[mod.PackageId];
        }

        checkDebouncer[mod.PackageId] = (int) DateTimeOffset.Now.ToUnixTimeMilliseconds() + 1000;
        
        // Using SteamUGC, get the download status of the file as a percentage integer
        // Return -1 if the file is not downloading

        var publishedFileId = new PublishedFileId_t(ulong.Parse(mod.FileId));
        
        if (!SteamUGC.GetItemDownloadInfo(publishedFileId, out var bytesDownloaded, out var bytesTotal))
        {
            return null;
        }
        
        if (bytesTotal == 0) return downloadStatuses[mod.PackageId];

        var progress = (int)((double)bytesDownloaded / bytesTotal * 100);

        if (progress >= 99)
        {
            downloadStatuses[mod.PackageId] = new DownloadStatus { State = DownloadState.Completed, Progress = 100 };
        }
        else
        {
            downloadStatuses[mod.PackageId] = new DownloadStatus { State = DownloadState.Downloading, Progress = progress };
        }

        return downloadStatuses[mod.PackageId];
    }
}