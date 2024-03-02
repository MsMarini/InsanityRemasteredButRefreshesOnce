using HarmonyLib;
using InsanityRemastered.General;
using System;
using System.Reflection;

namespace InsanityRemastered.ModIntegration
{
    public class ModIntegrator
    {
        public static void BeginIntegrations(Assembly assembly)
        {
            if (InsanityRemasteredConfiguration.useExperimentalSkinwalkerVersion)
            {
                SkinwalkerModIntegration.IsInstalled = true;
                InsanityRemasteredLogger.Log("Skinwalker mod installed, starting integration.");
                Harmony harmony = new("skinwalker");
                Type[] types = assembly.GetTypes();

                for (int i = 0; i < types.Length; i++)
                {
                    if (types[i].Name == "AudioAggregator")
                    {
                        MethodInfo test = types[i].GetMethod("AddAudioRecording", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
                        HarmonyMethod harmonyMethod = new(typeof(SkinwalkerModIntegration).GetMethod(nameof(SkinwalkerModIntegration.AddRecording)));
                        harmony.Patch(test, harmonyMethod);
                    }
                }
            }
            else if (assembly.FullName.StartsWith("SkinwalkerMod"))
            {
                SkinwalkerModIntegration.IsInstalled = true;
                InsanityRemasteredLogger.Log("Skinwalker mod installed, starting integration.");
                Harmony harmony = new("skinwalker");
                Type[] types = assembly.GetTypes();

                for (int i = 0; i < types.Length; i++)
                {
                    if (types[i].Name == "SkinwalkerModPersistent")
                    {
                        MethodInfo test = types[i].GetMethod("Update", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
                        HarmonyMethod harmonyMethod = new(typeof(SkinwalkerModIntegration).GetMethod(nameof(SkinwalkerModIntegration.UpdateClips)));
                        harmony.Patch(test, harmonyMethod);
                    }
                }
            }
            
            if (assembly.FullName.StartsWith("AdvancedCompany"))
            {
                InsanityRemasteredLogger.LogError("AdvancedCompany mod installed, starting integration.");

                Harmony harmony = new("AdvancedCompany"); /// was the old typo important?
                Type[] types = assembly.GetTypes();

                for (int i = 0; i < types.Length; i++)
                {
                    if (types[i].Name == "NightVision" && types[i].Namespace == "AdvancedCompany.Objects")
                    {
                        InsanityRemasteredLogger.Log("Vision Enhancer object found, starting method patching.");

                        MethodInfo useFlashlight = types[i].GetMethod("SwitchFlashlight", BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
                        MethodInfo unequip = types[i].GetMethod("Unequipped", BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);

                        HarmonyMethod nightVisionUse = new(typeof(AdvancedCompanyCompatibility).GetMethod(nameof(AdvancedCompanyCompatibility.HeadLightUtilityUse)));
                        HarmonyMethod unequipGoggles = new(typeof(AdvancedCompanyCompatibility).GetMethod(nameof(AdvancedCompanyCompatibility.UnequipHeadLightUtility)));

                        harmony.Patch(useFlashlight, nightVisionUse);
                        harmony.Patch(unequip, unequipGoggles);
                    }

                    if (types[i].Name == "HelmetLamp" && types[i].Namespace == "AdvancedCompany.Objects")
                    {
                        InsanityRemasteredLogger.Log("Helmet Lamp object found, starting fix.");

                        MethodInfo useHelmetLight = types[i].GetMethod("SwitchFlashlight", BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
                        MethodInfo unequip = types[i].GetMethod("Unequipped", BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);

                        HarmonyMethod helmetLampUse = new(typeof(AdvancedCompanyCompatibility).GetMethod(nameof(AdvancedCompanyCompatibility.HeadLightUtilityUse)));
                        HarmonyMethod unequipHelmetLamp = new(typeof(AdvancedCompanyCompatibility).GetMethod(nameof(AdvancedCompanyCompatibility.UnequipHeadLightUtility)));

                        harmony.Patch(useHelmetLight, helmetLampUse);
                        harmony.Patch(unequip, unequipHelmetLamp);
                    }

                    if (types[i].Name == "TacticalHelmet" && types[i].Namespace == "AdvancedCompany.Objects")
                    {
                        InsanityRemasteredLogger.Log("Tactical Helmet object found, starting fix.");

                        MethodInfo useHelmetLight = types[i].GetMethod("SwitchFlashlight", BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
                        MethodInfo unequip = types[i].GetMethod("Unequipped", BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);

                        HarmonyMethod helmetLampUse = new(typeof(AdvancedCompanyCompatibility).GetMethod(nameof(AdvancedCompanyCompatibility.HeadLightUtilityUse)));
                        HarmonyMethod unequipHelmetLamp = new(typeof(AdvancedCompanyCompatibility).GetMethod(nameof(AdvancedCompanyCompatibility.UnequipHeadLightUtility)));

                        harmony.Patch(useHelmetLight, helmetLampUse);
                        harmony.Patch(unequip, unequipHelmetLamp);
                    }
                }
            }

        }
    }
}