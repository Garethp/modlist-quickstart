## TODO

 * ~~Create a Def to define a modlist~~
 * ~~Subscribe to a pre-defined modlist on Steam Workshop~~
    * ~~We're probably going to need a scroll-bar for long lists~~
    * Can we get the size of each mod to download ahead of time? (We can, but it's annoying)
    * ~~Show a completion bar for mods that are either already downloaded or are completed~~
    * ~~Add an "Ok" button when we're done~~
      * ~~Should we add a cancel button?~~
    * ~~If there are no mods to download, we should show something else~~
 * ~~Set a pre-defined modlist load order~~
 * ~~Copy a pre-defined set of mod configs into the config folder~~
   * ~~We already have the module from the modlist config, we just need to hook it up and test it~~
   * ~~We don't need to worry about pre-sets, so we just need to use the modconfig to store the last set config so that we can auto-update~~
 * ~~Copy a pre-defined save file to the saves folder~~
   * We probably want to warn the user if there's already a save by that name
   * Maybe we can give the user the option to skip the save?
   * ~~Either that or we need to over-write the file...~~
   * ~~Also attempt to auto-load the save file on the next game start~~
     * Before auto-loading, we should double check that the mod-list matches what we expect 
 * ~~Patch a start button into the main menu~~
   * ~~Only if there's at least one modlist~~
   * ~~Show the modlist name on the button~~
 * ~~If Core isn't in the modlist, we should add it~~
   * ~~We should hide Core from the list of mods to download~~
   * ~~We also want to add Harmony before Core if it's not there, since this mod relies on it~~
 * ~~We need to restart RimWorld after applying changes~~
 * ~~Generate a def from the list of current mods~~
   * ~~We should do some basic validation on the text entered~~
   * ~~We should check if the save file exists (if the user entered it)~~
   * ~~Save this to the correct Path~~
   * ~~Inform the user where the file will be written to~~
   * ~~Warn if we're going to overwrite a file~~
   * ~~Don't skip HotSwap~~
   * ~~Give feedback after writing the file~~
 * ~~Validate the current def~~ 
   * ~~We should probably let the user know if they forgot to define mod configs~~ 
 * ~~Write Documentation on how to use this~~
 * Do some refactoring