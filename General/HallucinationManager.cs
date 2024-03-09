using GameNetcodeStuff;
using InsanityRemastered.Hallucinations;
using InsanityRemastered.Patches;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace InsanityRemastered.General
{
    internal class HallucinationManager : MonoBehaviour
    {
        public static HallucinationManager Instance;

        private float droneRNGTimer;
        private float droneRNGFrequency = 60f;
        private float hallucinationRNGTimer;
        private float hallucinationRNGFrequency = 2000f;
        private float panicAttackLevel;

        public static bool slowness;
        public static bool reduceVision;

        private readonly Dictionary<string, InsanityLevel> hallucinations = new()
        {
            {
                HallucinationID.Auditory, InsanityLevel.Low
            },
            {
                HallucinationID.CrypticStatus, InsanityLevel.Low
            },
            {
                HallucinationID.CrypticMessage, InsanityLevel.Low
            },
            {
                HallucinationID.FakePlayer, InsanityLevel.Medium
            },
            {
                HallucinationID.FakeItem, InsanityLevel.Medium
            },
            {
                HallucinationID.LightsOff, InsanityLevel.High
            }
        };
        // properties backed by unowned private fields
        private PlayerControllerB LocalPlayer => GameNetworkManager.Instance.localPlayerController;
        private InsanityLevel InsanityLevel => PlayerPatcher.CurrentInsanityLevel;
        // properties backed by owned private fields
        public float PanicAttackLevel
        {
            get => panicAttackLevel;
            set => panicAttackLevel = value;
        }
        public Dictionary<string, InsanityLevel> Hallucinations => hallucinations;
        // actions
        public static event Action<bool> OnPowerHallucination;
        public static event Action OnPlayerHallucinationStarted;
        public static event Action OnSpawnFakeItem;
        public static event Action OnSoundPlayed;
        public static event Action OnSanityRecovered;
        public static event Action OnExperiencePanicAttack;
        public static event Action OnUIHallucination;

        private void Start()
        {
            hallucinationRNGFrequency *= InsanityRemasteredConfiguration.hallucinationRNGMultiplier;
        }

        private void Awake()
        {
            Instance = this;
        }

        private void Update()
        {
            if (!LocalPlayer.isPlayerDead && LocalPlayer.isPlayerControlled && LocalPlayer.isInsideFactory)
            {
                if (LocalPlayer.insanityLevel < 20f)
                {
                    hallucinationRNGTimer += Time.deltaTime * 20f;
                }
                else
                {

                    hallucinationRNGTimer += Time.deltaTime * LocalPlayer.insanityLevel;
                }

                if (hallucinationRNGTimer > hallucinationRNGFrequency)
                {
                    hallucinationRNGTimer = 0f;

                    if (InsanityRemasteredConfiguration.logDebugVariables)
                        InsanityRemasteredLogger.LogError("hallucinationRNGTimer = 0f");

                    if (UnityEngine.Random.Range(0f, 300f) < LocalPlayer.insanityLevel + 100f)
                    {
                        Hallucinate(GetRandomHallucination());
                    }
                }

                if (PlayerPatcher.CurrentInsanityLevel >= InsanityLevel.High)
                {
                    droneRNGTimer += Time.deltaTime;
                    if (droneRNGTimer > droneRNGFrequency)
                    {
                        droneRNGTimer = 0f;
                        if (UnityEngine.Random.Range(0f, 1f) < 0.4f)
                        {
                            InsanitySoundManager.Instance.PlayDrone();
                            if (PlayerPatcher.CurrentInsanityLevel == InsanityLevel.Max)
                            {
                                PanicAttackSymptom();
                            }
                        }
                    }
                }
            }

            if (GameNetworkManager.Instance.gameHasStarted)
            {
                AdjustPanic();
            }
            else
            {
                AdjustPanic(true);
            }

            /// LOGGER
            if (InsanityRemasteredConfiguration.logDebugVariables)
            {
                if (InsanityRemasteredLogger.logTimer < 10f)
                {
                    InsanityRemasteredLogger.logTimer += Time.deltaTime;
                }
                else
                {
                    InsanityRemasteredLogger.logTimer = 0f;

                    InsanityRemasteredLogger.LogVariables
                    ([
                        nameof(LocalPlayer.insanitySpeedMultiplier),
                        nameof(LocalPlayer.insanityLevel),
                        nameof(panicAttackLevel),
                        nameof(PlayerPatcher.CurrentInsanityLevel),
                        nameof(hallucinationRNGTimer),
                        nameof(LocalPlayer.isPlayerAlone),
                        nameof(InsanityGameManager.Instance.IsNearLightSource),
                        nameof(StartOfRound.Instance.connectedPlayersAmount),
                        nameof(PlayerPatcher.PlayersConnected)
                    ],
                    [
                        LocalPlayer.insanitySpeedMultiplier,
                        LocalPlayer.insanityLevel,
                        panicAttackLevel,
                        PlayerPatcher.CurrentInsanityLevel,
                        hallucinationRNGTimer,
                        LocalPlayer.isPlayerAlone,
                        InsanityGameManager.Instance.IsNearLightSource,
                        StartOfRound.Instance.connectedPlayersAmount,
                        PlayerPatcher.PlayersConnected
                    ]);
                }
            }
        }

        public void AdjustPanic(bool reset = false)
        {
            if (!InsanityRemasteredConfiguration.panicAttacksEnabled)
            {
                return;
            }

            if (reset)
            {
                panicAttackLevel = 0f;
                SoundManager.Instance.SetDiageticMixerSnapshot(0, 1f);
                OnSanityRecovered?.Invoke();
            }
            else if (!LocalPlayer.isInsideFactory || !LocalPlayer.isPlayerAlone)
            {
                panicAttackLevel = Mathf.MoveTowards(panicAttackLevel, 0f, 0.25f * Time.deltaTime);
                if (InsanityRemasteredConfiguration.panicAttackFXEnabled)
                {
                    HUDManager.Instance.insanityScreenFilter.weight = Mathf.MoveTowards(HUDManager.Instance.insanityScreenFilter.weight, 0f, Time.deltaTime);
                    SoundManager.Instance.SetDiageticMixerSnapshot(0, 16f);
                }
            }
            else if (PlayerPatcher.CurrentInsanityLevel >= InsanityLevel.High && !InsanityGameManager.Instance.IsNearLightSource)
            {
                panicAttackLevel = Mathf.MoveTowards(panicAttackLevel, 1f, 0.5f * Time.deltaTime);
                if (InsanityRemasteredConfiguration.panicAttackFXEnabled)
                {
                    HUDManager.Instance.insanityScreenFilter.weight = Mathf.MoveTowards(HUDManager.Instance.insanityScreenFilter.weight, 0.5f, Time.deltaTime);
                    SoundManager.Instance.SetDiageticMixerSnapshot(1, 64f);
                }
            }

            if (panicAttackLevel == 0f)
            {
                slowness = false;
                reduceVision = false;
            }
        }

        public void PanicAttackSymptom(bool canKill = false)
        {
            LocalPlayer.JumpToFearLevel(0.6f, true);

            if (!InsanityRemasteredConfiguration.panicAttacksEnabled)
            {
                return;
            }

            InsanityRemasteredLogger.Log("Applying panic attack symptom.");

            if (PanicAttackLevel < 0.9f)
                panicAttackLevel = 0.9f;

            if (InsanityRemasteredConfiguration.panicAttackDebuffsEnabled)
            {
                switch (UnityEngine.Random.Range(0, 6))
                {
                    case 0:
                        break;
                    case 1:
                    case 2:
                        slowness = true;
                        break;
                    case 3:
                    case 4:
                        reduceVision = true;
                        break;
                    case 5:
                        if (InsanityRemasteredConfiguration.panicAttackDeathsEnabled && canKill)
                        {
                            LocalPlayer.KillPlayer(Vector3.zero);
                        }
                        else
                        {
                            slowness = true;
                            reduceVision = true;
                        }
                        break;
                }
            }

            if (InsanityRemasteredConfiguration.sanityRemindersEnabled)
                HUDManager.Instance.DisplayTip("WARNING!", "Heartrate is at dangerous levels. Please seek help immediately.", true); /// this could be a set of strings in another class?
            OnExperiencePanicAttack?.Invoke();
        }

        public void Hallucinate(string id)
        {
            InsanityRemasteredLogger.Log("Performing hallucination with ID: " + id);
            
            switch (id)
            {
                case HallucinationID.CrypticStatus:
                    HallucinateStatusEffect();
                    break;
                case HallucinationID.CrypticMessage:
                    HallucinateTipMessage();
                    break;
                case HallucinationID.FakePlayer:
                    HallucinatePlayerModel(InsanityGameManager.Instance.currentHallucinationModel);
                    break;
                case HallucinationID.Auditory:
                    HallucinateSound();
                    break;
                case HallucinationID.FakeItem:
                    HallucinateFakeItem();
                    break;
                case HallucinationID.LightsOff:
                    HallucinateLightsOff();
                    break;
                default:
                    InsanityRemasteredLogger.LogWarning("No such hallucination with ID: " + id);
                    break;
            }
        }

        public string GetRandomHallucination()
        {
            KeyValuePair<string, InsanityLevel> randomHallucination;

            do
            {
                randomHallucination = hallucinations.ElementAt(UnityEngine.Random.Range(0, hallucinations.Count()));
            }
            while (randomHallucination.Value > InsanityLevel);
            
            return randomHallucination.Key;
        }

        public string GetRandomHallucination(string excludedHallucination)
        {
            KeyValuePair<string, InsanityLevel> randomHallucination;

            do
            {
                randomHallucination = hallucinations.ElementAt(UnityEngine.Random.Range(0, hallucinations.Count()));
            }
            while (randomHallucination.Value > InsanityLevel || randomHallucination.Key.Equals(excludedHallucination));
            
            return randomHallucination.Key;
        }

        private void HallucinateStatusEffect()
        {
            if (InsanityRemasteredConfiguration.messageHallucinationsEnabled)
            {
                int randomTextIndex = UnityEngine.Random.Range(0, InsanityRemasteredConfiguration.statusEffectTexts.Length);
                string randomText = InsanityRemasteredConfiguration.statusEffectTexts[randomTextIndex];

                HUDManager.Instance.DisplayStatusEffect(randomText);

                if (UnityEngine.Random.Range(0f, 100f) < 20f + LocalPlayer.insanityLevel / 2)
                {
                    switch (randomTextIndex)
                    {
                        case 0:
                        case 1:
                        PanicAttackSymptom();
                        break;
                        case 2:
                        case 4:
                        Hallucinate(HallucinationID.FakePlayer);
                        break;
                        case 3:
                        Hallucinate(HallucinationID.LightsOff);
                        break;
                    }
                }
                OnUIHallucination?.Invoke();
            }
        }

        private void HallucinateTipMessage()
        {
            if (InsanityRemasteredConfiguration.messageHallucinationsEnabled)
            {
                int randomTextIndex = UnityEngine.Random.Range(0, InsanityRemasteredConfiguration.tipMessageTexts.Length);
                string randomText = InsanityRemasteredConfiguration.tipMessageTexts[randomTextIndex];

                HUDManager.Instance.DisplayTip("", randomText, true);
                OnUIHallucination?.Invoke();
                
                ///Invoke(nameof(HallucinateLightsOff), 3f); Invoke could be useful, but how does it work with method parameters?

                if (UnityEngine.Random.Range(0f, 100f) < 10f + (LocalPlayer.insanityLevel / 4))
                {
                    switch (randomTextIndex)
                    {
                        case 1:
                        case 5:
                        Hallucinate(HallucinationID.FakePlayer);
                        break;
                        case 6:
                        Hallucinate(HallucinationID.FakeItem);
                        break;
                        case 7:
                        Hallucinate(HallucinationID.Auditory);
                        break;
                    }
                }
            }
        }

        private void HallucinateLightsOff(bool reset = false)
        {
            if (InsanityRemasteredConfiguration.lightsOffEventEnabled && !RoundManager.Instance.powerOffPermanently)
            {
                if (reset && InsanityGameManager.Instance.LightsOff)
                {
                    foreach (Animator light in InsanityGameManager.Instance.BunkerLightsAnimators) //// check out `public IEnumerator turnOnLights(bool turnOn)` and `public void FlickerLights(bool flickerFlashlights = false, bool disableFlashlights = false)`
                    {
                        light.SetBool("on", true);
                    }
                    OnPowerHallucination?.Invoke(true);
                }
                else if (!reset && !InsanityGameManager.Instance.LightsOff)
                {
                    foreach (Animator light in InsanityGameManager.Instance.BunkerLightsAnimators)
                    {
                        light.SetBool("on", false);
                    }
                    PlayerPatcher.LocalPlayer.JumpToFearLevel(0.4f);
                    OnPowerHallucination?.Invoke(false);
                }
            }
        }

        public void ResetLightsOff()
        {
            HallucinateLightsOff(true);
        }

        private void HallucinatePlayerModel(GameObject model)
        {
            if (InsanityRemasteredConfiguration.modelHallucinationsEnabled)
            {
                if (!model.activeInHierarchy)
                {
                    model.SetActive(true);
                }
                OnPlayerHallucinationStarted?.Invoke();
            }
        }

        private void HallucinateSound()
        {
            if (InsanityRemasteredConfiguration.auditoryHallucinationsEnabled)
            {
                InsanitySoundManager.Instance.PlayHallucinationSound();
                OnSoundPlayed?.Invoke();
            }
        }

        private void HallucinateFakeItem()
        {
            if (InsanityRemasteredConfiguration.itemHallucinationsEnabled)
            {
                SpawnableItemWithRarity originalScrap = RoundManager.Instance.currentLevel.spawnableScrap[UnityEngine.Random.Range(0, RoundManager.Instance.currentLevel.spawnableScrap.Count)];
                Vector3 spawnPosition = RoundManager.Instance.GetRandomNavMeshPositionInRadiusSpherical(PlayerPatcher.LocalPlayer.transform.position) + Vector3.up * originalScrap.spawnableItem.verticalOffset;
                GameObject fakeScrap = Instantiate(originalScrap.spawnableItem.spawnPrefab, spawnPosition, Quaternion.identity);
                GrabbableObject grabbableComponent = fakeScrap.GetComponent<GrabbableObject>();
                grabbableComponent.SetScrapValue(UnityEngine.Random.Range(originalScrap.spawnableItem.minValue, originalScrap.spawnableItem.maxValue + 50));

                fakeScrap.AddComponent<FakeItem>();
                OnSpawnFakeItem?.Invoke();
            }
        }
    }
}