using GameNetcodeStuff;
using InsanityRemastered.General;
using InsanityRemastered.Patches;
using System;
using UnityEngine;
using UnityEngine.AI;

namespace InsanityRemastered.Hallucinations
{
    internal class InsanityRemasteredAI : MonoBehaviour
    {
        protected float duration = 30f;
        private float agentStoppingDistance = 3f;
        private float durationTimer;
        private bool notSeenYet = true;
        protected bool wanderSpot = false;
        private bool isSetUp = false;

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

        public virtual void LookingAtHallucination() /// currently unimplemented, but i could add sanity loss here instead
        {

        }

        public virtual void LookAtHallucinationFirstTime()
        {
            notSeenYet = false;
            LocalPlayer.JumpToFearLevel(0.5f);
        }

        public virtual void FinishHallucination(bool touched)
        {
            if (touched)
            {
                LocalPlayer.JumpToFearLevel(1f, true);
                
                if (UnityEngine.Random.Range(0f, 1f) < 0.4f)
                {
                    InsanitySoundManager.Instance.PlayJumpscare();
                }
            }
            else
            {
                LocalPlayer.insanityLevel = Mathf.Max(LocalPlayer.insanityLevel - 5f, 0f);
            }
            
            OnHallucinationEnded?.Invoke(touched);
            PoolForLater();
        }

        public virtual void Wander()
        {
            if (!wanderSpot)
            {
                agent.SetDestination(RoundManager.Instance.GetRandomNavMeshPositionInRadius(aiNodes[UnityEngine.Random.Range(0, aiNodes.Length)].transform.position, 12f));
                wanderSpot = true;
            }

            if (Vector3.Distance(transform.position, agent.destination) <= agentStoppingDistance)
            {
                PoolForLater();
                OnHallucinationEnded?.Invoke(false);
            }
        }

        public virtual void TimerTick()
        {
            durationTimer += Time.deltaTime;
            if (durationTimer > duration)
            {
                durationTimer = 0f;
                FinishHallucination(false);
            }
        }

        public virtual void ChasePlayer()
        {
            TimerTick();
            if (Vector3.Distance(transform.position, LocalPlayer.transform.position) <= agentStoppingDistance)
            {
                FinishHallucination(true);
            }

            agent.SetDestination(LocalPlayer.transform.position);
        }

        public virtual void PoolForLater()
        {
            agent.enabled = false;
            transform.position = Vector3.zero;
            gameObject.SetActive(false);
        }

        private void LoadAINodes()
        {
            /* not sure why this is commented out in the original code *edit: well its already done below. do we still need this method then?
            if (LocalPlayer.isInsideFactory)
            {
                aiNodes = GameObject.FindGameObjectsWithTag("AINode");
            }
            else
            {
                aiNodes = GameObject.FindGameObjectsWithTag("OutsideAINode");
            }
            */
        }

        public virtual void Update()
        {
            if (LocalPlayer.HasLineOfSightToPosition(transform.position))
            {
                if (notSeenYet)
                {
                    LookAtHallucinationFirstTime();
                }
                /// LookingAtHallucination(); unimplemented method
                PlayerPatcher.lookingAtModelHallucination = true;
            }
            else
            {
                PlayerPatcher.lookingAtModelHallucination = false;
            }
        }

        public virtual void SetupVariables()
        {
            if (!isSetUp)
            {
                aiNodes = GameObject.FindGameObjectsWithTag("AINode");
                agent = GetComponent<NavMeshAgent>();
                hallucinationAnimator = GetComponentInChildren<Animator>();
                soundSource = gameObject.AddComponent<AudioSource>();
                soundSource.spatialBlend = 1f;

                agent.angularSpeed = float.PositiveInfinity;
                agent.speed = 3f;
                agent.stoppingDistance = agentStoppingDistance;
                agent.areaMask = StartOfRound.Instance.walkableSurfacesMask;

                isSetUp = true;
            }
        }

        private Vector3 FindSpawnPosition()
        {
            if (hallucinationSpawnType == HallucinationSpawnType.NotLooking)
            {
                for (int i = 0; i < aiNodes.Length; i++)
                {
                    if (!Physics.Linecast(LocalPlayer.gameplayCamera.transform.position, aiNodes[i].transform.position, StartOfRound.Instance.collidersAndRoomMaskAndDefault) && !LocalPlayer.HasLineOfSightToPosition(aiNodes[i].transform.position, 45f, 20, 8f))
                    {
                        return aiNodes[i].transform.position;
                    }
                }
            }

            return Vector3.zero;
        }

        private void OnEnable()
        {
            if (isSetUp)
            {
                Spawn();
            }
            else
            {
                SetupVariables();
                isSetUp = true;
            }
        }
    }

}