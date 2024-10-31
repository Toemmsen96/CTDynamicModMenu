# CT Mods Dynamic Mod menu
This BepInEx-Mod allows to create Dynamic Mod menus easily.  
## How To Use
- use the DLL of this mod as an Assembly Reference in your BepInEx-Mod.
- add a dependency for this BepInExMod
- use the included CustomCommand and CommandInput classes to create each Mod to execute
  -> f.e.: MyCustomCommand:CustomCommand   
- use ```RegisterCommand(CustomCommand) ``` to register any command you want to be executed  
  -> top of file: ```Using CTDynamicModMenu.CTDynamicModMenu;```  
  -> ```CTDynamicModMenu.Instance.RegisterCommand(MyCustomCommand)```   
- compile your mod
- Install both this and your mod as a plugin and enjoy
