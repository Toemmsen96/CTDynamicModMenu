namespace CTDynamicModMenu
{
public partial class CTDynamicModMenu
    {
        public void DisplayMessage(string message)
        {
            string formattedMessage = "<color=green>Info:</color> " + message;
            lastDisplayedMessage = formattedMessage;
            logMessages.Add(formattedMessage);
            
            // Keep only the 100 most recent messages
            if (logMessages.Count > MAX_LOG_MESSAGES)
            {
                logMessages.RemoveAt(0);
            }
            
            logger.LogInfo(message);
        }
        public void DisplayError(string message)
        {
            string formattedMessage = "<color=red>Error:</color> " + message;
            lastDisplayedMessage = formattedMessage;
            logMessages.Add(formattedMessage);
            
            // Keep only the 100 most recent messages
            if (logMessages.Count > MAX_LOG_MESSAGES)
            {
                logMessages.RemoveAt(0);
            }
            
            logger.LogError(message);
        }
    }
}