using CTDynamicModMenu.Commands;
using UnityEngine;

namespace CTDynamicModMenu.Settings
{
    public class SetPersistance : CustomCommand
    {
        public override string Name => "SetPersistance";

        public override string Description => "Turn on or Off the persistance of settings / mods";
        public override string Format => "/setpersistance";
        public override string Category => "Settings";
        public override bool IsToggle => true;
        public override bool IsEnabled 
        { 
            get => CTDynamicModMenu.Instance.persistentSettings;
            set => CTDynamicModMenu.Instance.persistentSettings = value;
        }
        public override bool HasConfig => true;
        public override bool PersistConfig => true;

        public override void Execute(CommandInput? message)
        {
            CTDynamicModMenu.Instance.EnableCursor();
            CTDynamicModMenu.Instance.DisplayMessage($"Persistent settings is now {(IsEnabled ? "on" : "off")}");
        }
    }
}