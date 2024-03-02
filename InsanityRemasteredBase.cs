using BepInEx;
using HarmonyLib;
using InsanityRemastered.General;
using InsanityRemastered.ModIntegration;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace InsanityRemastered
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class InsanityRemasteredBase : BaseUnityPlugin
    {
        public static InsanityRemasteredBase Instance;

        public const string modGUID = "Epicool.InsanityRemastered";
        public const string modName = "Insanity Remastered";
        public const string modVersion = "1.2.0";

        private readonly Harmony harmony = new(modGUID);

        internal static GameObject SanityModObject;

        private static string logFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads", "log.txt");

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }

            SceneManager.sceneLoaded += OnSceneLoaded;
            AppDomain.CurrentDomain.AssemblyLoad += CurrentDomain_AssemblyLoad; /// why is this not working?!?!

            InsanityRemasteredLogger.Initialize(modGUID);
            InsanityRemasteredConfiguration.Initialize(Config);
            InsanityRemasteredConfiguration.ValidateSettings();
            InsanityRemasteredContent.LoadContent();

            harmony.PatchAll();
        }

        private void CurrentDomain_AssemblyLoad(object sender, AssemblyLoadEventArgs args)
        {
        /*Log("CurrentDomain_AssemblyLoad has been called.\n" +
                "Sender: " + (sender != null ? sender.ToString() : "null") +
                "args: " + (args != null ? args.ToString() : "null") +
                "Loaded Assembly: " + (args.LoadedAssembly != null ? args.LoadedAssembly.ToString() : "null"));*/
            ModIntegrator.BeginIntegrations(args.LoadedAssembly);
        }

        private void SetupModManager()
        {
            GameObject sanityObject = new("Sanity Mod");
            sanityObject.AddComponent<InsanityGameManager>();
            sanityObject.AddComponent<InsanitySoundManager>();
            sanityObject.AddComponent<HallucinationManager>().enabled = false;
            SanityModObject = sanityObject;
            SanityModObject.hideFlags = HideFlags.HideAndDontSave;
        }

        private void OnSceneLoaded(Scene level, LoadSceneMode loadEnum)
        {
            if (level.name == SceneNames.SampleSceneRelay.ToString() && !SanityModObject.activeInHierarchy)
            {
                SanityModObject.SetActive(true);
            }
            if (level.name == SceneNames.MainMenu.ToString())
            {
                if (SanityModObject)
                {
                    InsanitySoundManager.Instance.StopModSounds();
                    SanityModObject.hideFlags = HideFlags.HideAndDontSave;
                }
                else
                {
                    SetupModManager();
                    SanityModObject.hideFlags = HideFlags.HideAndDontSave;
                }
            }
        }
        /// temporary
        public static void Log(string message)
        {
            using (StreamWriter writer = new(logFilePath, true))
            {
                writer.WriteLine(message);
            }
        }
    }
}
