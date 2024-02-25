using BepInEx;
using System.IO;
using UnityEngine;

namespace InsanityRemastered
{
    internal class InsanityRemasteredContent
    {
        internal static Material[] Materials { get; set; }

        internal static GameObject[] EnemyModels { get; set; }

        internal static Texture2D[] Textures { get; set; }

        internal static AudioClip[] AuditoryHallucinations { get; set; }

        internal static AudioClip[] Stingers { get; set; }

        internal static AudioClip[] PlayerHallucinationSounds { get; set; }

        internal static AudioClip[] LCGameSFX { get; set; }

        internal static AudioClip[] Drones { get; set; }

        private static string DataFolder => Path.GetFullPath(Paths.PluginPath);

        public static void LoadContent()
        {
            LoadSounds();
        }

        public static GameObject GetEnemyModel(string name)
        {
            for (int i = 0; i < EnemyModels.Length; i++)
            {
                if (((Object)EnemyModels[i]).name == name)
                {
                    return EnemyModels[i];
                }
            }
            return null;
        }

        public static Material GetMaterial(string name)
        {
            for (int i = 0; i < EnemyModels.Length; i++)
            {
                if (((Object)Materials[i]).name == name)
                {
                    InsanityRemasteredLogger.Log("Sucessfully loaded material: " + name);
                    return Materials[i];
                }
            }
            return null;
        }

        public static Texture2D GetTexture(string name)
        {
            for (int i = 0; i < EnemyModels.Length; i++)
            {
                if (((Object)Textures[i]).name == name)
                {
                    InsanityRemasteredLogger.Log("Sucessfully loaded texture: " + name);
                    return Textures[i];
                }
            }
            return null;
        }

        private static void LoadEnemy()
        {
            string text = Path.Combine(DataFolder, "insanityremastered_enemies");
            InsanityRemasteredLogger.Log(text);
            AssetBundle val = AssetBundle.LoadFromFile(text);
            if ((Object)(object)val == (Object)null)
            {
                InsanityRemasteredLogger.Log("Failed to load enemies.");
            }
            EnemyModels = val.LoadAllAssets<GameObject>();
        }

        private static void LoadMaterials()
        {
            string text = Path.Combine(DataFolder, "insanityremastered_materials");
            AssetBundle val = AssetBundle.LoadFromFile(text);
            if ((Object)(object)val == (Object)null)
            {
                InsanityRemasteredLogger.Log("Failed to load materials.");
            }
            Materials = val.LoadAllAssets<Material>();
            Textures = val.LoadAllAssets<Texture2D>();
            EnemyModels = val.LoadAllAssets<GameObject>();
        }

        private static void LoadSounds()
        {
            string folderName = "Epicool-InsanityRemastered";
            string text = Path.Combine(DataFolder, folderName, "soundresources_sfx");
            string text2 = Path.Combine(DataFolder, folderName, "soundresources_stingers");
            string text3 = Path.Combine(DataFolder, folderName, "soundresources_hallucination");
            string text4 = Path.Combine(DataFolder, folderName, "soundresources_drones");
            string text5 = Path.Combine(DataFolder, folderName, "soundresources_lc");
            AssetBundle val = AssetBundle.LoadFromFile(text);
            AssetBundle val2 = AssetBundle.LoadFromFile(text2);
            AssetBundle val3 = AssetBundle.LoadFromFile(text3);
            AssetBundle val4 = AssetBundle.LoadFromFile(text4);
            AssetBundle val5 = AssetBundle.LoadFromFile(text5);
            if (val == null || val2 == null || val3 == null || text4 == null || text5 == null)
            {
                InsanityRemasteredLogger.LogError("Failed to load audio assets!");
                return;
            }
            AuditoryHallucinations = val.LoadAllAssets<AudioClip>();
            Stingers = val2.LoadAllAssets<AudioClip>();
            PlayerHallucinationSounds = val3.LoadAllAssets<AudioClip>();
            Drones = val4.LoadAllAssets<AudioClip>();
            LCGameSFX = val5.LoadAllAssets<AudioClip>();
        }
    }
}