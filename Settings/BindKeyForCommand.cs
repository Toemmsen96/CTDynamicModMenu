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

        public override void Execute(CommandInput? message)
        {
            if (message == null){
                 CTDynamicModMenu.Instance.DisplayError("Invalid command input.");    
                return;
            }
            if (message.Args.Count > 1)
            {
                string commandName = message.Args[0];
                string keyName = message.Args[1];

                
                var command = FindCommandByName(commandName);
                
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
                    CTDynamicModMenu.Instance.DisplayError($"Command not found: {commandName}");
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
                    CTDynamicModMenu.Instance.DisplayError($"Invalid key: {keyName}. Use Unity KeyCode names.");
                }
            }
            else
            {
                CTDynamicModMenu.Instance.DisplayError("Usage: " + Format);
            }
        }
        private CustomCommand? FindCommandByName(string name)
        {
            CustomCommand? returnCommand = CTDynamicModMenu.Instance.registeredCommands.Find(cmd => cmd.Name.ToLower().Contains(name.ToLower()));
            if (returnCommand == null)
            {
                CTDynamicModMenu.Instance.registeredCommands.ForEach(cmd =>
                {
                    if (cmd.Format.ToLower().Contains(name.ToLower()))
                    {
                        returnCommand = cmd;
                    }
                });
            }
            return returnCommand;
        }
    }
}