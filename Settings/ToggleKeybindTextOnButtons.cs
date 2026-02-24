using CTDynamicModMenu.Commands;
using UnityEngine;

namespace CTDynamicModMenu.Settings
{
    public class ToggleButtonKeybindText : CustomCommand
    {
        public override string Name => "ToggleButtonKeybindText";

        public override string Description => "Toggle the display of keybind text on the screen";

        public override string Format => "/togglebuttonkeybindtext";
        public override string Category => "Settings";
        public override bool IsToggle => true;
        public override bool IsEnabled 
        { 
            get => CTDynamicModMenu.Instance.showButtonKeybindText;
            set => CTDynamicModMenu.Instance.showButtonKeybindText = value;
        }
        public override bool HasConfig => true;
        public override bool PersistConfig => true;

        public override void Execute(CommandInput? message)
        {
            CTDynamicModMenu.Instance.DisplayMessage($"Keybind text display is now {(IsEnabled ? "on" : "off")}");
        }
    }
}