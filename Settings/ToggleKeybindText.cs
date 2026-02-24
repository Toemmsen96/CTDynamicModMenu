using CTDynamicModMenu.Commands;
using UnityEngine;

namespace CTDynamicModMenu.Settings
{
    public class ToggleKeybindText : CustomCommand
    {
        public override string Name => "ToggleKeybindText";

        public override string Description => "Toggle the display of keybind text on the screen";

        public override string Format => "/togglekeybindtext";
        public override string Category => "Settings";
        public override bool IsToggle => true;
        public override bool IsEnabled 
        { 
            get => CTDynamicModMenu.Instance.showKeybindText;
            set => CTDynamicModMenu.Instance.showKeybindText = value;
        }
        public override bool HasConfig => true;
        public override bool PersistConfig => true;

        public override void Execute(CommandInput? message)
        {
            CTDynamicModMenu.Instance.DisplayMessage($"Keybind text display is now {(IsEnabled ? "on" : "off")}");
        }
    }
}