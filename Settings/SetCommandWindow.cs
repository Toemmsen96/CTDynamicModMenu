using CTDynamicModMenu.Commands;
using UnityEngine;

namespace CTDynamicModMenu.Settings
{
    public class SetCommandWindow : CustomCommand
    {
        public override string Name => "SetCommandWindow";

        public override string Description => "Turn on or Off the log window";

        public override string Format => "/setcommandwindow";
        public override string Category => "Settings";
        public override bool IsToggle => true;
        public override bool IsEnabled => CTDynamicModMenu.Instance.showCommandWindow;
        public override KeyCode? Keybind => KeyCode.F11;

        public override void Execute(CommandInput? message)
        {
            CTDynamicModMenu.Instance.showCommandWindow = !CTDynamicModMenu.Instance.showCommandWindow;
            IsEnabled = CTDynamicModMenu.Instance.showCommandWindow;
            CTDynamicModMenu.Instance.DisplayMessage($"Command window is now {(CTDynamicModMenu.Instance.showCommandWindow ? "on" : "off")}");
        }
    }
}