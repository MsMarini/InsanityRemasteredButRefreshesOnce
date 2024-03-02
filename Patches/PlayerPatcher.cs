using GameNetcodeStuff;
using HarmonyLib;
using InsanityRemastered.General;
using InsanityRemastered.Hallucinations;
using InsanityRemastered.ModIntegration;
using InsanityRemastered.Utilities;
using System;
using UnityEngine;

namespace InsanityRemastered.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerPatcher
    {
        public static InsanityLevel CurrentInsanityLevel;

        public static int PlayersConnected;

        public static PlayerControllerB LocalPlayer => GameNetworkManager.Instance.localPlayerController;

        public static bool FlashlightOn { get; set; }

        internal static bool lookingAtModelHallucination;

        private static bool holdingPills;

        public static event Action OnInteractWithFakeItem;

        [HarmonyPatch("Awake")] /// am i able to use a non-hardcoded string here?
        [HarmonyPostfix]
        private static void _Awake(ref PlayerControllerB __instance)
        {
            InsanityRemasteredAI.OnHallucinationEnded += LoseSanity;
            GameEvents.OnItemSwitch += OnItemSwitch;
        }

        [HarmonyPatch(typeof(PlayerControllerB), "Start")] //// woah there, i have no idea if this works
        [HarmonyPostfix]
        private static void _Start(PlayerControllerB __instance)
        {
            __instance.maxInsanityLevel = 100f;
        }

        [HarmonyPatch(typeof(PlayerControllerB), "SetPlayerSanityLevel")]
        [HarmonyPrefix]
        private static bool PlayerInsanityPatch()
        {
            if (GameNetworkManager.Instance.gameHasStarted) /// add check for IsPlayerDead?
            {
                // idk maybe start of day stuff
                if (StartOfRound.Instance.inShipPhase || !TimeOfDay.Instance.currentDayTimeStarted)
                {
                    LocalPlayer.insanityLevel = 0f;
                    return false;
                }

                if (PlayersConnected > 1)
                {
                    LocalPlayer.isPlayerAlone = !InsanityGameManager.Instance.IsNearOtherPlayers;
                }
                else
                {
                    LocalPlayer.isPlayerAlone = true;
                }

                // calculate sanity gain/loss
                // determine location, apply base sanity gain/loss
                if (LocalPlayer.isInsideFactory)
                {
                    LocalPlayer.insanitySpeedMultiplier = InsanityRemasteredConfiguration.sanityLossInsideFactory;
                }
                else if (LocalPlayer.isInHangarShipRoom)
                {
                    LocalPlayer.insanitySpeedMultiplier = -InsanityRemasteredConfiguration.sanityGainInsideShip;
                }
                else if (TimeOfDay.Instance.dayMode > DayMode.Noon)
                {
                    LocalPlayer.insanitySpeedMultiplier = InsanityRemasteredConfiguration.sanityLossDarkOutside;
                }
                else // not in facility, not in ship, daytime
                {
                    LocalPlayer.insanitySpeedMultiplier = -InsanityRemasteredConfiguration.sanityGainLightOutside;
                }

                if (LocalPlayer.insanitySpeedMultiplier > 0f)
                {
                    if (InsanityGameManager.Instance.IsNearLightSource)
                        LocalPlayer.insanitySpeedMultiplier -= InsanityRemasteredConfiguration.sanityGainLightProximity;

                    if (InsanityGameManager.Instance.IsHearingPlayersThroughWalkie && LocalPlayer.isPlayerAlone)
                        LocalPlayer.insanitySpeedMultiplier -= InsanityRemasteredConfiguration.sanityGainHearingWalkies;

                    if (FlashlightOn || AdvancedCompanyCompatibility.nightVision)
                        LocalPlayer.insanitySpeedMultiplier -= InsanityRemasteredConfiguration.sanityGainFlashlight;
                }

                if (InsanityGameManager.Instance.LightsOff)
                    LocalPlayer.insanitySpeedMultiplier += InsanityRemasteredConfiguration.sanityLossLightsOffEvent;

                if (lookingAtModelHallucination)
                    LocalPlayer.insanitySpeedMultiplier += InsanityRemasteredConfiguration.sanityLossLookingAtModelHallucination;

                if (HallucinationManager.Instance.PanicAttackLevel > 0.5f)
                    LocalPlayer.insanitySpeedMultiplier += InsanityRemasteredConfiguration.sanityLossPanicAttack;

                // final calculation
                if (LocalPlayer.insanitySpeedMultiplier < 0f) // insanity is being lost
                {
                    LocalPlayer.insanityLevel = Mathf.MoveTowards(LocalPlayer.insanityLevel, 0f, Time.deltaTime * -LocalPlayer.insanitySpeedMultiplier);
                    return false;
                }
                else if (LocalPlayer.insanityLevel > LocalPlayer.maxInsanityLevel) // insanity is over the max and being lost
                {
                    LocalPlayer.insanityLevel = Mathf.MoveTowards(LocalPlayer.insanityLevel, LocalPlayer.maxInsanityLevel, Time.deltaTime * 2f);
                    return false;

                    
                }
                else // insanity is being gained
                {
                    if (PlayersConnected == 1)
                        LocalPlayer.insanitySpeedMultiplier *= InsanityRemasteredConfiguration.insanitySoloScaling;
                    else if (LocalPlayer.isPlayerAlone)
                        LocalPlayer.insanitySpeedMultiplier *= Mathf.Pow(InsanityRemasteredConfiguration.insanityMaxPlayerAmountScaling, 0.125f);
                    else
                        LocalPlayer.insanitySpeedMultiplier *= Mathf.Pow(InsanityRemasteredConfiguration.insanityMaxPlayerAmountScaling, 0.125f) * InsanityRemasteredConfiguration.sanityLossNearPlayersReduction;

                    LocalPlayer.insanityLevel = Mathf.MoveTowards(LocalPlayer.insanityLevel, LocalPlayer.maxInsanityLevel, Time.deltaTime * LocalPlayer.insanitySpeedMultiplier);
                    return false;
                }
            }
            return false;
        }

        [HarmonyPatch(typeof(PlayerControllerB), "Update")]
        [HarmonyPostfix]
        private static void _Update()
        {
            PlayersConnected = StartOfRound.Instance.connectedPlayersAmount + 1;
            PlayersConnected = Math.Clamp(PlayersConnected, 1, InsanityRemasteredConfiguration.insanityMaxPlayerAmountScaling);
            
            if (GameNetworkManager.Instance.gameHasStarted && LocalPlayer.isPlayerControlled && !LocalPlayer.isPlayerDead)
            {
                UpdateStatusEffects();
                if (HallucinationManager.Instance.PanicAttackLevel > 0.9f)
                {
                    CurrentInsanityLevel = InsanityLevel.Max;
                }
                else if (LocalPlayer.insanityLevel > 90f)
                {
                    CurrentInsanityLevel = InsanityLevel.High;
                }
                else if (LocalPlayer.insanityLevel > 45f)
                {
                    CurrentInsanityLevel = InsanityLevel.Medium;
                }
                else
                {
                    CurrentInsanityLevel = InsanityLevel.Low;
                }
            }
        }

        private static void UpdateStatusEffects() /// is this compatible with other mods that affect movespeed, like advanced company? or is walk/crouch speed constant even with those?
        {
            if (HallucinationManager.slowness)
            {
                LocalPlayer.movementSpeed = Mathf.MoveTowards(LocalPlayer.movementSpeed, 2.3f, 5f * Time.deltaTime);
            }
            else if (!HallucinationManager.slowness && !LocalPlayer.isSprinting && !LocalPlayer.isCrouching)
            {
                LocalPlayer.movementSpeed = Mathf.MoveTowards(LocalPlayer.movementSpeed, 4.6f, 5f * Time.deltaTime);
            }
            if (HallucinationManager.reduceVision)
            {
                HUDManager.Instance.increaseHelmetCondensation = true;
            }
        }

        private static void OnItemSwitch(GrabbableObject item)
        {
            if (item.itemProperties.name == "PillBottle") ///add the other pill item
            {
                holdingPills = true;
            }
            else
            {
                holdingPills = false;
            }
        }

        private static void LoseSanity(bool touched)
        {
            if (touched)
            {
                LocalPlayer.insanityLevel += 15f;
                LocalPlayer.JumpToFearLevel(1f);
            }
        }

        private static void OnHeardHallucinationSound() /// what is this used for?
        {
            if (CurrentInsanityLevel >= InsanityLevel.Medium)
            {
                LocalPlayer.insanityLevel += 2.5f;
            }
        }

        [HarmonyPatch(typeof(PlayerControllerB), "ActivateItem_performed")]
        [HarmonyPostfix]
        private static void _UseItem(PlayerControllerB __instance)
        {
            if (holdingPills)
            {
                LocalPlayer.ItemSlots[LocalPlayer.currentItemSlot].DestroyObjectInHand(LocalPlayer);
                holdingPills = false;
                LocalPlayer.insanityLevel = 0f;
                HallucinationManager.Instance.AdjustPanic(true);
            }
        }

        [HarmonyPatch(typeof(PlayerControllerB), "Interact_performed")]
        [HarmonyPostfix]
        private static void InteractPatch(PlayerControllerB __instance)
        {
            Ray interactRay = new(LocalPlayer.gameplayCamera.transform.position, __instance.gameplayCamera.transform.forward);

            if (!Physics.Raycast(interactRay, out RaycastHit hit, __instance.grabDistance, 832) || hit.collider.gameObject.layer == 8 || !(hit.collider.tag == "PhysicsProp") || LocalPlayer.twoHanded || __instance.sinkingValue > 0.73f)
            {
                return;
            }

            // check if null reference
            if (hit.collider.transform.gameObject.GetComponent<FakeItem>())
            {
                //is there a reason for these 2 lines not being in the event?
                LocalPlayer.insanityLevel += 13f;
                LocalPlayer.JumpToFearLevel(0.4f, true);
                OnInteractWithFakeItem?.Invoke();
            }
        }
    }
}