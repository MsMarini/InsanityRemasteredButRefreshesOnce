using GameNetcodeStuff;
using InsanityRemastered;
using InsanityRemastered.General;
using InsanityRemastered.ModIntegration;
using InsanityRemastered.Patches;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace InsanityRemastered.Hallucinations
{
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
            if (SkinwalkerModIntegration.IsInstalled && InsanityGameManager.AreOtherPlayersConnected && !spoken)
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
            if (touched)
            {
                float scareRNG = Random.Range(0f, 1f);
                if (scareRNG < 0.15f)
                {
                    LocalPlayer.DamagePlayer(Random.Range(1, 5), false, true, (CauseOfDeath)6, 0, false, default(Vector3));
                    return;
                }
                else if (scareRNG < 0.45f)
                {
                    HallucinationManager.Instance.PanicAttackSymptom(true);
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
            if (PlayerPatcher.CurrentSanityLevel >= SanityLevel.Medium)
            {
                GenerateWanderPoints();
                hallucinationType = HallucinationType.Wandering;
            }
            float num = Random.Range(0f, 1f);
            float num2 = Random.Range(0f, 1f);
            if (num >= 0.5f && SkinwalkerModIntegration.IsInstalled && InsanityGameManager.AreOtherPlayersConnected)
            {
                soundSource.clip = SkinwalkerModIntegration.GetRandomClip();
            }
            else
            {
                soundSource.clip = InsanitySoundManager.Instance.LoadFakePlayerSound();
            }
            if (num2 > 0.25f)
            {
                HUDManager.Instance.DisplayTip("", InsanityRemasteredConfiguration.tipMessageTexts[0], true, false, "LC_Tip1");
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