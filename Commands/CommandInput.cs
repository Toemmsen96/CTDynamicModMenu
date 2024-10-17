using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CTDynamicMenuMod.Commands
{
    public class CommandInput
    {
        public string Command { get; private set; }

        public List<string> Args { get; private set; } = new List<string>();

        public static CommandInput Parse(string input)
        {
            // Check for command and args
            Regex regex = new Regex(@"/(\S+)(?:\s+(""([^""]+)""|\S+))*");
            Match match = regex.Match(input);

            if (!match.Success)
            {
                return null;
            }

            CommandInput command = new CommandInput();
            command.Command = match.Groups[1].Value;

            // Extract parameters
            GroupCollection groups = match.Groups;
            CaptureCollection captures = groups[2].Captures;
            for (int i = 0; i < captures.Count; i++)
            {
                command.Args.Add(captures[i].Value.Trim('"'));
            }

            return command;
        }
    }
}
