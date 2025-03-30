using CTDynamicModMenu.Commands;
using UnityEngine;

namespace CTDynamicModMenu.Settings
{
    public class SetLogWindow : CustomCommand
    {
        public override string Name => "SetLogWindow";

        public override string Description => "Turn on or Off the log window";

        public override string Format => "/setlogwindow";

        public override string Category => "Settings";
        public override bool IsToggle => true;
        public override KeyCode? Keybind => KeyCode.F11;

        public override void Execute(CommandInput message)
        {
            CTDynamicModMenu.Instance.showLogWindow = !CTDynamicModMenu.Instance.showLogWindow;
            CTDynamicModMenu.Instance.DisplayMessage($"Log window is now {(CTDynamicModMenu.Instance.showLogWindow ? "on" : "off")}");
        }
    }
}