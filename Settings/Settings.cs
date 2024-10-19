using System.Collections.Generic;
using CTDynamicMenuMod.Commands;

namespace CTDynamicMenuMod.Settings
{
    public class Settings{
        public static List<CustomCommand> settingCommands = new List<CustomCommand>{
            new SetLogWindow(),
        };
    }
}