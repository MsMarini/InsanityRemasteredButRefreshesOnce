using InsanityRemastered.ModIntegration;
using InsanityRemastered;
using UnityEngine;

namespace InsanityRemastered.General
{
    internal class InsanitySoundManager : MonoBehaviour
    {
        public static InsanitySoundManager Instance;

        public AudioClip[] hallucinationSFX;
        public AudioClip[] drones;
        public AudioClip[] playerHallucinationSounds;
        public AudioClip[] vanillaSFX;
        public AudioClip[] stingers;
        public AudioSource hallucinationSource;
        private AudioSource droneSource;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }

        private void Start()
        {
            hallucinationSource = gameObject.AddComponent<AudioSource>();
            hallucinationSource.spatialBlend = 0f;
            droneSource = gameObject.AddComponent<AudioSource>();
            droneSource.spatialBlend = 0f;
            droneSource.volume = InsanityRemasteredConfiguration.SFXVolume;
            CacheSFX();
        }

        private void CacheSFX()
        {
            vanillaSFX = InsanityRemasteredContent.LCGameSFX;
            stingers = InsanityRemasteredContent.Stingers;
            drones = InsanityRemasteredContent.Drones;
            hallucinationSFX = InsanityRemasteredContent.AuditoryHallucinations;
            playerHallucinationSounds = InsanityRemasteredContent.PlayerHallucinationSounds;
        }

        public AudioClip LoadFakePlayerSound()
        {
            int randomClip = Random.Range(0, playerHallucinationSounds.Length);
            if (playerHallucinationSounds[randomClip] && playerHallucinationSounds[randomClip].name != "JumpScare")
            {
                return playerHallucinationSounds[randomClip];
            }
            return null;
        }

        public void PlayJumpscare()
        {
            foreach (AudioClip clip in playerHallucinationSounds)
            {
                if (clip.name == "JumpScare")
                {
                    hallucinationSource.clip = clip;
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
            if (SkinwalkerModIntegration.IsInstalled && StartOfRound.Instance.connectedPlayersAmount > 0 && Random.Range(0f, 1f) < 0.4f)
            {
                SoundManager.Instance.PlaySoundAroundLocalPlayer(SkinwalkerModIntegration.GetRandomClip(), 2.5f);
            }
            else
            {
                SoundManager.Instance.PlaySoundAroundLocalPlayer(LoadHallucinationSound(), 0.9f);
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
            if (InsanityRemasteredConfiguration.customSFXEnabled && Random.Range(0f, 1f) < 0.6f)
            {
                int randomClip = Random.Range(0, hallucinationSFX.Length);
                if (hallucinationSFX[randomClip])
                {
                    return hallucinationSFX[randomClip];
                }
            }
            else
            {
                int randomClip = Random.Range(0, vanillaSFX.Length);
                if (vanillaSFX[randomClip])
                {
                    return vanillaSFX[randomClip];
                }
            }
            return null;
        }

        private AudioClip LoadStingerSound()
        {
            int randomClip = Random.Range(0, stingers.Length);
            if (stingers[randomClip])
            {
                return stingers[randomClip];
            }
            return null;
        }

        private AudioClip LoadDroneSound()
        {
            int randomClip = Random.Range(0, drones.Length);
            if (drones[randomClip])
            {
                return drones[randomClip];
            }
            return null;
        }
    }
}