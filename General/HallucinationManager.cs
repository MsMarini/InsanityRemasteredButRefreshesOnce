using GameNetcodeStuff;
using InsanityRemastered;
using InsanityRemastered.Hallucinations;
using InsanityRemastered.Patches;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

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
}