using HarmonyLib;
using InsanityRemastered.General;
using InsanityRemastered.ModIntegration;
using UnityEngine;

namespace InsanityRemastered.Patches
{
    [HarmonyPatch(typeof(FlashlightItem))]
    internal class ItemPatcher
    {
        private static float walkieRNGFrequency = 35f;
        private static float walkieRNGTimer;

        [HarmonyPatch(typeof(FlashlightItem), nameof(FlashlightItem.SwitchFlashlight))]
        [HarmonyPostfix]
        private static void OnUse(bool on)
        {
            PlayerPatcher.FlashlightOn = on;
        }

        [HarmonyPatch(typeof(GrabbableObject), nameof(GrabbableObject.EquipItem))]
        [HarmonyPostfix]
        private static void SetControlTipForPills(ref GrabbableObject __instance) // Is there a reason why this is separate from OnItemSwitch?
        {
            if (__instance.itemProperties.name == "PillBottle")
            {
                HUDManager.Instance.ChangeControlTip(1, "Consume pills: [LMB]");
            }
        }

        [HarmonyPatch(typeof(WalkieTalkie), nameof(WalkieTalkie.Update))]
        [HarmonyPostfix]
        private static void WalkieEffects(ref WalkieTalkie __instance)
        {
            if (!(GameNetworkManager.Instance.gameHasStarted && InsanityRemasteredConfiguration.skinwalkerWalkiesEnabled && PlayerPatcher.LocalPlayer.isInsideFactory))
            { return; }

            walkieRNGTimer += Time.deltaTime;
            if (walkieRNGTimer > walkieRNGFrequency && __instance.isBeingUsed)
            {
                walkieRNGTimer = 0f;
                if (SkinwalkerModIntegration.IsInstalled && InsanityGameManager.AreOtherPlayersConnected && Random.Range(0f, 1f) < 0.35f)
                {
                    __instance.thisAudio.PlayOneShot(SkinwalkerModIntegration.GetRandomClip());
                }
            }
        }
    }
}