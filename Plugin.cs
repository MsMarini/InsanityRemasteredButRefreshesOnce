/*
features to implement?:
proximity/chased/looking at monsters increases insanity

bug list:
Turned off breaker -> took apparatus -> Lights Off hallucination -> left facility -> reentered facility -> lights were on!
FlashlightOn method could probably be improved (different method of detection?)

IsHearingPlayersThroughWalkie may not be compatible with advanced company, and it may give sanity from hearing hallucinations from skinwalkers
*/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using Dissonance;
using DunGen.Graph;
using GameNetcodeStuff;
using HarmonyLib;
using InsanityRemastered.General;
using InsanityRemastered.Hallucinations;
using InsanityRemastered.ModIntegration;
using InsanityRemastered.Patches;
using InsanityRemastered.Utilities;
using InsanityRemasteredMod;
using InsanityRemasteredMod.General;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.PlayerLoop;
using UnityEngine.SceneManagement;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.Default | DebuggableAttribute.DebuggingModes.DisableOptimizations | DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints | DebuggableAttribute.DebuggingModes.EnableEditAndContinue)]
[assembly: AssemblyTitle("SanityRewrittenMod")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("SanityRewrittenMod")]
[assembly: AssemblyCopyright("Copyright ©  2023")]
[assembly: AssemblyTrademark("")]
[assembly: ComVisible(false)]
[assembly: Guid("023dcefc-0d1e-49d8-8d40-c74bd054370b")]
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: TargetFramework(".NETFramework,Version=v4.8", FrameworkDisplayName = ".NET Framework 4.8")]
[assembly: AssemblyVersion("1.0.0.0")]

public enum EnumInsanity
{
    Low,
    Medium,
    High,
    Max
}
public enum SceneNames
{
    InitSceneLaunchOptions,
    MainMenu,
    SampleSceneRelay
}
namespace InsanityRemastered
{
    [BepInPlugin("Epicool.InsanityRemastered", "Insanity Remastered", "1.1.3")]
    public class InsanityRemasteredBase : BaseUnityPlugin
    {
        public static InsanityRemasteredBase Instance;

        public const string modGUID = "Epicool.InsanityRemastered";
        public const string modName = "Insanity Remastered";
        public const string modVersion = "1.1.3";

        private readonly Harmony harmony = new(modGUID);

        internal static GameObject SanityModObject;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }

            SceneManager.sceneLoaded += OnSceneLoaded;
            AppDomain.CurrentDomain.AssemblyLoad += CurrentDomain_AssemblyLoad;

            InsanityRemasteredLogger.Initialize(modGUID);
            InsanityRemasteredConfiguration.Initialize(Config);
            InsanityRemasteredConfiguration.ValidateSettings();
            InsanityRemasteredContent.LoadContent();

            harmony.PatchAll();
        }

        private void CurrentDomain_AssemblyLoad(object sender, AssemblyLoadEventArgs args)
        {
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
                else// if (!SanityModObject)
                {
                    SetupModManager();
                    SanityModObject.hideFlags = HideFlags.HideAndDontSave;
                }
            }
        }
    }
    public enum HallucinationType
    {
        Staring,
        Wandering,
        Approaching
    }
    internal class AnimationID
    {
        public const string PlayerWalking = "Walking";

        public const string PlayerCrouching = "crouching";

        public const string SpiderMoving = "moving";

        public const string BrackenMoving = "sneak";
    }
}
namespace InsanityRemastered.Utilities
{
    [HarmonyPatch]
    internal class GameEvents
    {
        public static event Action<bool> OnEnterOrLeaveFacility;

        public static event Action OnGameStart;

        public static event Action OnLateGameStart;

        public static event Action OnGameEnd;

        public static event Action OnEnemySpawned;

        public static event Action OnShipLanded;

        public static event Action<GrabbableObject> OnItemSwitch;

        public static event Action OnPlayerJoin;

        public static event Action OnPlayerLeave;

        public static event Action OnPlayerDied;

        public static event Action<int> OnTakeDamage;

        public static event Action<EnemyAI> OnKillEnemy;

        [HarmonyPatch(typeof(EntranceTeleport), nameof(EntranceTeleport.TeleportPlayer))]
        [HarmonyPrefix]
        private static void OnEnterLeaveFacility(EntranceTeleport __instance)
        {
            OnEnterOrLeaveFacility?.Invoke(__instance.isEntranceToBuilding);
        }

        [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.GenerateNewLevelClientRpc))]
        [HarmonyPostfix]
        private static void GameStart()
        {
            OnGameStart?.Invoke();
        }

        [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.FinishGeneratingNewLevelClientRpc))]
        [HarmonyPostfix]
        private static void LateGameStart()
        {
            OnLateGameStart?.Invoke();
        }

        [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.DespawnPropsAtEndOfRound))]
        [HarmonyPostfix]
        private static void GameEnd()
        {
            OnGameEnd?.Invoke();
        }

        [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.OnShipLandedMiscEvents))]
        [HarmonyPostfix]
        private static void ShipLanded()
        {
            OnShipLanded?.Invoke();
        }

        [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.SpawnEnemyGameObject))]
        [HarmonyPostfix]
        private static void SpawnEnemy()
        {
            OnEnemySpawned?.Invoke();
        }

        [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.OnClientDisconnect))]
        [HarmonyPostfix]
        private static void PlayerLeave()
        {
            OnPlayerLeave?.Invoke();
        }

        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.SwitchToItemSlot))]
        [HarmonyPostfix]
        private static void SwitchItem(ref GrabbableObject ___currentlyHeldObjectServer)
        {
            if (___currentlyHeldObjectServer != null)
            {
                OnItemSwitch?.Invoke(___currentlyHeldObjectServer);
            }
        }

        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.KillPlayer))]
        [HarmonyPostfix]
        private static void OnPlayerDeath(ref PlayerControllerB __instance)
        {
            OnPlayerDied?.Invoke();
        }

        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.DamagePlayer))]
        [HarmonyPostfix]
        private static void TakeDamage(int damageNumber)
        {
            OnTakeDamage?.Invoke(damageNumber);
        }

        [HarmonyPatch(typeof(EnemyAI), nameof(EnemyAI.KillEnemy))]
        [HarmonyPostfix]
        private static void KillEnemy(EnemyAI __instance)
        {
            OnKillEnemy?.Invoke(__instance);
        }
    }
}
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
            if (on)
            {
                PlayerPatcher.FlashlightOn = true;
            }
            else
            {
                PlayerPatcher.FlashlightOn = false;
            }
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
            if ( !(GameNetworkManager.Instance.gameHasStarted && InsanityRemasteredConfiguration.skinwalkerWalkiesEnabled && PlayerPatcher.LocalPlayer.isInsideFactory) )
            { return; }

            walkieRNGTimer += Time.deltaTime;
            if (walkieRNGTimer > walkieRNGFrequency && __instance.isBeingUsed)
            {
                walkieRNGTimer = 0f;
                float num = UnityEngine.Random.Range(0f, 1f);
                if (num <= 0.35f && SkinwalkerModIntegration.IsInstalled && InsanityGameManager.AreThereOtherPlayers)
                {
                    __instance.thisAudio.PlayOneShot(SkinwalkerModIntegration.GetRandomClip());
                }
            }
        }
    }
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerPatcher
    {
        public static EnumInsanity CurrentSanityLevel;

        public static int PlayersConnected;

        public static float InsanityLevel;

        public static PlayerControllerB LocalPlayer => GameNetworkManager.Instance.localPlayerController;

        public static bool FlashlightOn { get; set; }

        internal static bool lookingAtModelHallucination;

        private static bool holdingPills;

        public static event Action OnInteractWithFakeItem;

        [HarmonyPatch("Awake")] // am i able to use a non-hardcoded string here?
        [HarmonyPostfix]
        private static void _Awake(ref PlayerControllerB __instance)
        {
            InsanityRemastered_AI.OnHallucinationEnded += LoseSanity;
            GameEvents.OnItemSwitch += OnItemSwitch;
        }

        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.SetPlayerSanityLevel))]
        [HarmonyPrefix]
        private static bool PlayerInsanityPatch()
        {
            if (GameNetworkManager.Instance.gameHasStarted)
            {
                // idk maybe start of day stuff
                InsanityLevel = LocalPlayer.insanityLevel;
                if (StartOfRound.Instance.inShipPhase || !TimeOfDay.Instance.currentDayTimeStarted)
                {
                    LocalPlayer.insanityLevel = 0f;
                    return false;
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

                if (PlayersConnected > 1)
                {
                    LocalPlayer.insanitySpeedMultiplier *= (float)Math.Pow(InsanityRemasteredConfiguration.insanityMaxPlayerAmountScaling, 0.125);
                    
                    if (InsanityGameManager.Instance.IsNearPlayers)
                        LocalPlayer.isPlayerAlone = false;
                    else
                        LocalPlayer.isPlayerAlone = true;

                }

                if (InsanityGameManager.Instance.IsNearLightSource)
                    LocalPlayer.insanitySpeedMultiplier -= InsanityRemasteredConfiguration.sanityGainLightProximity;

                if (InsanityGameManager.Instance.IsHearingPlayersThroughWalkie && LocalPlayer.isPlayerAlone)
                    LocalPlayer.insanitySpeedMultiplier -= InsanityRemasteredConfiguration.sanityGainHearingWalkies;

                if (FlashlightOn || AdvancedCompanyCompatibility.nightVision)
                    LocalPlayer.insanitySpeedMultiplier -= InsanityRemasteredConfiguration.sanityGainFlashlight;
                
                if (InsanityGameManager.Instance.LightsOff)
                    LocalPlayer.insanitySpeedMultiplier += InsanityRemasteredConfiguration.sanityLossLightsOffEvent;

                if (lookingAtModelHallucination)
                    LocalPlayer.insanitySpeedMultiplier += InsanityRemasteredConfiguration.sanityLossLookingAtModelHallucination;
                
                if (HallucinationManager.Instance.PanicAttackLevel > 0f)
                    LocalPlayer.insanitySpeedMultiplier += InsanityRemasteredConfiguration.sanityLossPanicAttack;

                // final calculation
                if (LocalPlayer.insanitySpeedMultiplier < 0f) // insanity is being gained
                {
                    LocalPlayer.insanityLevel = Mathf.MoveTowards(LocalPlayer.insanityLevel, 0f, Time.deltaTime * -LocalPlayer.insanitySpeedMultiplier);
                    return false;
                }
                else if (LocalPlayer.insanityLevel < LocalPlayer.maxInsanityLevel) // insanity is being lost
                {
                    if (!LocalPlayer.isPlayerAlone)
                        LocalPlayer.insanitySpeedMultiplier *= InsanityRemasteredConfiguration.sanityLossNearPlayersReduction;
                    else if (PlayersConnected == 1)
                        LocalPlayer.insanitySpeedMultiplier *= InsanityRemasteredConfiguration.insanitySoloScaling;
                    
                    LocalPlayer.insanityLevel = Mathf.MoveTowards(LocalPlayer.insanityLevel, LocalPlayer.maxInsanityLevel, Time.deltaTime * LocalPlayer.insanitySpeedMultiplier);
                    return false;
                }
                else // insanity is over the max
                {
                    LocalPlayer.insanityLevel = Mathf.MoveTowards(LocalPlayer.insanityLevel, LocalPlayer.maxInsanityLevel, Time.deltaTime * 2f);
                    return false;
                }
            }
            return false;
        }

        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.Update))]
        [HarmonyPostfix]
        private static void _Update()
        {
            PlayersConnected = StartOfRound.Instance.connectedPlayersAmount + 1;
            PlayersConnected = Mathf.Clamp(PlayersConnected, 1, InsanityRemasteredConfiguration.insanityMaxPlayerAmountScaling);
            if (GameNetworkManager.Instance.gameHasStarted && LocalPlayer.isPlayerControlled && !LocalPlayer.isPlayerDead)
            {
                UpdateStatusEffects();
                if (HallucinationManager.Instance.PanicAttackLevel >= 1f)
                {
                    CurrentSanityLevel = EnumInsanity.Max;
                }
                else if (LocalPlayer.insanityLevel >= 100f)
                {
                    CurrentSanityLevel = EnumInsanity.High;
                }
                else if (LocalPlayer.insanityLevel >= 50f)
                {
                    CurrentSanityLevel = EnumInsanity.Medium;
                }
                else
                {
                    CurrentSanityLevel = EnumInsanity.Low;
                }
            }
        }

        private static void UpdateStatusEffects() // is this compatible with other mods that affect movespeed, like advanced company? or is walk/crouch speed constant even with those?
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
                LocalPlayer.JumpToFearLevel(1f, true);
            }
        }

        private static void OnHeardHallucinationSound() // what is this used for?
        {
            if (CurrentSanityLevel >= EnumInsanity.Medium)
            {
                LocalPlayer.insanityLevel += 5f;
            }
        }

        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.ActivateItem_performed))]
        [HarmonyPostfix]
        private static void _UseItem(PlayerControllerB __instance)
        {
            if (holdingPills)
            {
                LocalPlayer.ItemSlots[LocalPlayer.currentItemSlot].DestroyObjectInHand(LocalPlayer);
                holdingPills = false;
                LocalPlayer.insanityLevel = 0f;
                SoundManager.Instance.SetDiageticMixerSnapshot(0, 1f);
            }
        }

        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.Interact_performed))]
        [HarmonyPostfix]
        private static void InteractPatch(PlayerControllerB __instance)
        {
            Ray interactRay = new(LocalPlayer.gameplayCamera.transform.position, __instance.gameplayCamera.transform.forward);

            if (!Physics.Raycast(interactRay, out RaycastHit hit, __instance.grabDistance, 832) || hit.collider.gameObject.layer == 8 || !(hit.collider.tag == "PhysicsProp") || LocalPlayer.twoHanded || __instance.sinkingValue > 0.73f)
            {
                return;
            }

            FakeItem fakeItem = hit.collider.transform.gameObject.GetComponent<FakeItem>();

            if (fakeItem)
            {
                LocalPlayer.insanityLevel += 13f;
                LocalPlayer.JumpToFearLevel(0.4f, true);
                OnInteractWithFakeItem?.Invoke();
            }
        }
    }
}
namespace InsanityRemastered.ModIntegration
{
    public class AdvancedCompanyCompatibility
    {
        public static bool nightVision;

