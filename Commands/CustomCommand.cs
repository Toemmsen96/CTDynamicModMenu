using BepInEx;
using UnityEngine;

namespace CTDynamicModMenu.Commands
{
    public abstract class CustomCommand
    {
        public abstract string Name { get; }
        public abstract string Description { get; }
        public abstract string Format { get; }
        public virtual string? AlternativeFormat { get; } = null;
        public abstract string Category { get; }

        // Optional configuration functionality
        public virtual bool HasConfig { get; } = false;
        public virtual bool PersistConfig { get; } = false;
        
        // Optional toggle functionality
        public virtual bool IsToggle { get; } = false;
        public virtual bool IsEnabled { get; set; } = false;
        
        // Optional keybind functionality
        public virtual KeyCode? Keybind { get; set; } = null;
        public virtual bool RequireControlKey { get; set; } = false;
        public virtual bool RequireAltKey { get; set; } = false;
        public virtual bool RequireShiftKey { get; set; } = false;

        public bool Handle(string message)
        {
            CommandInput command = CommandInput.Parse(message);

            if (command == null)
            {
                return false;
            }

            // Check name
            if (command.Command != this.Format.Split(' ')[0].Trim('/'))
            {
                return false;
            }

            // Toggle state if it's a toggle command
            if (IsToggle)
            {
                IsEnabled = !IsEnabled;
                SaveConfig();
            }

            // Execute command
            this.Execute(command);
            return true;
        }

        public abstract void Execute(CommandInput? message);
        
        // Method to check if keybind is pressed
        public bool IsKeybindPressed()
        {
            if (Keybind == null) return false;
            bool modifiersMatch = 
                (!RequireControlKey || UnityInput.Current.GetKeyDown(KeyCode.LeftControl) || UnityInput.Current.GetKeyDown(KeyCode.RightControl)) &&
                (!RequireAltKey || UnityInput.Current.GetKeyDown(KeyCode.LeftAlt) || UnityInput.Current.GetKeyDown(KeyCode.RightAlt)) &&
                (!RequireShiftKey || UnityInput.Current.GetKeyDown(KeyCode.LeftShift) || UnityInput.Current.GetKeyDown(KeyCode.RightShift));
                
            return modifiersMatch && UnityInput.Current.GetKeyDown(Keybind.Value);
        }

        public void LoadConfig()
        {
            if (!HasConfig) return;
            if (Keybind != null)
            {
                Keybind = CTDynamicModMenu.Instance.Config.Bind<KeyCode>("Command Settings", $"{Name}: Toggle Key", Keybind.Value, "Key to toggle the menu").Value;
            }
            if (IsToggle)
            {
                IsEnabled = CTDynamicModMenu.Instance.Config.Bind<bool>("Command Settings", $"{Name}: IsEnabled", IsEnabled, "Whether the command is enabled").Value;
            }
        }
        public void SaveConfig()
        {
            if (!HasConfig || !PersistConfig || !CTDynamicModMenu.Instance.persistentSettings) return;
            if (Keybind != null)
            {
                CTDynamicModMenu.Instance.Config.Bind<KeyCode>("Command Settings", $"{Name}: Toggle Key", Keybind.Value, "Key to toggle the menu").Value = Keybind.Value;
            }
            if (IsToggle)
            {
                CTDynamicModMenu.Instance.Config.Bind<bool>("Command Settings", $"{Name}: IsEnabled", IsEnabled, "Whether the command is enabled").Value = IsEnabled;
            }
        } 
    }
}
