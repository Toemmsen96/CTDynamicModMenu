using CTDynamicModMenu.Commands;
using UnityEngine;

namespace CTDynamicModMenu.Settings
{
    public class BindKeyForCommand : CustomCommand
    {
        public override string Name => "BindKeyForCommand";

        public override string Description => "Bind a key to a command";

        public override string Format => "/bindkey <command> <key>";

        public override string Category => "Settings";
        public override bool IsToggle => false;

        public override void Execute(CommandInput message)
        {
            if (message.Args.Count > 1)
            {
                string commandName = message.Args[0];
                string keyName = message.Args[1];

                
                var command = CTDynamicModMenu.Instance.registeredCommands.Find(cmd => cmd.Name.ToLower().Contains(commandName.ToLower()));
                if (command != null)
                {
                    // Check if the command is already bound to a key
                    if (command.Keybind != null)
                    {
                        CTDynamicModMenu.Instance.DisplayMessage($"Command {commandName} is already bound to {command.Keybind}");
                    }
                }
                else
                {
                    CTDynamicModMenu.Instance.DisplayMessage($"Command not found: {commandName}");
                    return;
                }

                // Parse the key
                KeyCode key;
                if (System.Enum.TryParse(keyName, true, out key))
                {
                    // Register the keybinding
                    command.Keybind = key;
                    CTDynamicModMenu.Instance.DisplayMessage($"Bound {key} to command {commandName}");
                }
                else
                {
                    CTDynamicModMenu.Instance.DisplayMessage($"Invalid key: {keyName}. Use Unity KeyCode names.");
                }
            }
            else
            {
                CTDynamicModMenu.Instance.DisplayMessage("Usage: " + Format);
            }
        }
    }
}