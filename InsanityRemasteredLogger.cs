using BepInEx.Logging;
using InsanityRemastered.General;
using System;

namespace InsanityRemastered
{
    internal static class InsanityRemasteredLogger
    {
        internal static ManualLogSource logSource;

        public static float logTimer;

        public static void Initialize(string modGUID)
        {
            logSource = Logger.CreateLogSource(modGUID);
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

        public static void LogVariables(string[] variableNames, object[] variables)
        {
            if (variableNames.Length != variables.Length)
            {
                throw new ArgumentException("The number of variable names and variables must match.");
            }

            for (int i = 0; i < variables.Length; i++)
            {
                string value = (variables[i] != null) ? variables[i].ToString() : "null";

                logSource.LogMessage($"{variableNames[i]}: {value}");
            }
        }
    }
}