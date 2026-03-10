using CTDynamicModMenu.Commands;
using UnityEngine;

namespace CTDynamicModMenu.Settings
{
    public class ToggleMenuButton : CustomCommand
    {
        public override string Name => "ToggleMenuButton";

        public override string Description => "Toggle the display of the menu button on the screen";

        public override string Format => "/togglemenubutton";
        public override string Category => "Settings";
        public override bool IsToggle => true;
        public override bool IsEnabled 
        { 
            get => CTDynamicModMenu.Instance.showMenuButton;
            set => CTDynamicModMenu.Instance.showMenuButton = value;
        }
        public override bool HasConfig => true;
        public override bool PersistConfig => true;

        public override void Execute(CommandInput? message)
        {
            CTDynamicModMenu.Instance.DisplayMessage($"Menu button display is now {(IsEnabled ? "on" : "off")}");
        }
    }
}