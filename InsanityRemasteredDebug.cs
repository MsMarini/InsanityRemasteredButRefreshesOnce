using BepInEx;
using InsanityRemastered.General;
using InsanityRemastered.Patches;
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
            foreach (Item item in StartOfRound.Instance.allItemsList.itemsList)
            {
                if (item.name == itemName)
                {
                    GameObject prop = Object.Instantiate (
                        item.spawnPrefab,
                        PlayerPatcher.LocalPlayer.transform.position + PlayerPatcher.LocalPlayer.transform.forward * 2f,
                        PlayerPatcher.LocalPlayer.transform.rotation,
                        RoundManager.Instance.spawnedScrapContainer
                    );

                    GrabbableObject grabbable = prop.GetComponent<GrabbableObject>();
                    grabbable.fallTime = 1f;
                    grabbable.scrapPersistedThroughRounds = false;
                    grabbable.grabbable = true;

                    if (item.isScrap)
                    {
                        grabbable.SetScrapValue(Random.Range(item.minValue, item.maxValue));
                        grabbable.NetworkObject.Spawn(false);
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