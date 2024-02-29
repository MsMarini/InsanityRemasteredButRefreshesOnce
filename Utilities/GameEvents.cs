using GameNetcodeStuff;
using HarmonyLib;
using System;

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
        private static void OnEnterLeaveFacility(EntranceTeleport __instance) /// THIS MAY NOT WORK WITH TELEPORTERS
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

        [HarmonyPatch(typeof(StartOfRound), "OnShipLandedMiscEvents")]
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

        [HarmonyPatch(typeof(PlayerControllerB), "SwitchToItemSlot")]
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
