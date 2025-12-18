using CTDynamicModMenu.Commands;
using UnityEngine;

namespace CTDynamicModMenu.Settings
{
    public class ListCommands : CustomCommand
    {
        public override string Name => "ListCommands";

        public override string Description => "List all available commands";

        public override string Format => "/listcommands";
        //public override string AlternativeFormat => "/help";

        public override string Category => "Help";
        public override bool IsToggle => false;
        public override KeyCode? Keybind => KeyCode.F1;

        public override void Execute(CommandInput? message)
        {
            string commandList = "Available Commands:\n";
            foreach (var command in CTDynamicModMenu.Instance.registeredCommands)
            {
                commandList += $"{command.Name} - {command.Description}\n{command.Format}\n";
                if (command.IsToggle)
                {
                    commandList += $" (Toggle: {(command.IsEnabled ? "On" : "Off")})\n";
                }
            }
            CTDynamicModMenu.Instance.DisplayMessage(commandList);
            CTDynamicModMenu.Instance.DisplayMessage("Use /help <command> for more information on a specific command.");
        }
    }
}