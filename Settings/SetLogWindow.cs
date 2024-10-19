using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTDynamicMenuMod.Commands;

namespace CTDynamicMenuMod.Settings
{
    public class SetLogWindow : CustomCommand
    {
        public override string Name => "SetLogWindow";

        public override string Description => "Turn on or Off the log window";

        public override string Format => "/setlogwindow";

        public override string Category => "Settings";

        public override void Execute(CommandInput message)
        {
            CTDynamicMenuMod.Instance.showLogWindow = !CTDynamicMenuMod.Instance.showLogWindow;
            CTDynamicMenuMod.Instance.DisplayMessage($"Log window is now {(CTDynamicMenuMod.Instance.showLogWindow ? "on" : "off")}");
        }
    }
}