using InsanityRemastered.General;
using InsanityRemastered.Patches;
using System;
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
                ((Component)this).gameObject.SetActive(false);
            }
        }

        private void Interaction()
        {
            //IL_0021: Unknown result type (might be due to invalid IL or missing references)
            //IL_0027: Unknown result type (might be due to invalid IL or missing references)
            //IL_0048: Unknown result type (might be due to invalid IL or missing references)
            float num = Random.Range(0, 1);
            if (num <= 0.35f)
            {
                PlayerPatcher.LocalPlayer.DropBlood(default(Vector3), true, false);
            }
            else if (num <= 1E-05f)
            {
                PlayerPatcher.LocalPlayer.KillPlayer(Vector3.zero, true, (CauseOfDeath)0, 0);
            }
            InsanitySoundManager.Instance.PlayStinger();
            ((Component)this).gameObject.SetActive(false);
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