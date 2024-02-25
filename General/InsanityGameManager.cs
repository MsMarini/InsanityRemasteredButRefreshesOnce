using Dissonance;
using DunGen.Graph;
using GameNetcodeStuff;
using InsanityRemastered.Hallucinations;
using InsanityRemastered.ModIntegration;
using InsanityRemastered.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace InsanityRemastered.General
{
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
}