        internal static void UnequipHeadLightUtility()
        {
            nightVision = false;
        }

        internal static void HeadLightUtilityUse(bool on)
        {
            if (on)
            {
                nightVision = true;
            }
            else
            {
                nightVision = false;
            }
        }
    }
    public class SkinwalkerModIntegration
    {
        private static List<AudioClip> skinwalkerClips = [];

        public static bool IsInstalled { get; set; }

        internal static void UpdateClips(ref List<AudioClip> ___cachedAudio)
        {
            skinwalkerClips = ___cachedAudio;
        }

        public static AudioClip GetRandomClip()
        {
            return skinwalkerClips[UnityEngine.Random.Range(0, skinwalkerClips.Count)];
        }

        internal static void AddRecording(object recording) // this might be super broken from the decompiler
        {
            skinwalkerClips.Add((AudioClip)recording.GetType().GetField("clip").GetValue(recording));
        }

        public static void ClearRecordings()
        {
            skinwalkerClips.Clear();
        }
    }
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
                        MethodInfo test = types[i].GetMethod(nameof(Update), BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
                        HarmonyMethod harmonyMethod = new(typeof(SkinwalkerModIntegration).GetMethod(nameof(SkinwalkerModIntegration.UpdateClips)));
                        harmony.Patch(test, harmonyMethod);
                    }
                }
            }

            if (assembly.FullName.StartsWith("AdvancedCompany"))
            {
                InsanityRemasteredLogger.Log("AdvancedCompany mod installed, starting integration.");
                
                Harmony harmony = new("advancecompany "); /// is this typo important?
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
namespace InsanityRemastered.Hallucinations
{
    internal class InsanityRemastered_AI : MonoBehaviour
    {
        protected float duration = 30f;
        private float agentStoppingDistance = 3f;
        private float durationTimer;
        private bool notSeenYet = true;
        protected bool wanderSpot = false;
        private bool setup = false;

        public HallucinationType hallucinationType;
        public HallucinationSpawnType hallucinationSpawnType = HallucinationSpawnType.NotLooking;

        protected AudioSource soundSource;
        protected AudioClip[] sound;

        protected GameObject[] aiNodes;
        protected Animator hallucinationAnimator;
        protected NavMeshAgent agent;
        protected PlayerControllerB LocalPlayer => GameNetworkManager.Instance.localPlayerController;
        
        public static event Action OnFailedToSpawn;
        public static event Action<bool> OnHallucinationEnded;

        public virtual void Start()
        {
            SetupVariables();
        }

        public virtual void Spawn()
        {
            LoadAINodes();
            Vector3 spawnPosition = FindSpawnPosition();

            if (spawnPosition != Vector3.zero || spawnPosition != null)
            {
                transform.position = spawnPosition;
                wanderSpot = false;
                notSeenYet = true;
                agent.enabled = true;
            }
            else
            {
                OnFailedToSpawn?.Invoke();
                PoolForLater();
            }
        }

        public virtual bool HasLineOfSightToPosition(Transform eye, Vector3 pos, float width = 45f, int range = 60, float proximityAwareness = -1f)
        {
            if (Vector3.Distance(eye.position, pos) < range && !Physics.Linecast(eye.position, pos, StartOfRound.Instance.collidersAndRoomMaskAndDefault))
            {
                Vector3 to = pos - eye.position;
                if (Vector3.Angle(eye.forward, to) < width || Vector3.Distance(transform.position, pos) < proximityAwareness)
                {
                    return true;
                }
            }
            return false;
        }

        public virtual void LookingAtHallucination() // currently unimplemented
        {
            
        }

        public virtual void LookAtHallucinationFirstTime()
        {
            notSeenYet = false;
            LocalPlayer.JumpToFearLevel(0.4f, true);
        }

        public virtual void FinishHallucination(bool touched)
        {
            if (touched)
            {
                float scareRNG = UnityEngine.Random.Range(0f, 1f);
                if (scareRNG < 0.4f)
                {
                    InsanitySoundManager.Instance.PlayJumpscare();
                }
                LocalPlayer.JumpToFearLevel(1f, true);
            }
            else
            {
                LocalPlayer.insanityLevel -= 5;
            }
            /*///
            possible implementation, if the checks are necessary
            else if (LocalPlayer.insanityLevel > 5)
            {
                LocalPlayer.insanityLevel -= 5;
            }
            else
            {
                LocalPlayer.insanityLevel = 0;
            }
            */
            OnHallucinationEnded?.Invoke(touched);
            PoolForLater();
        }

        public virtual void Wander()
        {
            //IL_0061: Unknown result type (might be due to invalid IL or missing references)
            //IL_006c: Unknown result type (might be due to invalid IL or missing references)
            //IL_0034: Unknown result type (might be due to invalid IL or missing references)
            //IL_0040: Unknown result type (might be due to invalid IL or missing references)
            //IL_0046: Unknown result type (might be due to invalid IL or missing references)
            //IL_0047: Unknown result type (might be due to invalid IL or missing references)
            if (!wanderSpot)
            {
                agent.SetDestination(RoundManager.Instance.GetRandomNavMeshPositionInRadius(aiNodes[Random.Range(0, aiNodes.Length)].transform.position, 12f, default(NavMeshHit)));
                wanderSpot = true;
            }
            if (Vector3.Distance(((Component)this).transform.position, agent.destination) <= agentStoppingDistance && wanderSpot)
            {
                PoolForLater();
                InsanityRemastered_AI.OnHallucinationEnded?.Invoke(obj: false);
            }
        }

        public virtual void TimerTick()
        {
            durationTimer += Time.deltaTime;
            if (durationTimer > duration)
            {
                durationTimer = 0f;
                FinishHallucination(touched: false);
            }
        }

        public virtual void ChasePlayer()
        {
            //IL_000e: Unknown result type (might be due to invalid IL or missing references)
            //IL_001e: Unknown result type (might be due to invalid IL or missing references)
            //IL_0052: Unknown result type (might be due to invalid IL or missing references)
            TimerTick();
            if (Vector3.Distance(((Component)this).transform.position, ((Component)LocalPlayer).transform.position) <= agentStoppingDistance)
            {
                FinishHallucination(touched: true);
            }
            agent.SetDestination(((Component)LocalPlayer).transform.position);
        }

        public virtual void PoolForLater()
        {
            //IL_0014: Unknown result type (might be due to invalid IL or missing references)
            ((Behaviour)agent).enabled = false;
            ((Component)this).transform.position = Vector3.zero;
            ((Component)this).gameObject.SetActive(false);
        }

        private void LoadAINodes()
        {
        }

        public virtual void Update()
        {
            //IL_000d: Unknown result type (might be due to invalid IL or missing references)
            if (LocalPlayer.HasLineOfSightToPosition(((Component)this).transform.position, 45f, 60, -1f))
            {
                if (notSeenYet)
                {
                    LookAtHallucinationFirstTime();
                }
                // currently unimplemented
                // LookingAtHallucination();
                PlayerPatcher.lookingAtModelHallucination = true;
            }
            else
            {
                PlayerPatcher.lookingAtModelHallucination = false;
            }
        }

        public virtual void SetupVariables()
        {
            if (!setup)
            {
                aiNodes = GameObject.FindGameObjectsWithTag("AINode");
                agent = ((Component)this).GetComponent<NavMeshAgent>();
                hallucinationAnimator = ((Component)this).GetComponentInChildren<Animator>();
                soundSource = ((Component)this).gameObject.AddComponent<AudioSource>();
                soundSource.spatialBlend = 1f;
                agent.angularSpeed = float.PositiveInfinity;
                agent.speed = 3f;
                agent.stoppingDistance = agentStoppingDistance;
                setup = true;
                agent.areaMask = StartOfRound.Instance.walkableSurfacesMask;
            }
        }

        private Vector3 FindSpawnPosition()
        {
            //IL_00b5: Unknown result type (might be due to invalid IL or missing references)
            //IL_00ba: Unknown result type (might be due to invalid IL or missing references)
            //IL_00bd: Unknown result type (might be due to invalid IL or missing references)
            //IL_002a: Unknown result type (might be due to invalid IL or missing references)
            //IL_003c: Unknown result type (might be due to invalid IL or missing references)
            //IL_0065: Unknown result type (might be due to invalid IL or missing references)
            //IL_0093: Unknown result type (might be due to invalid IL or missing references)
            //IL_0098: Unknown result type (might be due to invalid IL or missing references)
            if (hallucinationSpawnType == HallucinationSpawnType.NotLooking)
            {
                for (int i = 0; i < aiNodes.Length; i++)
                {
                    if (!Physics.Linecast(((Component)LocalPlayer.gameplayCamera).transform.position, aiNodes[i].transform.position, StartOfRound.Instance.collidersAndRoomMaskAndDefault) && !LocalPlayer.HasLineOfSightToPosition(aiNodes[i].transform.position, 45f, 20, 8f))
                    {
                        return aiNodes[i].transform.position;
                    }
                }
            }
            return Vector3.zero;
        }

        private void OnEnable()
        {
            if (!setup)
            {
                SetupVariables();
                setup = true;
            }
            else
            {
                Spawn();
            }
        }
    }
    public enum HallucinationSpawnType
    {
        NotLooking,
        Visible
    }
    internal class FakeItem : MonoBehaviour
    {
        private float stayTimer = 50f;

        private void Update()
        {
            stayTimer -= Time.deltaTime;
            if (stayTimer <= 0f)
            {
                ((Component)this).gameObject.SetActive(false);
            }
        }

        private void Interaction()
        {
            //IL_0021: Unknown result type (might be due to invalid IL or missing references)
            //IL_0027: Unknown result type (might be due to invalid IL or missing references)
            //IL_0048: Unknown result type (might be due to invalid IL or missing references)
            float num = Random.Range(0, 1);
            if (num <= 0.35f)
            {
                PlayerPatcher.LocalPlayer.DropBlood(default(Vector3), true, false);
            }
            else if (num <= 1E-05f)
            {
                PlayerPatcher.LocalPlayer.KillPlayer(Vector3.zero, true, (CauseOfDeath)0, 0);
            }
            InsanitySoundManager.Instance.PlayStinger();
            ((Component)this).gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            PlayerPatcher.OnInteractWithFakeItem += Interaction;
        }

        private void OnDisable()
        {
            PlayerPatcher.OnInteractWithFakeItem -= Interaction;
        }
    }
    internal class PlayerHallucination : InsanityRemastered_AI
    {
        private float stareTimer = 5f;

        private float waitTimeForNewWander = 5f;

        private float speakTimer = 3f;

        private float wanderTimer;

        private float stareDuration;

        private float speakDuration;

        private float rotationSpeed = 0.95f;

        private float footstepDistance = 1.5f;

        private int minWanderPoints = 3;

        private int maxWanderPoints = 5;

        private int currentFootstepSurfaceIndex;

        private List<Vector3> wanderPositions = new List<Vector3>();

        private Vector3 lastStepPosition;

        private AudioSource footstepSource;

        private bool spoken;

        private bool seenPlayer;

        public static SkinnedMeshRenderer suitRenderer;

        private void StopAndStare()
        {
            //IL_003f: Unknown result type (might be due to invalid IL or missing references)
            soundSource.Stop();
            seenPlayer = true;
            hallucinationAnimator.SetBool("Walking", false);
            agent.isStopped = true;
            Stare(((Component)base.LocalPlayer).transform.position);
            if (SkinwalkerModIntegration.IsInstalled && InsanityGameManager.AreThereOtherPlayers && !spoken)
            {
                soundSource.PlayOneShot(SkinwalkerModIntegration.GetRandomClip());
                spoken = true;
            }
            stareDuration += Time.deltaTime;
            if (stareDuration > stareTimer)
            {
                agent.isStopped = false;
                ((Behaviour)hallucinationAnimator).enabled = true;
                hallucinationAnimator.SetBool("Walking", true);
                hallucinationType = HallucinationType.Approaching;
            }
        }

        private void Stare(Vector3 position)
        {
            //IL_0001: Unknown result type (might be due to invalid IL or missing references)
            //IL_0008: Unknown result type (might be due to invalid IL or missing references)
            //IL_000d: Unknown result type (might be due to invalid IL or missing references)
            //IL_0012: Unknown result type (might be due to invalid IL or missing references)
            //IL_0017: Unknown result type (might be due to invalid IL or missing references)
            //IL_0024: Unknown result type (might be due to invalid IL or missing references)
            //IL_0029: Unknown result type (might be due to invalid IL or missing references)
            //IL_0036: Unknown result type (might be due to invalid IL or missing references)
            Quaternion val = Quaternion.LookRotation(position - ((Component)this).transform.position);
            ((Component)this).transform.rotation = Quaternion.Slerp(((Component)this).transform.rotation, val, rotationSpeed * Time.deltaTime);
        }

        private void GenerateNewDestination()
        {
            //IL_002a: Unknown result type (might be due to invalid IL or missing references)
            //IL_002f: Unknown result type (might be due to invalid IL or missing references)
            //IL_0036: Unknown result type (might be due to invalid IL or missing references)
            //IL_0043: Unknown result type (might be due to invalid IL or missing references)
            hallucinationAnimator.SetBool("Walking", true);
            Vector3 val = wanderPositions[Random.Range(0, wanderPositions.Count)];
            wanderPositions.Remove(val);
            agent.SetDestination(val);
            agent.isStopped = false;
            wanderSpot = true;
        }

        private void ReachDestination()
        {
            if (wanderPositions.Count == 0)
            {
                FinishHallucination(touched: false);
            }
            hallucinationAnimator.SetBool("Walking", false);
            agent.isStopped = true;
            wanderTimer += Time.deltaTime;
            if (wanderTimer > waitTimeForNewWander)
            {
                wanderTimer = 0f;
                wanderSpot = false;
            }
        }

        private void GenerateWanderPoints()
        {
            //IL_0049: Unknown result type (might be due to invalid IL or missing references)
            //IL_0055: Unknown result type (might be due to invalid IL or missing references)
            //IL_005b: Unknown result type (might be due to invalid IL or missing references)
            //IL_005c: Unknown result type (might be due to invalid IL or missing references)
            if (wanderPositions.Count > 0)
            {
                wanderPositions.Clear();
            }
            int num = Random.Range(minWanderPoints, maxWanderPoints);
            for (int i = 0; i < num; i++)
            {
                wanderPositions.Add(RoundManager.Instance.GetRandomNavMeshPositionInRadius(((Component)this).transform.position, 20f, default(NavMeshHit)));
            }
        }

        private void PlayFootstepSound()
        {
            GetCurrentMaterialStandingOn();
            int num = Random.Range(0, StartOfRound.Instance.footstepSurfaces[currentFootstepSurfaceIndex].clips.Length);
            footstepSource.pitch = Random.Range(0.93f, 1.07f);
            footstepSource.PlayOneShot(StartOfRound.Instance.footstepSurfaces[currentFootstepSurfaceIndex].clips[num], 5.5f);
        }

        private void GetCurrentMaterialStandingOn()
        {
            //IL_0009: Unknown result type (might be due to invalid IL or missing references)
            //IL_000e: Unknown result type (might be due to invalid IL or missing references)
            //IL_0013: Unknown result type (might be due to invalid IL or missing references)
            //IL_0018: Unknown result type (might be due to invalid IL or missing references)
            //IL_001d: Unknown result type (might be due to invalid IL or missing references)
            //IL_0027: Unknown result type (might be due to invalid IL or missing references)
            Ray val = default(Ray);
            ((Ray)(ref val))..ctor(((Component)this).transform.position + Vector3.up, -Vector3.up);
            RaycastHit val2 = default(RaycastHit);
            if (!Physics.Raycast(val, ref val2, 6f, StartOfRound.Instance.walkableSurfacesMask, (QueryTriggerInteraction)1) || ((Component)((RaycastHit)(ref val2)).collider).CompareTag(StartOfRound.Instance.footstepSurfaces[currentFootstepSurfaceIndex].surfaceTag))
            {
                return;
            }
            for (int i = 0; i < StartOfRound.Instance.footstepSurfaces.Length; i++)
            {
                if (((Component)((RaycastHit)(ref val2)).collider).CompareTag(StartOfRound.Instance.footstepSurfaces[i].surfaceTag))
                {
                    currentFootstepSurfaceIndex = i;
                    break;
                }
            }
        }

        private void Footstep()
        {
            //IL_0007: Unknown result type (might be due to invalid IL or missing references)
            //IL_000d: Unknown result type (might be due to invalid IL or missing references)
            //IL_002b: Unknown result type (might be due to invalid IL or missing references)
            //IL_0030: Unknown result type (might be due to invalid IL or missing references)
            if (Vector3.Distance(((Component)this).transform.position, lastStepPosition) > footstepDistance)
            {
                lastStepPosition = ((Component)this).transform.position;
                PlayFootstepSound();
            }
        }

        private int GetRandomPlayerSuitID()
        {
            PlayerControllerB val = StartOfRound.Instance.allPlayerScripts[Random.Range(0, StartOfRound.Instance.allPlayerScripts.Length)];
            if (val.isPlayerControlled)
            {
                return val.currentSuitID;
            }
            GetRandomPlayerSuitID();
            return base.LocalPlayer.currentSuitID;
        }

        private void SetSuit(int id)
        {
            Material suitMaterial = StartOfRound.Instance.unlockablesList.unlockables[id].suitMaterial;
            ((Renderer)suitRenderer).material = suitMaterial;
        }

        public override void Start()
        {
            base.Start();
            base.sound = InsanityRemasteredContent.PlayerHallucinationSounds;
            hallucinationAnimator.runtimeAnimatorController = StartOfRound.Instance.localClientAnimatorController;
            footstepSource = ((Component)this).gameObject.AddComponent<AudioSource>();
            footstepSource.spatialBlend = 1f;
        }

        public override void Update()
        {
            //IL_0028: Unknown result type (might be due to invalid IL or missing references)
            //IL_0046: Unknown result type (might be due to invalid IL or missing references)
            //IL_0056: Unknown result type (might be due to invalid IL or missing references)
            //IL_00bb: Unknown result type (might be due to invalid IL or missing references)
            base.Update();
            if (hallucinationType == HallucinationType.Wandering)
            {
                if (HasLineOfSightToPosition(((Component)this).transform, ((Component)base.LocalPlayer).transform.position, 45f, 45) || Vector3.Distance(((Component)this).transform.position, ((Component)base.LocalPlayer).transform.position) < 3f || seenPlayer)
                {
                    StopAndStare();
                }
                else
                {
                    Wander();
                }
            }
            else if (hallucinationType == HallucinationType.Approaching)
            {
                ChasePlayer();
            }
            if (hallucinationType == HallucinationType.Staring)
            {
                Stare(((Component)base.LocalPlayer).transform.position);
                TimerTick();
            }
            Footstep();
        }

        public override void LookAtHallucinationFirstTime()
        {
            base.LookAtHallucinationFirstTime();
        }

        public override void FinishHallucination(bool touched)
        {
            //IL_003b: Unknown result type (might be due to invalid IL or missing references)
            //IL_0041: Unknown result type (might be due to invalid IL or missing references)
            if (touched)
            {
                float num = Random.Range(0f, 1f);
                if (num <= 0.15f)
                {
                    base.LocalPlayer.DamagePlayer(Random.Range(1, 5), false, true, (CauseOfDeath)6, 0, false, default(Vector3));
                    return;
                }
                if (num <= 0.45f)
                {
                    HallucinationManager.Instance.PanicAttack = true;
                }
                base.FinishHallucination(touched: true);
            }
            else
            {
                base.FinishHallucination(touched: false);
            }
        }

        public override void Wander()
        {
            //IL_002d: Unknown result type (might be due to invalid IL or missing references)
            //IL_0038: Unknown result type (might be due to invalid IL or missing references)
            if (wanderPositions.Count != 0 && !wanderSpot)
            {
                GenerateNewDestination();
            }
            if (Vector3.Distance(((Component)this).transform.position, agent.destination) <= agent.stoppingDistance && wanderSpot)
            {
                ReachDestination();
            }
        }

        public override void Spawn()
        {
            base.Spawn();
            seenPlayer = false;
            SetSuit(GetRandomPlayerSuitID());
            hallucinationType = HallucinationType.Staring;
            if (PlayerPatcher.CurrentSanityLevel >= EnumInsanity.Medium)
            {
                GenerateWanderPoints();
                hallucinationType = HallucinationType.Wandering;
            }
            float num = Random.Range(0f, 1f);
            float num2 = Random.Range(0f, 1f);
            if (num >= 0.5f && SkinwalkerModIntegration.IsInstalled && InsanityGameManager.AreThereOtherPlayers)
            {
                soundSource.clip = SkinwalkerModIntegration.GetRandomClip();
            }
            else
            {
                soundSource.clip = InsanitySoundManager.Instance.LoadFakePlayerSound();
            }
            if (num2 > 0.25f)
            {
                HUDManager.Instance.DisplayTip("", InsanityRemasteredConfiguration.hallucinationTipTexts[0], true, false, "LC_Tip1");
            }
            spoken = false;
            soundSource.Play();
            hallucinationAnimator.SetBool("Walking", false);
            stareDuration = 0f;
        }

        public override void SetupVariables()
        {
            //IL_0052: Unknown result type (might be due to invalid IL or missing references)
            //IL_0057: Unknown result type (might be due to invalid IL or missing references)
            base.SetupVariables();
            agent.obstacleAvoidanceType = (ObstacleAvoidanceType)0;
            suitRenderer = ((Component)this).GetComponentInChildren<SkinnedMeshRenderer>(false);
            base.duration = 30f;
            hallucinationAnimator.runtimeAnimatorController = StartOfRound.Instance.localClientAnimatorController;
            hallucinationSpawnType = HallucinationSpawnType.NotLooking;
            lastStepPosition = ((Component)this).transform.position;
        }
    }
}
namespace InsanityRemastered.General
{
    internal class HallucinationManager : MonoBehaviour
    {
        public static HallucinationManager Instance;

        private float droneRNGTimer;

        private float droneRNGFrequency = 60f;

        private float sanityRNGTimer;

        private float sanityRNGFrequency = 40f;

        private float slowdownTimer = 120f;

        private float panicAttackLevel;

        private bool panicAttack = false;

        public static bool slowness;

        public static bool reduceVision;

        private readonly Dictionary<string, EnumInsanity> hallucinations = new Dictionary<string, EnumInsanity>
        {
            {
                "AuditoryHallucination",
                EnumInsanity.Low
            },
            {
                "CrypticStatus",
                EnumInsanity.Low
            },
            {
                "CrypticMessage",
                EnumInsanity.Low
            },
            {
                "Fake Player",
                EnumInsanity.Low
            },
            {
                "Fake Item",
                EnumInsanity.Medium
            },
            {
                "Power loss",
                EnumInsanity.High
            }
        };

        private PlayerControllerB localPlayer => GameNetworkManager.Instance.localPlayerController;

        private EnumInsanity SanityLevel => PlayerPatcher.CurrentSanityLevel;

        public bool PanicAttack
        {
            get
            {
                return panicAttack;
            }
            set
            {
                panicAttack = value;
            }
        }

        public float PanicAttackLevel
        {
            get
            {
                return panicAttackLevel;
            }
            set
            {
                panicAttackLevel = value;
            }
        }

        public float EffectTransition => slowdownTimer;

        public Dictionary<string, EnumInsanity> Hallucinations => hallucinations;

        public static event Action<bool> OnPowerHallucination;

        public static event Action OnPlayerHallucinationStarted;

        public static event Action OnSpawnFakeItem;

        public static event Action OnSoundPlayed;

        public static event Action OnSanityRecovered;

        public static event Action OnExperiencePanicAttack;

        public static event Action OnUIHallucination;

        private void Start()
        {
            sanityRNGFrequency *= InsanityRemasteredConfiguration.rngCheckTimerMultiplier;
        }

        private void Awake()
        {
            Instance = this;
        }

        private void Update()
        {
            if (localPlayer.isPlayerControlled && !localPlayer.isPlayerDead && localPlayer.isInsideFactory && localPlayer.isPlayerControlled && !localPlayer.isPlayerDead)
            {
                sanityRNGTimer += Time.deltaTime;
                if (sanityRNGTimer > sanityRNGFrequency)
                {
                    sanityRNGTimer = 0f;
                    float num = Random.Range(0f, 1f);
                    if (num <= 0.45f)
                    {
                        Hallucinate(GetRandomHallucination());
                    }
                }
                if (localPlayer.insanityLevel == localPlayer.maxInsanityLevel)
                {
                    droneRNGTimer += Time.deltaTime;
                    if (droneRNGTimer > droneRNGFrequency)
                    {
                        droneRNGTimer = 0f;
                        float num2 = Random.Range(0f, 1f);
                        if (num2 <= 0.45f)
                        {
                            InsanitySoundManager.Instance.PlayDrone();
                            if (panicAttackLevel == 1f && InsanityRemasteredConfiguration.panicAttacksEnabled)
                            {
                                PanicAttackSymptom();
                            }
                        }
                    }
                    if (localPlayer.isInsideFactory && localPlayer.isPlayerAlone && !InsanityGameManager.Instance.IsNearLightSource)
                    {
                        HighSanityEffects();
                    }
                    else
                    {
                        LessenPanicEffects();
                    }
                }
            }
            if (panicAttackLevel >= 0f)
            {
                LessenPanicEffects();
            }
            if (!GameNetworkManager.Instance.gameHasStarted)
            {
                ResetPanicValues();
            }
        }

        private void LessenPanicEffects()
        {
            if (PanicAttackLevel <= 0f)
            {
                ResetPanicValues();
            }
            else if (GameNetworkManager.Instance.gameHasStarted && ((!localPlayer.isInsideFactory && panicAttackLevel >= 0f) || (localPlayer.isInsideFactory && !localPlayer.isPlayerAlone && panicAttackLevel >= 0f)))
            {
                panicAttackLevel = Mathf.MoveTowards(panicAttackLevel, 0f, 0.5f * Time.deltaTime);
                HallucinationEffects.LessenPanicEffects();
            }
        }

        private void HighSanityEffects()
        {
            panicAttackLevel = Mathf.MoveTowards(panicAttackLevel, 1f, slowdownTimer * Time.deltaTime);
            HallucinationEffects.IncreasePanicEffects();
        }

        public void ResetPanicValues()
        {
            SoundManager.Instance.SetDiageticMixerSnapshot(0, 1f);
            panicAttack = false;
            slowness = false;
            reduceVision = false;
            HallucinationManager.OnSanityRecovered?.Invoke();
        }

        public void PanicAttackSymptom()
        {
            //IL_0069: Unknown result type (might be due to invalid IL or missing references)
            panicAttack = true;
            localPlayer.JumpToFearLevel(0.55f, true);
            if (InsanityRemasteredConfiguration.panicAttackDebuffsEnabled)
            {
                switch (Random.Range(0, 2))
                {
                    case 0:
                        slowness = true;
                        break;
                    case 1:
                        reduceVision = true;
                        break;
                    case 2:
                        if (InsanityRemasteredConfiguration.panicAttackDeathsEnabled)
                        {
                            localPlayer.KillPlayer(Vector3.zero, true, (CauseOfDeath)0, 0);
                        }
                        else
                        {
                            slowness = true;
                        }
                        break;
                }
            }
            HUDManager.Instance.DisplayTip("WARNING!", "Heartrate is at dangerous levels. Please seek help immediately.", true, false, "LC_Tip1");
            HallucinationManager.OnExperiencePanicAttack?.Invoke();
        }

        public void Hallucinate(string id)
        {
            InsanityRemasteredLogger.Log("Performing hallucination with ID: " + id);
            float num = Random.Range(0f, 1f);
            switch (id)
            {
                case "CrypticStatus":
                    UpdateStatusEffect();
                    break;
                case "CrypticMessage":
                    ShowHallucinationTip();
                    break;
                case "Fake Player":
                    PlayerModelHallucination(InsanityGameManager.Instance.currentHallucinationModel);
                    break;
                case "AuditoryHallucination":
                    PlaySound();
                    break;
                case "Fake Item":
                    SpawnFakeObject();
                    break;
                case "Power loss":
                    LightHallucination();
                    break;
            }
            if (num <= 0.15f && hallucinations[id] >= EnumInsanity.Medium)
            {
                Hallucinate(GetRandomHallucination());
            }
        }

        public string GetRandomHallucination()
        {
            KeyValuePair<string, EnumInsanity> keyValuePair = hallucinations.ElementAt(Random.Range(0, hallucinations.Count()));
            if (keyValuePair.Value <= SanityLevel)
            {
                return keyValuePair.Key;
            }
            GetRandomHallucination();
            return "AuditoryHallucination";
        }

        private void UpdateStatusEffect()
        {
            if (InsanityRemasteredConfiguration.messageHallucinationsEnabled)
            {
                string text = InsanityRemasteredConfiguration.statusEffectTexts[Random.Range(0, InsanityRemasteredConfiguration.statusEffectTexts.Count)];
                HUDManager.Instance.DisplayStatusEffect(text);
                HallucinationManager.OnUIHallucination?.Invoke();
            }
        }

        private void LightHallucination()
        {
            if (!InsanityRemasteredConfiguration.lightsOffEventEnabled)
            {
                return;
            }
            if (!InsanityGameManager.Instance.LightsOff)
            {
                foreach (Animator allPoweredLightsAnimator in RoundManager.Instance.allPoweredLightsAnimators)
                {
                    allPoweredLightsAnimator.SetBool("on", false);
                }
                PlayerPatcher.LocalPlayer.JumpToFearLevel(0.3f, true);
                HallucinationManager.OnPowerHallucination?.Invoke(obj: false);
            }
            else
            {
                if (!InsanityGameManager.Instance.LightsOff)
                {
                    return;
                }
                foreach (Animator allPoweredLightsAnimator2 in RoundManager.Instance.allPoweredLightsAnimators)
                {
                    allPoweredLightsAnimator2.SetBool("on", true);
                }
                HallucinationManager.OnPowerHallucination?.Invoke(obj: true);
            }
        }

        private void ShowHallucinationTip()
        {
            if (!InsanityRemasteredConfiguration.messageHallucinationsEnabled)
            {
                return;
            }
            string text = InsanityRemasteredConfiguration.hallucinationTipTexts[Random.Range(0, InsanityRemasteredConfiguration.hallucinationTipTexts.Count)];
            if (text == InsanityRemasteredConfiguration.hallucinationTipTexts[1])
            {
                float num = Random.Range(0f, 1f);
                if (num <= 0.35f)
                {
                    ((MonoBehaviour)this).Invoke("LightHallucination", 3f);
                }
            }
            HallucinationManager.OnUIHallucination?.Invoke();
            HUDManager.Instance.DisplayTip("", text, true, false, "LC_Tip1");
        }

        private void PlayerModelHallucination(GameObject model)
        {
            if (InsanityRemasteredConfiguration.modelHallucinationsEnabled)
            {
                if (!model.activeInHierarchy)
                {
                    model.SetActive(true);
                }
                HallucinationManager.OnPlayerHallucinationStarted?.Invoke();
            }
        }

        private void PlaySound()
        {
            if (InsanityRemasteredConfiguration.auditoryHallucinationsEnabled)
            {
                InsanitySoundManager.Instance.PlayHallucinationSound();
            }
        }

        private void SpawnFakeObject()
        {
            //IL_004c: Unknown result type (might be due to invalid IL or missing references)
            //IL_0058: Unknown result type (might be due to invalid IL or missing references)
            //IL_005e: Unknown result type (might be due to invalid IL or missing references)
            //IL_0060: Unknown result type (might be due to invalid IL or missing references)
            //IL_0065: Unknown result type (might be due to invalid IL or missing references)
            //IL_0075: Unknown result type (might be due to invalid IL or missing references)
            //IL_007a: Unknown result type (might be due to invalid IL or missing references)
            //IL_007f: Unknown result type (might be due to invalid IL or missing references)
            //IL_008b: Unknown result type (might be due to invalid IL or missing references)
            //IL_008c: Unknown result type (might be due to invalid IL or missing references)
            if (InsanityRemasteredConfiguration.itemHallucinationsEnabled)
            {
                SpawnableItemWithRarity val = RoundManager.Instance.currentLevel.spawnableScrap[Random.Range(0, RoundManager.Instance.currentLevel.spawnableScrap.Count)];
                Vector3 val2 = RoundManager.Instance.GetRandomNavMeshPositionInRadiusSpherical(((Component)PlayerPatcher.LocalPlayer).transform.position, 10f, default(NavMeshHit)) + Vector3.up * val.spawnableItem.verticalOffset;
                GameObject val3 = Object.Instantiate<GameObject>(val.spawnableItem.spawnPrefab, val2, Quaternion.identity);
                GrabbableObject component = val3.GetComponent<GrabbableObject>();
                component.SetScrapValue(Random.Range(val.spawnableItem.minValue, val.spawnableItem.maxValue + 50));
                val3.AddComponent<FakeItem>();
                HallucinationManager.OnSpawnFakeItem?.Invoke();
            }
        }
    }
    public class HallucinationID
    {
        public const string Observer = "Observer";

        public const string CrypticStatusEffect = "CrypticStatus";

        public const string Auditory = "AuditoryHallucination";

        public const string CrypticMessage = "CrypticMessage";

        public const string FakeItem = "Fake Item";

        public const string FakePlayer = "Fake Player";

        public const string PowerLoss = "Power loss";
    }
    internal class InsanityGameManager : MonoBehaviour
    {
        public static InsanityGameManager Instance;

        private List<Light> bunkerLights = new List<Light>();

        public GameObject currentHallucinationModel;

        private float deletionTimer;

        private float deletionFrequency = 10f;

        private PlayerControllerB LocalPlayer => GameNetworkManager.Instance.localPlayerController;

        public static DungeonFlow MapFlow => RoundManager.Instance.dungeonGenerator.Generator.DungeonFlow;

        public static bool AreThereOtherPlayers => StartOfRound.Instance.connectedPlayersAmount > 0;

        public bool IsNearPlayers => NearOtherPlayers();

        public bool IsNearLightSource => NearLightSource();

        public bool IsHearingPlayersThroughWalkie => PlayerIsHearingOthersThroughWalkieTalkie();

        public bool IsTalking => PlayerTalking();

        public bool LightsOff { get; private set; }

        public List<Light> BunkerLights => bunkerLights;

        private void Awake()
        {
            if ((Object)(object)Instance == (Object)null)
            {
                Instance = this;
            }
            GameEvents.OnGameEnd += OnRoundEnd;
            GameEvents.OnShipLanded += GameEvents_OnShipLanded;
            GameEvents.OnPlayerDied += GameEvents_OnPlayerDied;
            GameEvents.OnEnterOrLeaveFacility += OnEnterOrLeaveFacility;
            HallucinationManager.OnPowerHallucination += PowerHallucination;
            SceneManager.sceneLoaded += SceneLoaded;
        }

        private void GameEvents_OnShipLanded()
        {
            CacheLights();
        }

        private void Update()
        {
            if (InsanityRemasteredConfiguration.useExperimentalSkinwalkerVersion && SkinwalkerModIntegration.IsInstalled)
            {
                deletionTimer += Time.deltaTime;
                if (deletionTimer > deletionFrequency)
                {
                    deletionTimer = 0f;
                    SkinwalkerModIntegration.ClearRecordings();
                }
            }
            if (GameNetworkManager.Instance.gameHasStarted && RoundManager.Instance.powerOffPermanently)
            {
                LightsOff = true;
            }
        }

        private void PowerHallucination(bool on)
        {
            if (on)
            {
                LightsOff = false;
            }
            else if (!on)
            {
                LightsOff = true;
            }
        }

        private void SceneLoaded(Scene scene, LoadSceneMode arg1)
        {
            if (((Scene)(ref scene)).name == SceneNames.SampleSceneRelay.ToString())
            {
                SavePlayerModel();
                ((Behaviour)HallucinationManager.Instance).enabled = true;
            }
            else if (((Scene)(ref scene)).name == SceneNames.MainMenu.ToString() || ((Scene)(ref scene)).name == SceneNames.InitSceneLaunchOptions.ToString())
            {
                ((Behaviour)HallucinationManager.Instance).enabled = false;
            }
        }

        private void GameEvents_OnPlayerDied()
        {
            InsanitySoundManager.Instance.StopModSounds();
            HallucinationManager.Instance.ResetPanicValues();
        }

        private void OnRoundEnd()
        {
            currentHallucinationModel.SetActive(false);
            LocalPlayer.insanityLevel = 0f;
        }

        private void OnEnterOrLeaveFacility(bool outside)
        {
            if (outside && Instance.LightsOff)
            {
                ResetLights();
            }
        }

        public void ResetLights()
        {
            if (LightsOff)
            {
                HallucinationManager.Instance.Hallucinate("Power loss");
            }
        }

        private bool NearOtherPlayers(PlayerControllerB playerScript = null, float checkRadius = 16f)
        {
            if (playerScript == null)
            {
                playerScript = LocalPlayer;
            }
            LocalPlayer.gameObject.layer = 0;
            bool result = Physics.CheckSphere(playerScript.transform.position, checkRadius, 8, QueryTriggerInteraction.Ignore);
            LocalPlayer.gameObject.layer = 3;
            return result;
        }

        private bool PlayerIsHearingOthersThroughWalkieTalkie(PlayerControllerB playerScript = null)
        {
        if (playerScript == null)
        {
            playerScript = LocalPlayer;
        }
        if (!playerScript.holdingWalkieTalkie)
        {
            return false;
        }
        for (int i = 0; i < WalkieTalkie.allWalkieTalkies.Count; i++)
        {
            if (WalkieTalkie.allWalkieTalkies[i].clientIsHoldingAndSpeakingIntoThis && WalkieTalkie.allWalkieTalkies[i] != playerScript.currentlyHeldObjectServer as WalkieTalkie)
            {
                return true;
            }
        }
        return false;
        }

        private bool PlayerTalking()
        {
            VoicePlayerState val = StartOfRound.Instance.voiceChatModule.FindPlayer(StartOfRound.Instance.voiceChatModule.LocalPlayerName);
            float num = Mathf.Clamp(val.Amplitude, 0f, 1f);
            return val.IsSpeaking && num > 0.85f;
        }

        private bool NearLightSource(float checkRadius = 10f)
        {
            //IL_001b: Unknown result type (might be due to invalid IL or missing references)
            //IL_002b: Unknown result type (might be due to invalid IL or missing references)
            for (int i = 0; i < RoundManager.Instance.allPoweredLights.Count; i++)
            {
                float num = Vector3.Distance(((Component)RoundManager.Instance.allPoweredLights[i]).transform.position, ((Component)LocalPlayer).transform.position);
                if (num < checkRadius && RoundManager.Instance.allPoweredLightsAnimators[i].GetBool("on"))
                {
                    return true;
                }
            }
            return false;
        }

        private void SavePlayerModel()
        {
            //IL_0029: Unknown result type (might be due to invalid IL or missing references)
            //IL_002f: Expected O, but got Unknown
            //IL_00be: Unknown result type (might be due to invalid IL or missing references)
            //IL_00c5: Expected O, but got Unknown
            GameObject val = Object.Instantiate<GameObject>(GameObject.Find("ScavengerModel"));
            foreach (Transform item in val.transform)
            {
                Transform val2 = item;
                if (((Object)val2).name == "LOD2" || ((Object)val2).name == "LOD3")
                {
                    ((Component)val2).gameObject.SetActive(false);
                }
                if (((Object)val2).name == "LOD1")
                {
                    ((Component)val2).gameObject.SetActive(true);
                }
                if (!(((Object)val2).name == "metarig"))
                {
                    continue;
                }
                foreach (Transform item2 in ((Component)val2).transform)
                {
                    Transform val3 = item2;
                    if (((Object)val3).name == "ScavengerModelArmsOnly")
                    {
                        ((Component)val3).gameObject.SetActive(false);
                    }
                    if (((Object)val3).name == "CameraContainer")
                    {
                        ((Component)val3).gameObject.SetActive(false);
                    }
                }
            }
            val.SetActive(false);
            val.AddComponent<PlayerHallucination>();
            val.AddComponent<NavMeshAgent>();
            val.GetComponent<LODGroup>().enabled = false;
            currentHallucinationModel = val;
        }

        private void CacheLights()
        {
            BunkerLights.Clear();
            foreach (Light allPoweredLight in RoundManager.Instance.allPoweredLights)
            {
                if (!bunkerLights.Contains(allPoweredLight))
                {
                    bunkerLights.Add(allPoweredLight);
                }
            }
        }
    }
    internal class InsanitySoundManager : MonoBehaviour
    {
        public static InsanitySoundManager Instance;

        public AudioClip[] hallucinationEffects;

        public AudioClip[] drones;

        public AudioClip[] playerHallucinationSounds;

        public AudioClip[] vanillaSFX;

        public AudioClip[] stingers;

        public AudioSource hallucinationSource;

        private AudioSource droneSource;

        private void Awake()
        {
            if ((Object)(object)Instance == (Object)null)
            {
                Instance = this;
            }
        }

        private void Start()
        {
            hallucinationSource = ((Component)this).gameObject.AddComponent<AudioSource>();
            hallucinationSource.spatialBlend = 0f;
            droneSource = ((Component)this).gameObject.AddComponent<AudioSource>();
            droneSource.spatialBlend = 0f;
            droneSource.volume = InsanityRemasteredConfiguration.SFXVolume;
            CacheSFX();
        }

        private void CacheSFX()
        {
            stingers = InsanityRemasteredContent.Stingers;
            vanillaSFX = InsanityRemasteredContent.LCGameSFX;
            drones = InsanityRemasteredContent.Drones;
            hallucinationEffects = InsanityRemasteredContent.AuditoryHallucinations;
            playerHallucinationSounds = InsanityRemasteredContent.PlayerHallucinationSounds;
        }

        public AudioClip LoadFakePlayerSound()
        {
            int num = Random.Range(0, playerHallucinationSounds.Length);
            if (Object.op_Implicit((Object)(object)playerHallucinationSounds[num]) && ((Object)playerHallucinationSounds[num]).name != "JumpScare")
            {
                return playerHallucinationSounds[num];
            }
            return null;
        }

        public void PlayJumpscare()
        {
            AudioClip[] array = playerHallucinationSounds;
            foreach (AudioClip val in array)
            {
                if (((Object)val).name == "JumpScare")
                {
                    hallucinationSource.clip = val;
                    hallucinationSource.Play();
                }
            }
        }

        public void PlayStinger(bool mono = true)
        {
            if (mono)
            {
                droneSource.clip = LoadStingerSound();
                droneSource.Play();
            }
        }

        public void PlayHallucinationSound()
        {
            float num = UnityEngine.Random.Range(0f, 1f);
            if (num >= 0.5f && SkinwalkerModIntegration.IsInstalled && StartOfRound.Instance.connectedPlayersAmount > 0)
            {
                SoundManager.Instance.PlaySoundAroundLocalPlayer(SkinwalkerModIntegration.GetRandomClip(), 2.5f);
            }
            else if (InsanityRemasteredConfiguration.customSFXEnabled)
            {
                SoundManager.Instance.PlaySoundAroundLocalPlayer(LoadHallucinationSound(), 0.85f);
            }
            else
            {
                SoundManager.Instance.PlaySoundAroundLocalPlayer(vanillaSFX[UnityEngine.Random.Range(0, vanillaSFX.Length)], 1.4f);
            }
        }

        public void PlayUISound(AudioClip sfx)
        {
            hallucinationSource.PlayOneShot(sfx, 0.8f);
        }

        public void PlayDrone()
        {
            if (!droneSource.isPlaying)
            {
                droneSource.clip = LoadDroneSound();
                droneSource.Play();
            }
        }

        public void StopModSounds()
        {
            hallucinationSource.Stop();
            droneSource.Stop();
        }

        public AudioClip LoadHallucinationSound()
        {
            float num = Random.Range(0f, 1f);
            if (num <= 0.4f)
            {
                int num2 = Random.Range(0, vanillaSFX.Length);
                if (Object.op_Implicit((Object)(object)vanillaSFX[num2]))
                {
                    return vanillaSFX[num2];
                }
            }
            else
            {
                int num3 = Random.Range(0, hallucinationEffects.Length);
                if (Object.op_Implicit((Object)(object)hallucinationEffects[num3]))
                {
                    return hallucinationEffects[num3];
                }
            }
            return null;
        }

        private AudioClip LoadStingerSound()
        {
            int num = Random.Range(0, stingers.Length);
            if (Object.op_Implicit((Object)(object)stingers[num]))
            {
                return stingers[num];
            }
            return null;
        }

        private AudioClip LoadDroneSound()
        {
            int num = Random.Range(0, drones.Length);
            if (Object.op_Implicit((Object)(object)drones[num]))
            {
                return drones[num];
            }
            return null;
        }
    }
}
namespace InsanityRemasteredMod
{
    internal class InsanityRemasteredContent
    {
        internal static Material[] Materials { get; set; }

        internal static GameObject[] EnemyModels { get; set; }

        internal static Texture2D[] Textures { get; set; }

        internal static AudioClip[] AuditoryHallucinations { get; set; }

        internal static AudioClip[] Stingers { get; set; }

        internal static AudioClip[] PlayerHallucinationSounds { get; set; }

        internal static AudioClip[] LCGameSFX { get; set; }

        internal static AudioClip[] Drones { get; set; }

        private static string DataFolder => Path.GetFullPath(Paths.PluginPath);

        public static void LoadContent()
        {
            LoadSounds();
        }

        public static GameObject GetEnemyModel(string name)
        {
            for (int i = 0; i < EnemyModels.Length; i++)
            {
                if (((Object)EnemyModels[i]).name == name)
                {
                    return EnemyModels[i];
                }
            }
            return null;
        }

        public static Material GetMaterial(string name)
        {
            for (int i = 0; i < EnemyModels.Length; i++)
            {
                if (((Object)Materials[i]).name == name)
                {
                    InsanityRemasteredLogger.Log("Sucessfully loaded material: " + name);
                    return Materials[i];
                }
            }
            return null;
        }

        public static Texture2D GetTexture(string name)
        {
            for (int i = 0; i < EnemyModels.Length; i++)
            {
                if (((Object)Textures[i]).name == name)
                {
                    InsanityRemasteredLogger.Log("Sucessfully loaded texture: " + name);
                    return Textures[i];
                }
            }
            return null;
        }

        private static void LoadEnemy()
        {
            string text = Path.Combine(DataFolder, "insanityremastered_enemies");
            InsanityRemasteredLogger.Log(text);
            AssetBundle val = AssetBundle.LoadFromFile(text);
            if ((Object)(object)val == (Object)null)
            {
                InsanityRemasteredLogger.Log("Failed to load enemies.");
            }
            EnemyModels = val.LoadAllAssets<GameObject>();
        }

        private static void LoadMaterials()
        {
            string text = Path.Combine(DataFolder, "insanityremastered_materials");
            AssetBundle val = AssetBundle.LoadFromFile(text);
            if ((Object)(object)val == (Object)null)
            {
                InsanityRemasteredLogger.Log("Failed to load materials.");
            }
            Materials = val.LoadAllAssets<Material>();
            Textures = val.LoadAllAssets<Texture2D>();
            EnemyModels = val.LoadAllAssets<GameObject>();
        }

        private static void LoadSounds()
        {
            string folderName = "Epicool-InsanityRemastered";
            string text = Path.Combine(DataFolder, folderName, "soundresources_sfx");
            string text2 = Path.Combine(DataFolder, folderName, "soundresources_stingers");
            string text3 = Path.Combine(DataFolder, folderName, "soundresources_hallucination");
            string text4 = Path.Combine(DataFolder, folderName, "soundresources_drones");
            string text5 = Path.Combine(DataFolder, folderName, "soundresources_lc");
            AssetBundle val = AssetBundle.LoadFromFile(text);
            AssetBundle val2 = AssetBundle.LoadFromFile(text2);
            AssetBundle val3 = AssetBundle.LoadFromFile(text3);
            AssetBundle val4 = AssetBundle.LoadFromFile(text4);
            AssetBundle val5 = AssetBundle.LoadFromFile(text5);
            if (val == null || val2 == null || val3 == null || text4 == null || text5 == null)
            {
                InsanityRemasteredLogger.LogError("Failed to load audio assets!");
                return;
            }
            AuditoryHallucinations = val.LoadAllAssets<AudioClip>();
            Stingers = val2.LoadAllAssets<AudioClip>();
            PlayerHallucinationSounds = val3.LoadAllAssets<AudioClip>();
            Drones = val4.LoadAllAssets<AudioClip>();
            LCGameSFX = val5.LoadAllAssets<AudioClip>();
        }
    }
    internal class InsanityRemasteredDebug
    {
        public static void QuickHotkeyTesting()
        {
            if (UnityInput.Current.GetKeyDown("f"))
            {
                SpawnFakePlayer();
            }
            if (UnityInput.Current.GetKeyDown("v"))
            {
                HallucinationManager.Instance.Hallucinate("Power loss");
            }
        }

        public static void SpawnFakePlayer()
        {
            HallucinationManager.Instance.PanicAttackLevel = 1f;
            PlayerPatcher.LocalPlayer.insanityLevel = 100f;
            HallucinationManager.Instance.Hallucinate("Fake Player");
        }

        public static void SpawnItem(string itemName)
        {
            //IL_0049: Unknown result type (might be due to invalid IL or missing references)
            //IL_0058: Unknown result type (might be due to invalid IL or missing references)
            //IL_0062: Unknown result type (might be due to invalid IL or missing references)
            //IL_0067: Unknown result type (might be due to invalid IL or missing references)
            //IL_0076: Unknown result type (might be due to invalid IL or missing references)
            foreach (Item items in StartOfRound.Instance.allItemsList.itemsList)
            {
                if (((Object)items).name == itemName)
                {
                    GameObject val = Object.Instantiate<GameObject>(items.spawnPrefab, ((Component)PlayerPatcher.LocalPlayer).transform.position + ((Component)PlayerPatcher.LocalPlayer).transform.forward * 2f, ((Component)PlayerPatcher.LocalPlayer).transform.rotation, RoundManager.Instance.spawnedScrapContainer);
                    GrabbableObject component = val.GetComponent<GrabbableObject>();
                    component.fallTime = 1f;
                    component.scrapPersistedThroughRounds = false;
                    component.grabbable = true;
                    if (items.isScrap)
                    {
                        component.SetScrapValue(Random.Range(items.minValue, items.maxValue));
                        ((NetworkBehaviour)component).NetworkObject.Spawn(false);
                    }
                }
            }
        }

        public static void SpawnObserver()
        {
            HallucinationManager.Instance.Hallucinate("Observer");
        }
    }
    internal static class InsanityRemasteredLogger
    {
        internal static ManualLogSource logSource;

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
    }
}
namespace InsanityRemasteredMod.Patches
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
namespace InsanityRemasteredMod.General
{
    internal class InsanityRemasteredConfiguration
    {
        public static int insanityMaxPlayerAmountScaling { get; set; }

        public static List<string> hallucinationTipTexts = new List<string> { "I'm always watching.", "behind you.", "You will never make it out of here.", "Did you see that?", "The company will never be satisfied. This is all pointless.", "you are the only one alive." };

        public static List<string> statusEffectTexts = new List<string> { "WARNING:\n\nMultiple organ failures detected. Please lie down and hope it ends quickly.", "SYSTEM ERROR:\n\nLife support power is dropping. Please return to your ship immediately.", "Unknown lifeform detected nearby." };

        public static bool panicAttackDeathsEnabled { get; set; }

        public static bool auditoryHallucinationsEnabled { get; set; }

        public static bool modelHallucinationsEnabled { get; set; }

        public static bool itemHallucinationsEnabled { get; set; }

        public static bool lightsOffEventEnabled { get; set; }

        public static bool panicAttacksEnabled { get; set; }

        public static bool panicAttackFXEnabled { get; set; }

        public static bool messageHallucinationsEnabled { get; set; }

        public static bool customSFXEnabled { get; set; }

        public static bool sanityRemindersEnabled { get; set; }

        public static bool useExperimentalSkinwalkerVersion { get; set; }

        public static float skinwalkerWalkiesFrequency { get; set; }

        public static float sanityGainLightProximity { get; set; }

        public static float sanityGainHearingWalkies { get; set; }

        public static bool skinwalkerWalkiesEnabled { get; set; }

        public static bool panicAttackDebuffsEnabled { get; set; }

        public static float sanityLossNearPlayersReduction { get; set; }

        public static float SFXVolume
        {
            get => SFXVolume;
            set => SFXVolume = Math.Clamp(value, 0, 1);
        }

        public static float rngCheckTimerMultiplier { get; set; }

        public static float insanitySoloScaling { get; set; }

        public static float sanityLossLightsOffEvent { get; set; }

        public static float sanityLossLookingAtModelHallucination { get; set; }

        public static float sanityLossPanicAttack { get; set; }

        public static float sanityLossDarkOutside { get; set; }

        public static float sanityGainLightOutside { get; set; }

        public static float sanityLossInsideFactory { get; set; }

        public static float sanityGainInsideShip { get; set; }

        public static float sanityGainFlashlight { get; set; }

        public static void Initialize(ConfigFile Config)
        {
            SFXVolume = Config.Bind("Volume", "Stinger/Drone volume", 0.25f, "Sets the volume of the stinger and drone sounds.\nValue Constraints: 0.0 - 1.0").Value;

            panicAttacksEnabled = Config.Bind("General", "Enable panic attacks", true, "Enables panic attacks.").Value;
            panicAttackDeathsEnabled = Config.Bind("General", "Enable deaths from panic attacks", false, "Enables the possibility to die when having a panic attack.").Value;
            panicAttackFXEnabled = Config.Bind("General", "Enable panic attack effects", true, "Enables the auditory and visual effects from panic attacks.").Value;
            panicAttackDebuffsEnabled = Config.Bind("General", "Enable panic attack debuffs", true, "Enables all panic attack debuffs. (e.g. slowness, cloudy vision)").Value;
            sanityRemindersEnabled = Config.Bind("General", "Enable sanity level notifications", true, "Enables notifications as your insanity begins to increase.").Value;
            
            insanityMaxPlayerAmountScaling = Config.Bind("Scaling", "Max player count for sanity loss scaling", 4, "Sets the max amount of players to take into account when scaling sanity loss.\nValue Constraints: 1 - 16").Value;
            insanitySoloScaling = Config.Bind("Scaling", "Solo insanity speed scaling", 0.75f, "Sets the scaling of insanity gain when playing solo. \nValue Constraints: 0.0 - 1.0").Value;
            
            sanityLossNearPlayersReduction = Config.Bind("Sanity Loss", "Reduction when near other players", 0.5f, "Multiplies the final sanity loss by this amount when near other players. Lower values reduce sanity loss.\nValue Constraints: 0.0 - 1.0").Value;
            sanityLossLightsOffEvent = Config.Bind("Sanity Loss", "Sanity loss during Lights Off event", 0.1f, "Sets the sanity loss during the Lights Off event.\nValue Constraints: 0.0 - 1.0").Value;
            sanityLossLookingAtModelHallucination = Config.Bind("Sanity Loss", "Sanity loss looking at a model hallucination", 0.1f, "Sets the sanity loss when looking at a model hallucination.\nValue Constraints: 0.0 - 1.0").Value;
            sanityLossPanicAttack = Config.Bind("Sanity Loss", "Sanity loss during a panic attack", 1.0f, "Sets the sanity loss during a panic attack.\nValue Constraints: 0.0 - 1.0").Value;
            sanityLossInsideFactory = Config.Bind("Sanity Loss", "Sanity loss inside the factory", 0.2f, "Sets the base sanity loss when you are inside the factory.\nValue Constraints: 0.0 - 1.0").Value;
            sanityLossDarkOutside = Config.Bind("Sanity Loss", "Sanity loss outside during nighttime", 0.1f, "Sets the base sanity loss when you are outside at night.\nValue Constraints: 0.0 - 1.0").Value;
            
            sanityGainLightProximity = Config.Bind("Sanity Gain", "Sanity gain near light", 0.04f, "Sets the sanity gain when near a light source.\nValue Constraints: 0.0 - 1.0").Value;
            sanityGainHearingWalkies = Config.Bind("Sanity Gain", "Sanity gain hearing walkies", 0.04f, "Sets the sanity gain when hearing other players through walkie talkies.\nValue Constraints: 0.0 - 1.0").Value;
            sanityGainFlashlight = Config.Bind("Sanity Gain", "Sanity gain with active flashlight", 0.08f, "Sets the sanity gain with an active flashlight.\nValue Constraints: 0.0 - 1.0").Value;
            sanityGainInsideShip = Config.Bind("Sanity Gain", "Sanity gain inside ship", 0.32f, "Sets the base sanity gain when inside the ship.\nValue Constraints: 0.0 - 1.0").Value;
            sanityGainLightOutside = Config.Bind("Sanity Gain", "Sanity gain outside during daytime", 0.16f, "Sets the base sanity gain when you are outside during day.\nValue Constraints: 0.0 - 1.0").Value;
            
            
            rngCheckTimerMultiplier = Config.Bind("Hallucinations", "Multiplier for hallucination RNG check", 1.0f, "A multiplier that affects the frequency of hallucinations. Higher values reduce the frequency.\nValue Constraints: 0.1 - 4.0").Value;
            messageHallucinationsEnabled = Config.Bind("Hallucinations", "Enable message hallucinations", true, "Enables cryptic hallucination messages from the system.").Value;
            itemHallucinationsEnabled = Config.Bind("Hallucinations", "Enable item hallucinations", true, "Enables hallucinations of fake items.").Value;
            modelHallucinationsEnabled = Config.Bind("Hallucinations", "Enable model hallucinations", true, "Enables hallucinations of fake players or enemy models.").Value;
            lightsOffEventEnabled = Config.Bind("Hallucinations", "Enable Lights Off event", false, "Enables a hallucination event in which the lights are shut off.").Value;
            auditoryHallucinationsEnabled = Config.Bind("Hallucinations", "Enable auditory hallucinations", true, "Enables auditory hallucinations.").Value;
            customSFXEnabled = Config.Bind("Misc", "Enable custom SFX", true, "Use custom sound effects for auditory hallucinations.").Value;
            skinwalkerWalkiesEnabled = Config.Bind("Hallucinations", "Enable skinwalker walkies", false, "Enables walkie talkies to play skinwalker clips.").Value;
            skinwalkerWalkiesFrequency = Config.Bind("Hallucinations", "Multiplier for skinwalker walkie frequency", 0.35f, "Enables walkie talkies to play skinwalker clips.\nValue Constraints: 0.1 - 1.0").Value;
            
            useExperimentalSkinwalkerVersion = Config.Bind("Misc", "Use experimental skinwalker version", false, "Allows InsanityRemastered to load the experimental version of Skinwalker.").Value;
        }

        public static void ValidateSettings()
        {
            if (SFXVolume > 1f)
            {
                SFXVolume = 1f;
            }
            if (insanityMaxPlayerAmountScaling <= 0)
            {
                insanityMaxPlayerAmountScaling = 1;
            }
            if (rngCheckTimerMultiplier <= 0f)
            {
                rngCheckTimerMultiplier = 1f;
            }
        }
    }
    internal class HallucinationEffects
    {
        // why does this variable exist?
        private static float PanicLevel => HallucinationManager.Instance.PanicAttackLevel;

        private static float EffectTransitionTime => HallucinationManager.Instance.EffectTransition;

        private static PlayerControllerB localPlayer => GameNetworkManager.Instance.localPlayerController;

        public static void LessenPanicEffects()
        {
            if (GameNetworkManager.Instance.gameHasStarted && ((!localPlayer.isInsideFactory && PanicLevel >= 0f) || (localPlayer.isInsideFactory && !localPlayer.isPlayerAlone && PanicLevel >= 0f)) && InsanityRemasteredConfiguration.panicAttackFXEnabled)
            {
                HUDManager.Instance.insanityScreenFilter.weight = Mathf.MoveTowards(HUDManager.Instance.insanityScreenFilter.weight, 0f, EffectTransitionTime - 100f * Time.deltaTime);
                SoundManager.Instance.SetDiageticMixerSnapshot(0, EffectTransitionTime - 100f);
            }
        }

        public static void IncreasePanicEffects()
        {
            if (InsanityRemasteredConfiguration.panicAttackFXEnabled)
            {
                HUDManager.Instance.insanityScreenFilter.weight = Mathf.MoveTowards(HUDManager.Instance.insanityScreenFilter.weight, 0.5f, EffectTransitionTime * Time.deltaTime);
                SoundManager.Instance.SetDiageticMixerSnapshot(1, EffectTransitionTime);
            }
        }
    }
}
