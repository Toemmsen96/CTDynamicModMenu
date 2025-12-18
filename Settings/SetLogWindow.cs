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
        public override bool IsEnabled => CTDynamicModMenu.Instance.showLogWindow;

        public override void Execute(CommandInput? message)
        {
            CTDynamicModMenu.Instance.showLogWindow = !CTDynamicModMenu.Instance.showLogWindow;
            IsEnabled = CTDynamicModMenu.Instance.showLogWindow;
            CTDynamicModMenu.Instance.DisplayMessage($"Log window is now {(CTDynamicModMenu.Instance.showLogWindow ? "on" : "off")}");
        }
    }
}