using GameNetcodeStuff;
using InsanityRemastered.General;
using UnityEngine;

namespace InsanityRemastered.General
{
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