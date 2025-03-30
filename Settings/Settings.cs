using System.Collections.Generic;
using CTDynamicModMenu.Commands;

namespace CTDynamicModMenu.Settings
{
    public class Settings{
        public static List<CustomCommand> settingCommands = new List<CustomCommand>{
            new SetLogWindow(),
            new BindKeyForCommand(),
            new ListCommands()
        };
    }
}