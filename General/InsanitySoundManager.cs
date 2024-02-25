using InsanityRemastered.ModIntegration;
using InsanityRemastered;
using UnityEngine;

namespace InsanityRemastered.General
{
    internal class InsanitySoundManager : MonoBehaviour
    {
        public static InsanitySoundManager Instance;

        public AudioClip[] hallucinationEffects;

        public AudioClip[] drones;

        public AudioClip[] playerHallucinationSounds;

        public AudioClip[] vanillaSFX;

        public AudioClip[] stingers;

        public AudioSource hallucinationSource;

        private AudioSource droneSource;

        private void Awake()
        {
            if ((Object)(object)Instance == (Object)null)
            {
                Instance = this;
            }
        }

        private void Start()
        {
            hallucinationSource = ((Component)this).gameObject.AddComponent<AudioSource>();
            hallucinationSource.spatialBlend = 0f;
            droneSource = ((Component)this).gameObject.AddComponent<AudioSource>();
            droneSource.spatialBlend = 0f;
            droneSource.volume = InsanityRemasteredConfiguration.SFXVolume;
            CacheSFX();
        }

        private void CacheSFX()
        {
            stingers = InsanityRemasteredContent.Stingers;
            vanillaSFX = InsanityRemasteredContent.LCGameSFX;
            drones = InsanityRemasteredContent.Drones;
            hallucinationEffects = InsanityRemasteredContent.AuditoryHallucinations;
            playerHallucinationSounds = InsanityRemasteredContent.PlayerHallucinationSounds;
        }

        public AudioClip LoadFakePlayerSound()
        {
            int num = Random.Range(0, playerHallucinationSounds.Length);
            if (Object.op_Implicit((Object)(object)playerHallucinationSounds[num]) && ((Object)playerHallucinationSounds[num]).name != "JumpScare")
            {
                return playerHallucinationSounds[num];
            }
            return null;
        }

        public void PlayJumpscare()
        {
            AudioClip[] array = playerHallucinationSounds;
            foreach (AudioClip val in array)
            {
                if (((Object)val).name == "JumpScare")
                {
                    hallucinationSource.clip = val;
                    hallucinationSource.Play();
                }
            }
        }

        public void PlayStinger(bool mono = true)
        {
            if (mono)
            {
                droneSource.clip = LoadStingerSound();
                droneSource.Play();
            }
        }

        public void PlayHallucinationSound()
        {
            float num = UnityEngine.Random.Range(0f, 1f);
            if (num >= 0.5f && SkinwalkerModIntegration.IsInstalled && StartOfRound.Instance.connectedPlayersAmount > 0)
            {
                SoundManager.Instance.PlaySoundAroundLocalPlayer(SkinwalkerModIntegration.GetRandomClip(), 2.5f);
            }
            else if (InsanityRemasteredConfiguration.customSFXEnabled)
            {
                SoundManager.Instance.PlaySoundAroundLocalPlayer(LoadHallucinationSound(), 0.85f);
            }
            else
            {
                SoundManager.Instance.PlaySoundAroundLocalPlayer(vanillaSFX[UnityEngine.Random.Range(0, vanillaSFX.Length)], 1.4f);
            }
        }

        public void PlayUISound(AudioClip sfx)
        {
            hallucinationSource.PlayOneShot(sfx, 0.8f);
        }

        public void PlayDrone()
        {
            if (!droneSource.isPlaying)
            {
                droneSource.clip = LoadDroneSound();
                droneSource.Play();
            }
        }

        public void StopModSounds()
        {
            hallucinationSource.Stop();
            droneSource.Stop();
        }

        public AudioClip LoadHallucinationSound()
        {
            float num = Random.Range(0f, 1f);
            if (num <= 0.4f)
            {
                int num2 = Random.Range(0, vanillaSFX.Length);
                if (Object.op_Implicit((Object)(object)vanillaSFX[num2]))
                {
                    return vanillaSFX[num2];
                }
            }
            else
            {
                int num3 = Random.Range(0, hallucinationEffects.Length);
                if (Object.op_Implicit((Object)(object)hallucinationEffects[num3]))
                {
                    return hallucinationEffects[num3];
                }
            }
            return null;
        }

        private AudioClip LoadStingerSound()
        {
            int num = Random.Range(0, stingers.Length);
            if (Object.op_Implicit((Object)(object)stingers[num]))
            {
                return stingers[num];
            }
            return null;
        }

        private AudioClip LoadDroneSound()
        {
            int num = Random.Range(0, drones.Length);
            if (Object.op_Implicit((Object)(object)drones[num]))
            {
                return drones[num];
            }
            return null;
        }
    }
}