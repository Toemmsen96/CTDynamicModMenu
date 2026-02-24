using CTDynamicModMenu.Commands;
using UnityEngine;

namespace CTDynamicModMenu.Settings
{
    public class ToggleLogWindow : CustomCommand
    {
        public override string Name => "Toggle Log Window";

        public override string Description => "Turn on or Off the menu's log window";

        public override string Format => "/togglelogwindow";

        public override string Category => "Settings";
        public override bool IsToggle => true;
        public override bool IsEnabled 
        { 
            get => CTDynamicModMenu.Instance.showLogWindow;
            set => CTDynamicModMenu.Instance.showLogWindow = value;
        }
        public override bool HasConfig => true;
        public override bool PersistConfig => true;

        public override void Execute(CommandInput? message)
        {
            CTDynamicModMenu.Instance.DisplayMessage($"Log window is now {(IsEnabled ? "on" : "off")}");
        }
    }
}