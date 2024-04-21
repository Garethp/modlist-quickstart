using UnityEngine;
using Verse;

namespace ModlistQuickstart.ModlistManager;

enum Page
{
    ActionsToTake,
    DownloadingItems,
    ConfirmApplySettings,
}

[HotSwappable]
public class ModlistManagerWindow(Controller controller) : Window
{
    private Controller _controller = controller;
    private Page _page = Page.ActionsToTake;
    private Vector2 _scrollPosition = Vector2.zero;

    public override void DoWindowContents(Rect inRect)
    {
        switch (_page)
        {
            case Page.ActionsToTake:
                ShowActionsToTake(inRect);
                break;
            case Page.DownloadingItems:
                ShowDownloadingItems(inRect);
                break;
            case Page.ConfirmApplySettings:
                ShowConfirmApplySettings(inRect);
                break;
        }
    }

    private void ShowActionsToTake(Rect inRect)
    {
        // Get the list of mods to subscribe to
        var modsToSubscribe = _controller.GetModsToSubscribeTo();

        if (modsToSubscribe.Count == 0)
        {
            _page = Page.ConfirmApplySettings;
            return;
        }
        
        // Create a label for the list
        Widgets.Label(new Rect(inRect.x, inRect.y, inRect.width, 30), "Mods to download from Steam Workshop:");

        // Create a list of mods
        float listY = inRect.y + 40;
        foreach (var mod in modsToSubscribe)
        {
            Widgets.Label(new Rect(inRect.x, listY, inRect.width, 30), mod.Name);
            listY += 30;
        }

        // Create a button to confirm the action
        if (Widgets.ButtonText(new Rect(inRect.x, inRect.height - 40, (inRect.width / 2) - 16, 30), "Confirm"))
        {
            _page = Page.DownloadingItems;
            _controller.SubScribeToMissingMods();
        }
        
        if (Widgets.ButtonText(new Rect((inRect.width / 2), inRect.height - 40, (inRect.width / 2) - 16, 30), "Cancel"))
        {
            Close();
        }
    }

    private void ShowDownloadingItems(Rect inRect)
    {
        var desiredItems = _controller.GetDesiredItems();

        // Create a label for the list
        Widgets.Label(new Rect(inRect.x, inRect.y, inRect.width, 30), "Mods to download:");

        // Create a list of mods
        float listY = 0;
        var viewRect = new Rect(0, 0, inRect.width - 16, desiredItems.Count * 30);

        Widgets.BeginScrollView(new Rect(inRect.x, inRect.y + 40, inRect.width, inRect.height - 80),
            ref this._scrollPosition, viewRect);

        var allFinished = true;

        foreach (var mod in desiredItems)
        {
            if (mod.PackageId.StartsWith("ludeon.rimworld")) return;
            
            Widgets.Label(new Rect(0, listY, viewRect.width / 2, 30), mod.Name);

            var nullableDownloadStatus = _controller.GetDownloadStatus(mod);
            if (nullableDownloadStatus is { } downloadStatus)
            {
                Rect barRect = new Rect(viewRect.width / 2, listY, viewRect.width / 4, 20);
                Widgets.FillableBar(barRect, downloadStatus.Progress / 100f);
                Widgets.Label(
                    new Rect(3 * viewRect.width / 4, listY, viewRect.width / 4, 20),
                    $"{downloadStatus.Progress}%");
            }

            if (nullableDownloadStatus?.State != DownloadState.Completed) allFinished = false;

            listY += 30;
        }

        Widgets.EndScrollView();

        if (allFinished)
        {
            if (Widgets.ButtonText(
                    new Rect(inRect.x, inRect.y + inRect.height - 30, inRect.width - 16, 30),
                    "Next",
                    true,
                    true
                    , Color.white)
               )
            {
                _page = Page.ConfirmApplySettings;
            }
        }
    }

    private void ShowConfirmApplySettings(Rect inRect)
    {
        Widgets.Label(new Rect(inRect.x, inRect.y, inRect.width, 30), "This next step will do the following:");
        Widgets.Label(new Rect(inRect.x + 15, inRect.y + 30, inRect.width, 30),
            "Copy a Save file into your Rimworld Saves");
        Widgets.Label(new Rect(inRect.x + 15, inRect.y + 60, inRect.width, 30),
            "Copy Mod Configs, overwriting any existing ones");
        Widgets.Label(new Rect(inRect.x + 15, inRect.y + 90, inRect.width, 30),
            "Set your active mods to the ones from this Modlist");

        Widgets.Label(new Rect(inRect.x, inRect.y + 150, inRect.width, 30), "Press Next to apply these settings and restart RimWorld");

        if (Widgets.ButtonText(
                new Rect(inRect.x, inRect.y + inRect.height - 30, (inRect.width - 16) / 2, 30),
                "Next",
                true,
                true
                , Color.white)
           )
        {
            _controller.ImportSettings();
            _controller.CopySave();
            _controller.SetModlist();
            
            ModsConfig.RestartFromChangedMods();
        }
        
        if (Widgets.ButtonText(
                new Rect(inRect.x + (inRect.width / 2) , inRect.y + inRect.height - 30, (inRect.width - 16) / 2, 30),
                "Cancel",
                true,
                true
                , Color.white)
           )
        {
            Close();
        }
    }
}