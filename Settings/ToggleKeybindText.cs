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
        public override bool IsEnabled => CTDynamicModMenu.Instance.showKeybindText;

        public override void Execute(CommandInput? message)
        {
            CTDynamicModMenu.Instance.showKeybindText = !CTDynamicModMenu.Instance.showKeybindText;
            IsEnabled = CTDynamicModMenu.Instance.showKeybindText;
            CTDynamicModMenu.Instance.DisplayMessage($"Keybind text display is now {(CTDynamicModMenu.Instance.showKeybindText ? "on" : "off")}");
        }
    }
}