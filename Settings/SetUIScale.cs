using CTDynamicModMenu.Commands;
using UnityEngine;

namespace CTDynamicModMenu.Settings
{
    public class SetUIScale : CustomCommand
    {
        public override string Name => "SetUIScale";

        public override string Description => "Set the UI scale factor (0.5 - 2.0)";

        public override string Format => "/setuiscale <scale>";
        public override string Category => "Settings";
        public override bool IsToggle => false;
        public override bool HasConfig => true;
        public override bool PersistConfig => true;

        public override void Execute(CommandInput? message)
        {
            if (message == null || message.Args.Count == 0)
            {
                CTDynamicModMenu.Instance.DisplayMessage($"Current UI scale: {CTDynamicModMenu.Instance.uiScale:F2}. Usage: /setuiscale <0.5-2.0>");
                return;
            }

            if (float.TryParse(message.Args[0], out float scale))
            {
                scale = Mathf.Clamp(scale, 0.5f, 2.0f);
                CTDynamicModMenu.Instance.uiScale = scale;
                CTDynamicModMenu.Instance.SaveConfig();
                CTDynamicModMenu.Instance.DisplayMessage($"UI scale set to {scale:F2}");
            }
            else
            {
                CTDynamicModMenu.Instance.DisplayMessage("Invalid scale value. Please use a number between 0.5 and 2.0");
            }
        }
    }
}
