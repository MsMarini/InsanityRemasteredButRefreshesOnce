using BepInEx;
using InsanityRemastered.General;
using InsanityRemastered.Patches;
using Unity.Netcode;
using UnityEngine;

namespace InsanityRemastered
{
    internal class InsanityRemasteredDebug
    {
        public static void QuickHotkeyTesting()
        {
            if (UnityInput.Current.GetKeyDown("f"))
            {
                SpawnFakePlayer();
            }
            if (UnityInput.Current.GetKeyDown("v"))
            {
                HallucinationManager.Instance.Hallucinate(HallucinationID.LightsOff);
            }
        }

        public static void SpawnFakePlayer()
        {
            HallucinationManager.Instance.PanicAttackLevel = 1f;
            PlayerPatcher.LocalPlayer.insanityLevel = 100f;
            HallucinationManager.Instance.Hallucinate(HallucinationID.FakePlayer);
        }

        public static void SpawnItem(string itemName)
        {
            foreach (Item items in StartOfRound.Instance.allItemsList.itemsList)
            {
                if (((Object)items).name == itemName)
                {
                    GameObject val = Object.Instantiate<GameObject>(items.spawnPrefab, ((Component)PlayerPatcher.LocalPlayer).transform.position + ((Component)PlayerPatcher.LocalPlayer).transform.forward * 2f, ((Component)PlayerPatcher.LocalPlayer).transform.rotation, RoundManager.Instance.spawnedScrapContainer);
                    GrabbableObject component = val.GetComponent<GrabbableObject>();
                    component.fallTime = 1f;
                    component.scrapPersistedThroughRounds = false;
                    component.grabbable = true;
                    if (items.isScrap)
                    {
                        component.SetScrapValue(Random.Range(items.minValue, items.maxValue));
                        ((NetworkBehaviour)component).NetworkObject.Spawn(false);
                    }
                }
            }
        }

        public static void SpawnObserver()
        {
            HallucinationManager.Instance.Hallucinate(HallucinationID.Observer);
        }
    }
}