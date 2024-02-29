using GameNetcodeStuff;
using InsanityRemastered.General;
using InsanityRemastered.ModIntegration;
using InsanityRemastered.Patches;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace InsanityRemastered.Hallucinations
{
    internal class PlayerHallucination : InsanityRemasteredAI
    {
        private float stareTimer = 5f;
        private float waitTimeForNewWander = 5f;
        private float wanderTimer;
        private float stareDuration;

        private float rotationSpeed = 0.95f;
        private float footstepDistance = 1.5f;

        private int minWanderPoints = 3;
        private int maxWanderPoints = 5;
        private int currentFootstepSurfaceIndex;
        private List<Vector3> wanderPositions = [];
        private Vector3 lastStepPosition;
        private AudioSource footstepSource;
        private bool spoken;
        private bool seenPlayer;
        public static SkinnedMeshRenderer suitRenderer;

        private void StopAndStare()
        {
            soundSource.Stop();
            seenPlayer = true;
            agent.isStopped = true;
            hallucinationAnimator.SetBool(AnimationID.PlayerWalking, false);
            

            Stare(LocalPlayer.transform.position);

            if (SkinwalkerModIntegration.IsInstalled && InsanityGameManager.AreOtherPlayersConnected && !spoken)
            {
                soundSource.PlayOneShot(SkinwalkerModIntegration.GetRandomClip());
                spoken = true;
            }

            stareDuration += Time.deltaTime;
            if (stareDuration > stareTimer)
            {
                agent.isStopped = false;
                hallucinationAnimator.enabled = true;
                hallucinationAnimator.SetBool(AnimationID.PlayerWalking, true);
                hallucinationType = HallucinationType.Approaching;
            }
        }

        private void Stare(Vector3 position)
        {
            Quaternion lookRotation = Quaternion.LookRotation(position - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);
        }

        private void GenerateNewDestination()
        {
            hallucinationAnimator.SetBool(AnimationID.PlayerWalking, true);

            Vector3 randomPosition = wanderPositions[Random.Range(0, wanderPositions.Count)];
            wanderPositions.Remove(randomPosition);
            agent.SetDestination(randomPosition);
            agent.isStopped = false;
            wanderSpot = true;
        }

        private void ReachDestination()
        {
            if (wanderPositions.Count == 0)
            {
                FinishHallucination(false);
            }

            hallucinationAnimator.SetBool(AnimationID.PlayerWalking, false);
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
            if (wanderPositions.Count > 0)
            {
                wanderPositions.Clear();
            }

            for (int i = 0; i < Random.Range(minWanderPoints, maxWanderPoints); i++)
            {
                wanderPositions.Add(RoundManager.Instance.GetRandomNavMeshPositionInRadius(transform.position, 20f));
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
            Ray materialRay = new(transform.position + Vector3.up, -Vector3.up);

            if (!Physics.Raycast(materialRay, out RaycastHit hit, 6f, StartOfRound.Instance.walkableSurfacesMask, QueryTriggerInteraction.Ignore) || hit.collider.CompareTag(StartOfRound.Instance.footstepSurfaces[currentFootstepSurfaceIndex].surfaceTag))
            {
                return;
            }

            for (int i = 0; i < StartOfRound.Instance.footstepSurfaces.Length; i++)
            {
                if (hit.collider.CompareTag(StartOfRound.Instance.footstepSurfaces[i].surfaceTag))
                {
                    currentFootstepSurfaceIndex = i;
                    break;
                }
            }
        }

        private void Footstep()
        {
            if (Vector3.Distance(transform.position, lastStepPosition) > footstepDistance)
            {
                lastStepPosition = transform.position;
                PlayFootstepSound();
            }
        }

        private int GetRandomPlayerSuitID()
        {
            PlayerControllerB randomPlayer;

            do
            {
                randomPlayer = StartOfRound.Instance.allPlayerScripts[Random.Range(0, StartOfRound.Instance.allPlayerScripts.Length)];
            }
            while (!randomPlayer.isPlayerControlled);

            return randomPlayer.currentSuitID;
        }

        private void SetSuit(int id)
        {
            Material suitMaterial = StartOfRound.Instance.unlockablesList.unlockables[id].suitMaterial;
            suitRenderer.material = suitMaterial;
        }

        public override void Start()
        {
            base.Start();

            sound = InsanityRemasteredContent.PlayerHallucinationSounds;
            hallucinationAnimator.runtimeAnimatorController = StartOfRound.Instance.localClientAnimatorController;
            footstepSource = gameObject.AddComponent<AudioSource>();
            footstepSource.spatialBlend = 1f;
        }

        public override void Update()
        {
            base.Update();

            if (hallucinationType == HallucinationType.Wandering)
            {
                if (HasLineOfSightToPosition(transform, LocalPlayer.transform.position, 45f, 45) || Vector3.Distance(transform.position, LocalPlayer.transform.position) < 3f || seenPlayer)
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
                Stare(LocalPlayer.transform.position);
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
                if (scareRNG < 0.2f)
                {
                    LocalPlayer.DamagePlayer(Random.Range(1, 5), false, causeOfDeath: CauseOfDeath.Suffocation); /// can this actually kill?
                    return;
                }
                else if (scareRNG < 0.5f)
                {
                    HallucinationManager.Instance.PanicAttackSymptom(true);
                }

                base.FinishHallucination(true);
            }
            else
            {
                base.FinishHallucination(false);
            }
        }

        public override void Wander()
        {
            /* maybe apparatus, if nearby?
            if(poi == null || poi == Vector3.zero)
            {
                poi = GeneratePointOfInterest();
            }
            */
            if (wanderPositions.Count != 0 && !wanderSpot)
            {
                GenerateNewDestination();
            }
            if (Vector3.Distance(transform.position, agent.destination) <= agent.stoppingDistance && wanderSpot)
            {
                ReachDestination();
            }
            // LookAtPointOfInterest();
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

            if (SkinwalkerModIntegration.IsInstalled && InsanityGameManager.AreOtherPlayersConnected && Random.Range(0f, 1f) < 0.5f)
            {
                soundSource.clip = SkinwalkerModIntegration.GetRandomClip();
            }
            else
            {
                soundSource.clip = InsanitySoundManager.Instance.LoadFakePlayerSound();
            }

            if (Random.Range(0f, 1f) < 0.7f)
            {
                HUDManager.Instance.DisplayTip("", InsanityRemasteredConfiguration.tipMessageTexts[0], true);
            }

            spoken = false;
            soundSource.Play();
            hallucinationAnimator.SetBool(AnimationID.PlayerWalking, false);
            stareDuration = 0f;
        }

        public override void SetupVariables()
        {
            base.SetupVariables();

            agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
            suitRenderer = GetComponentInChildren<SkinnedMeshRenderer>(false);
            duration = 30f;
            hallucinationAnimator.runtimeAnimatorController = StartOfRound.Instance.localClientAnimatorController;
            hallucinationSpawnType = HallucinationSpawnType.NotLooking;
            lastStepPosition = transform.position;
        }
    }
}