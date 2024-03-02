using Dissonance;
using DunGen.Graph;
using GameNetcodeStuff;
using InsanityRemastered.Hallucinations;
using InsanityRemastered.ModIntegration;
using InsanityRemastered.Utilities;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

namespace InsanityRemastered.General
{
    internal class InsanityGameManager : MonoBehaviour
    {
        public static InsanityGameManager Instance;

        private List<Light> bunkerLights = [];
        private List<Animator> bunkerLightsAnimators = [];
        private float deletionTimer;
        private float deletionFrequency = 10f;
        public GameObject currentHallucinationModel;

        private PlayerControllerB LocalPlayer => GameNetworkManager.Instance.localPlayerController;
        public static DungeonFlow MapFlow => RoundManager.Instance.dungeonGenerator.Generator.DungeonFlow;
        public static bool AreOtherPlayersConnected => StartOfRound.Instance.connectedPlayersAmount > 0;
        public bool IsNearOtherPlayers => NearOtherPlayers();
        public bool IsNearLightSource => NearLightSource();
        public bool IsHearingPlayersThroughWalkie => PlayerIsHearingOthersThroughWalkieTalkie();
        public bool IsTalking => PlayerTalking();
        
        public bool LightsOff { get; private set; }
        public List<Light> BunkerLights => bunkerLights;
        public List<Animator> BunkerLightsAnimators => bunkerLightsAnimators;

        private void Awake()
        {
            if (!Instance)
            {
                Instance = this;
            }
            GameEvents.OnGameEnd += OnRoundEnd;
            GameEvents.OnShipLanded += GameEvents_OnShipLanded;
            GameEvents.OnPlayerDied += GameEvents_OnPlayerDied;
            GameEvents.OnEnterOrLeaveFacility += OnEnterOrLeaveFacility; /// something about all the references/etc. seem redundant, but idk
            HallucinationManager.OnPowerHallucination += PowerHallucination; /// something about all the references/etc. seem redundant, but idk
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

            if (GameNetworkManager.Instance.gameHasStarted && RoundManager.Instance.powerOffPermanently) /// may need to update LightsOff on round start/end
            {
                LightsOff = true;
            }
        }

        private void PowerHallucination(bool on)
        {
            LightsOff = !on;
        }

        private void SceneLoaded(Scene scene, LoadSceneMode arg1) /// might be able to simplify this if i knew more about scenes
        {
            if (scene.name == SceneNames.SampleSceneRelay.ToString())
            {
                SavePlayerModel();
                HallucinationManager.Instance.enabled = true;
            }
            else if (scene.name == SceneNames.MainMenu.ToString() || scene.name == SceneNames.InitSceneLaunchOptions.ToString())
            {
                HallucinationManager.Instance.enabled = false;
            }
        }

        private void GameEvents_OnPlayerDied()
        {
            InsanitySoundManager.Instance.StopModSounds();
            HallucinationManager.Instance.AdjustPanic(true);
            HallucinationManager.Instance.ResetLightsOff();
        }

        private void OnRoundEnd()
        {
            currentHallucinationModel.SetActive(false);
            LocalPlayer.insanityLevel = 0f;
        }

        private void OnEnterOrLeaveFacility(bool outside)
        {
            if (outside)
            {
                HallucinationManager.Instance.ResetLightsOff();
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
            VoicePlayerState voiceState = StartOfRound.Instance.voiceChatModule.FindPlayer(StartOfRound.Instance.voiceChatModule.LocalPlayerName);
            float volume = Mathf.Clamp(voiceState.Amplitude, 0f, 1f);
            return voiceState.IsSpeaking && volume > 0.85f;
        }

        private bool NearLightSource(float checkRadius = 10f) /// this seems intensive, but also like the only option?
        {
            for (int i = 0; i < bunkerLights.Count; i++)
            {
                float lightDistance = Vector3.Distance(bunkerLights[i].transform.position, LocalPlayer.transform.position);
                if (lightDistance < checkRadius && RoundManager.Instance.allPoweredLightsAnimators[i].GetBool("on"))
                {
                    return true;
                }
            }
            return false;
        }

        private void SavePlayerModel()
        {
            GameObject model = Instantiate(GameObject.Find("ScavengerModel"));

            foreach (Transform child in model.transform)
            {
                if (child.name == "LOD2" || child.name == "LOD3")
                {
                    child.gameObject.SetActive(false);
                }
                if (child.name == "LOD1")
                {
                    child.gameObject.SetActive(true);
                }
                if (child.name == "metarig")
                {
                    foreach (Transform _child in child.transform)
                    {
                        if (_child.name == "ScavengerModelArmsOnly")
                        {
                            _child.gameObject.SetActive(false);
                        }
                        if (_child.name == "CameraContainer")
                        {
                            _child.gameObject.SetActive(false);
                        }
                    }
                }
            }

            model.SetActive(false);
            model.AddComponent<PlayerHallucination>();
            model.AddComponent<NavMeshAgent>();
            model.GetComponent<LODGroup>().enabled = false;
            currentHallucinationModel = model;
        }

        private void CacheLights()
        {
            bunkerLights.Clear();
            bunkerLightsAnimators.Clear();

            foreach (Light light in RoundManager.Instance.allPoweredLights)
            {
                bunkerLights.Add(light);
            }

            foreach (Animator lightAnimator in RoundManager.Instance.allPoweredLightsAnimators)
            {
                bunkerLightsAnimators.Add(lightAnimator);
            }

            if (bunkerLights.Count != bunkerLightsAnimators.Count)
                InsanityRemasteredLogger.LogError("The lights and light animators are not equal. Please report this bug.");
        }
    }
}