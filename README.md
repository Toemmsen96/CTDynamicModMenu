# CT Mods Dynamic Mod menu
This BepInEx-Mod allows to create Dynamic Mod menus easily.  
## How To Use
1. use the DLL of this mod as an Assembly Reference in your BepInEx-Mod.
2. add a dependency for this BepInExMod
3. use the included CustomCommand and CommandInput classes to create each Mod to execute
  -> f.e.: MyCustomCommand:CustomCommand   
4. use ```RegisterCommand(CustomCommand) ``` to register any command you want to be executed  
  -> top of file: ```Using CTDynamicModMenu.CTDynamicModMenu;```  
  -> ```CTDynamicModMenu.Instance.RegisterCommand(MyCustomCommand)```   
5. compile your mod
6. Install both this and your mod as a plugin and enjoy
## Template
Here is Template which you can use to get a basic structure for your Mod:
https://github.com/Toemmsen96/CTDynMMTemplate

Using this Template you can skip steps 1-4 to have a first working Version.
