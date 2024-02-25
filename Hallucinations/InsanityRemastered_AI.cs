using GameNetcodeStuff;
using InsanityRemastered.General;
using InsanityRemastered.Patches;
using System;
using UnityEngine;
using UnityEngine.AI;

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

}