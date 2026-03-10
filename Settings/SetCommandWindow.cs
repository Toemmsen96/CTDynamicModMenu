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
        public override bool IsEnabled 
        { 
            get => CTDynamicModMenu.Instance.showCommandWindow;
            set => CTDynamicModMenu.Instance.showCommandWindow = value;
        }
        public override KeyCode? Keybind { get; set; } = KeyCode.F11;

        public override void Execute(CommandInput? message)
        {
            CTDynamicModMenu.Instance.EnableCursor();
            CTDynamicModMenu.Instance.DisplayMessage($"Command window is now {(IsEnabled ? "on" : "off")}");
        }
    }
}