using BepInEx;
using HarmonyLib;
using InsanityRemastered.General;
using InsanityRemastered.Patches;
using InsanityRemastered.Utilities;
using System;
using UnityEngine;

namespace InsanityRemastered.Patches
{
    [HarmonyPatch]
    internal class HUDPatcher
    {
        private static bool hudOn;

        private static bool alreadyWarned;

        private static void ResetWarningFacility(bool outside)
        {
            if (outside)
            {
                alreadyWarned = false;
            }
        }

        private static void ResetWarning()
        {
        }

        [HarmonyPatch(typeof(HUDManager), nameof(HUDManager.Awake))]
        [HarmonyPostfix]
        private static void _Awake()
        {
            HallucinationManager.OnUIHallucination += PlayUISFX;
            GameEvents.OnEnterOrLeaveFacility += ResetWarningFacility;
            GameEvents.OnGameEnd += ResetWarning;
            GameEvents.OnPlayerDied += ResetWarning;
        }

        [HarmonyPatch(typeof(HUDManager), nameof(HUDManager.Update))]
        [HarmonyPostfix]
        private static void _Update()
        {
            if (UnityInput.Current.GetKeyDown((KeyCode)48))
            {
                ToggleHUD();
            }
            if (!alreadyWarned && PlayerPatcher.CurrentSanityLevel >= EnumInsanity.Medium)
            {
                HUDManager.Instance.DisplayTip("WARNING!", "Heartrate level is above normal. Please exercise caution.", true, false, "LC_Tip1");
                alreadyWarned = true;
            }
            if (!alreadyWarned && PlayerPatcher.CurrentSanityLevel >= EnumInsanity.High)
            {
                HUDManager.Instance.DisplayTip("WARNING!", "Heartrate is . Please exercise caution.", true, false, "LC_Tip1");
                alreadyWarned = true;
            }
        }

        private static void PlayUISFX()
        {
            InsanitySoundManager.Instance.PlayUISound(HUDManager.Instance.warningSFX[Random.Range(0, HUDManager.Instance.warningSFX.Length)]);
        }

        private static void ToggleHUD()
        {
            if (!hudOn)
            {
                HUDManager.Instance.HideHUD(true);
                hudOn = true;
            }
            else if (hudOn)
            {
                HUDManager.Instance.HideHUD(false);
                hudOn = false;
            }
        }
    }
}