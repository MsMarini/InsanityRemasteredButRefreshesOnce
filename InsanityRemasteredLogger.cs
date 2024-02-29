using BepInEx.Logging;
using InsanityRemastered.General;

namespace InsanityRemastered
{
    internal static class InsanityRemasteredLogger
    {
        internal static ManualLogSource logSource;

        public static bool UpdateLoggingEnabled {private set; get;}
        public static float logTimer;

        public static void Initialize(string modGUID)
        {
            logSource = Logger.CreateLogSource(modGUID);
            UpdateLoggingEnabled = InsanityRemasteredConfiguration.logDebugVariables; /// go to hallucinationmanager's Update to find where this is used
        }

        public static void Log(object message)
        {
            logSource.LogMessage(message);
        }

        public static void LogError(object message)
        {
            logSource.LogError(message);
        }

        public static void LogWarning(object message)
        {
            logSource.LogWarning(message);
        }

        public static void LogVariables(object[] variables)
        {
            foreach (var item in variables)
            {
                string value = (item != null) ? item.ToString() : "null";

                logSource.LogMessage($"{nameof(item)}: {value}");
            }
        }
    }
}