using System.Collections.Generic;
using UnityEngine;

namespace InsanityRemastered.ModIntegration
{
    public class SkinwalkerModIntegration
    {
        private static List<AudioClip> skinwalkerClips = [];

        public static bool IsInstalled { get; set; }

        internal static void UpdateClips(ref List<AudioClip> ___cachedAudio)
        {
            skinwalkerClips = ___cachedAudio;
        }

        public static AudioClip GetRandomClip()
        {
            return skinwalkerClips[UnityEngine.Random.Range(0, skinwalkerClips.Count)];
        }

        internal static void AddRecording(object recording) /// this might be broken from the decompiler, i cant tell
        {
            skinwalkerClips.Add((AudioClip)recording.GetType().GetField("clip").GetValue(recording));
        }

        public static void ClearRecordings()
        {
            skinwalkerClips.Clear();
        }
    }
}