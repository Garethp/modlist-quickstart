# Modlist Quickstart

Modlist Quickstart is my attempt at a "Modack in a Box" solution. By itself it's not a mod, but it's a tool for you to
define a modpack as a Rimworld mod and then have anyone who's subscribed to it be able to "Quickstart" that modpack, which
downloads all the required mods, sets the mod load order, copies over mod config files, optionally copies over a save
file and then restarts Rimworld directly into that save file on the next start.

## How to use it

1. Clone this project
2. Change the `About/About.xml` to reflect your modpack
3. Copy all of your mod configs into the `Settings` folder
4. Copy a `.rws` save file into the root folder (next to this README.md)
5. Either create a `ModlistQuickstart.ModlistDef` Def in the `Defs/` folder or have one generated automatically in Rimworld for you
6. Publish this mod
7. Share the link to the Steam Workshop page with your friends

## How it works

When a user subscribes to your modpack, they will see a new button in the main menu that says "Quickstart". When they click
that button, it will determine which mods they need to subscribe to over Steam, subscribe to them and when they're finished
downloading it will prompt the player one more time to confirm that they'd like to overwrite their mod configs and mod list.
After confirming, the Quickstart will copy over the mod configs, set the mod load order, copy over the save file and then
reboot. When it reboots, if the Quickstart mod is still enabled, it will automatically load the save file.

## How to create a ModlistDef

### Automatically

When you start Rimworld with this mod enabled you can go into `Options -> Mod Options -> Modlist Quickstart` and you'll
see a button that says "Generate Modlist Def". Clicking this button will show you a form asking you to enter a "Def Name",
"Modlist Name", "Save File Name (Optional)" and "Config Version". If you've entered this all in and click the "Generate"
button, it will automatically create a XML file in your `Defs` folder with the "Def Name" as the file name. You can then
restart Rimworld and click "Validate Modlist Def" in that same settings panel to double check that everything is correct
before publishing.

### Manually

A `ModlistQuickstart.ModlistDef` is a `Def` with the following fields: `defName`, `modlistName`, `saveFileName`, `configVersion`
and `mods`. Here's an example:

```xml
<Defs>
    <ModlistQuickstart.ModlistDef>
        <defName>TestingModlist</defName>
        <modlistName>Test</modlistName>
        <saveFileName>ModlistQuickstart.rws</saveFileName>
        <configVersion>1</configVersion>
        
        <mods>
            <li>
                <Name>Modlist Quickstart</Name>
                <PackageId>garethp.modlistquickstart</PackageId>
                <FileId>2910546545im</FileId>
            </li>
        </mods>
    </ModlistQuickstart.ModlistDef>
</Defs>
```

## Updating Mod Configs

If you want to distribute updated mod configs, just paste your new configs into the `Settings` folder and update the
`configVersion` in your def, then re-publish your mod. When players playing on this modpack get the updated version
of this mod, it'll automatically update their mod configs.