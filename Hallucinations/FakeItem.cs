using InsanityRemastered.General;
using InsanityRemastered.Patches;
using UnityEngine;

namespace InsanityRemastered.Hallucinations
{
    internal class FakeItem : MonoBehaviour
    {
        private float stayTimer = 50f;

        private void Update()
        {
            stayTimer -= Time.deltaTime;
            if (stayTimer <= 0f)
            {
                gameObject.SetActive(false);
            }
        }

        private void Interaction()
        {
            float action = Random.Range(0f, 1f);

            if (action < 0.35f)
            {
                PlayerPatcher.LocalPlayer.DropBlood(); /// inflict damage, if it doesn't already
            }
            else if (action < 0.001f)
            {
                PlayerPatcher.LocalPlayer.KillPlayer(Vector3.zero);
            }

            InsanitySoundManager.Instance.PlayStinger();
            gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            PlayerPatcher.OnInteractWithFakeItem += Interaction;
        }

        private void OnDisable()
        {
            PlayerPatcher.OnInteractWithFakeItem -= Interaction;
        }
    }
}