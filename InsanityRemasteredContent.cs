using BepInEx;
using InsanityRemastered.General;
using InsanityRemastered.Patches;
using System.IO;
using System.Reflection;
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
            //LoadEnemy();
            //LoadMaterials();
            LoadSounds();
        }

        public static GameObject GetEnemyModel(string name)
        {
            for (int i = 0; i < EnemyModels.Length; i++)
            {
                if (EnemyModels[i].name == name)
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
                if (Materials[i].name == name)
                {
                    InsanityRemasteredLogger.Log("Successfully loaded material: " + name);
                    return Materials[i];
                }
            }
            return null;
        }

        public static Texture2D GetTexture(string name)
        {
            for (int i = 0; i < EnemyModels.Length; i++)
            {
                if (Textures[i].name == name)
                {
                    InsanityRemasteredLogger.Log("Successfully loaded texture: " + name);
                    return Textures[i];
                }
            }
            return null;
        }

        private static void LoadEnemy()
        {
            string enemyBundlePath = Path.Combine(DataFolder, "insanityremastered_enemies");
            InsanityRemasteredLogger.Log(enemyBundlePath);
            AssetBundle enemies = AssetBundle.LoadFromFile(enemyBundlePath);
            
            if (!enemies)
            {
                InsanityRemasteredLogger.LogWarning("Failed to load enemies.");
            }

            EnemyModels = enemies.LoadAllAssets<GameObject>();
        }

        private static void LoadMaterials()
        {
            string materialBundlePath = Path.Combine(DataFolder, "insanityremastered_materials");
            InsanityRemasteredLogger.Log(materialBundlePath);
            AssetBundle materials = AssetBundle.LoadFromFile(materialBundlePath);

            if (!materials)
            {
                InsanityRemasteredLogger.LogWarning("Failed to load materials.");
            }

            Materials = materials.LoadAllAssets<Material>();
            Textures = materials.LoadAllAssets<Texture2D>();
            EnemyModels = materials.LoadAllAssets<GameObject>();
        }

        private static void LoadSounds()
        {
            string modFolder;

            if (InsanityRemasteredConfiguration.useThunderstoreFolderPath)
                modFolder = "Epicool-InsanityRemastered";
            else
                modFolder = "InsanityRemastered";

            string sfxPath = Path.Combine(DataFolder, modFolder, "soundresources_sfx");
            string ambiencePath = Path.Combine(DataFolder, modFolder, "soundresources_stingers");
            string fakePlayerPath = Path.Combine(DataFolder, modFolder, "soundresources_hallucination");
            string dronePath = Path.Combine(DataFolder, modFolder, "soundresources_drones");
            string lcGamePath = Path.Combine(DataFolder, modFolder, "soundresources_lc");


            AssetBundle sfx = AssetBundle.LoadFromFile(sfxPath);
            AssetBundle ambience = AssetBundle.LoadFromFile(ambiencePath);
            AssetBundle fakePlayer = AssetBundle.LoadFromFile(fakePlayerPath);
            AssetBundle drone = AssetBundle.LoadFromFile(dronePath);
            AssetBundle lcGame = AssetBundle.LoadFromFile(lcGamePath);


            if (InsanityRemasteredConfiguration.logDebugVariables)
            {
                InsanityRemasteredLogger.LogVariables([
                    nameof(modFolder),
                    nameof(sfxPath),
                    nameof(ambiencePath),
                    nameof(fakePlayerPath),
                    nameof(dronePath),
                    nameof(lcGamePath),
                    nameof(sfx),
                    nameof(ambience),
                    nameof(fakePlayer),
                    nameof(drone),
                    nameof(lcGame)
                ],
                [
                    modFolder,
                    sfxPath,
                    ambiencePath,
                    fakePlayerPath,
                    dronePath,
                    lcGamePath,
                    sfx,
                    ambience,
                    fakePlayer,
                    drone,
                    lcGame
                ]);
            }

            if (sfx && ambience && fakePlayer && drone && lcGame)
            {
                InsanityRemasteredLogger.Log("Successfully loaded audio assets!");
            }
            else
            {
                InsanityRemasteredLogger.LogError("Failed to load audio assets!");
                return;
            }

            AuditoryHallucinations = sfx.LoadAllAssets<AudioClip>();
            Stingers = ambience.LoadAllAssets<AudioClip>();
            PlayerHallucinationSounds = fakePlayer.LoadAllAssets<AudioClip>();
            Drones = drone.LoadAllAssets<AudioClip>();
            LCGameSFX = lcGame.LoadAllAssets<AudioClip>();
        }
    }
}