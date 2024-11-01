﻿using BepInEx.Configuration;
using System;

namespace InsanityRemastered.General
{
    internal class InsanityRemasteredConfiguration
    {
        public static string[] tipMessageTexts =
        [
            "I'm always watching.",
            "behind you.",
            "did you see that?",

            "Time is running out.",
            "You will never make it out of here.",
            "you are the only one alive",
            
            "The company is just using you. This is all pointless.",
            "You will regret that."
        ];

        public static string[] statusEffectTexts =
        [
            "WARNING:\n\nMultiple organ failures detected. Multiple organ failures detected. Multiple organ failures detected. Multiple organ failures detected. Multiple organ failures detected. Multiple organ failures detected. Multiple organ failures detected. Multiple organ failures detected. Multiple organ failures detected. Multiple organ failures detected. Multiple organ failures detected. Multiple organ failures detected.",
            "WARNING:\n\nLife support systems compromised.\nLie down and hope it ends quickly.",

            "SYSTEM ERROR: SECURITY BREACH.\n\nAvoid all other crew members.",
            "SYSTEM ERROR: CRITICAL RADIATION EXPOSURE.\n\nImmediately discard all flashlights.",

            "Unknown lifeform detected nearby.",
            "Biological anomaly detected."
        ];

        public static float SFXVolume { get; set; }

        public static string configPreset { get; set; }
        public static bool panicAttacksEnabled { get; set; }
        public static bool panicAttackDeathsEnabled { get; set; }
        public static bool panicAttackFXEnabled { get; set; }
        public static bool panicAttackDebuffsEnabled { get; set; }
        public static bool sanityRemindersEnabled { get; set; }

        // now unused, but could be useful later on
        public static int insanityMaxPlayerAmountScaling { get; set; }

        public static float sanityLossSoloReduction { get; set; }
        public static float sanityLossNearPlayersReduction { get; set; }
        public static float sanityLossLightsOffEvent { get; set; }
        public static float sanityLossLookingAtModelHallucination { get; set; }
        public static float sanityLossPanicAttack { get; set; }
        public static float sanityLossInsideFactory { get; set; }
        public static float sanityLossDarkOutside { get; set; }

        public static float sanityGainLight { get; set; }
        public static float sanityGainHearingWalkies { get; set; }
        public static float sanityGainInsideShip { get; set; }
        public static float sanityGainLightOutside { get; set; }

        public static float hallucinationRNGMultiplier { get; set; }
        public static bool messageHallucinationsEnabled { get; set; }
        public static bool itemHallucinationsEnabled { get; set; }
        public static bool modelHallucinationsEnabled { get; set; }
        public static bool lightsOffEventEnabled { get; set; }
        public static bool auditoryHallucinationsEnabled { get; set; }
        public static bool customSFXEnabled { get; set; }
        public static bool skinwalkerWalkiesEnabled { get; set; }
        public static float skinwalkerWalkiesFrequency { get; set; }

        public static bool useThunderstoreFolderPath { get; set; }
        public static bool useExperimentalSkinwalkerVersion { get; set; }
        public static bool logDebugVariables { get; set; }


        public static void Initialize(ConfigFile Config)
        {
            SFXVolume = Config.Bind("Volume", "Stinger/drone volume", 0.5f, "Sets the volume of the stinger and drone sounds.\nValue Constraints: 0.0 - 1.0").Value;

            configPreset = Config.Bind("General", "Configures sanity gain/loss values to", "Custom", "\"Slow\" will allow you to set your own values. \"Slow\" is the recommended preset. \"Fast\" is the alternative preset.").Value;
            panicAttacksEnabled = Config.Bind("General", "Enable panic attacks", true, "Enables panic attacks.").Value;
            panicAttackDeathsEnabled = Config.Bind("General", "Enable deaths from panic attacks", false, "Enables the possibility to die when having a panic attack.").Value;
            panicAttackFXEnabled = Config.Bind("General", "Enable panic attack effects", true, "Enables the auditory and visual effects from panic attacks.").Value;
            panicAttackDebuffsEnabled = Config.Bind("General", "Enable panic attack debuffs", true, "Enables all panic attack debuffs. (e.g. slowness, cloudy vision)").Value;
            sanityRemindersEnabled = Config.Bind("General", "Enable sanity level notifications", true, "Enables notifications as your insanity begins to increase.").Value;

            sanityLossSoloReduction = Config.Bind("Sanity Loss", "Solo sanity scaling", 0.7f, "Sets the scaling of sanity loss when playing solo. \nValue Constraints: 0.1 - 1.0").Value;
            sanityLossNearPlayersReduction = Config.Bind("Sanity Loss", "Reduction when near other players", 0.65f, "Multiplies the final sanity loss by this amount when near other players. Lower values reduce sanity loss.\nValue Constraints: 0.1 - 1.0").Value;
            sanityLossLightsOffEvent = Config.Bind("Sanity Loss", "Sanity loss during Lights Off event", 0.11f, "Sets the sanity loss during the Lights Off event.\nValue Constraints: 0.0 - 1.0").Value;
            sanityLossLookingAtModelHallucination = Config.Bind("Sanity Loss", "Sanity loss looking at a model hallucination", 0f, "Sets the sanity loss when looking at a model hallucination.\nValue Constraints: 0.0 - 1.0").Value;
            sanityLossPanicAttack = Config.Bind("Sanity Loss", "Sanity loss during a panic attack", 0.22f, "Sets the sanity loss during a panic attack.\nValue Constraints: 0.0 - 1.0").Value;
            sanityLossInsideFactory = Config.Bind("Sanity Loss", "Sanity loss inside the factory", 0.22f, "Sets the base sanity loss when you are inside the factory.\nValue Constraints: 0.0 - 1.0").Value;
            sanityLossDarkOutside = Config.Bind("Sanity Loss", "Sanity loss outside during nighttime", 0.08f, "Sets the base sanity loss when you are outside at night.\nValue Constraints: 0.0 - 1.0").Value;

            sanityGainLight = Config.Bind("Sanity Gain", "Sanity gain near light or using flashlights", 0.11f, "Sets the sanity gain when near a light source or when using a flashlight.\nValue Constraints: 0.0 - 1.0").Value;
            sanityGainHearingWalkies = Config.Bind("Sanity Gain", "Sanity gain hearing walkies", 0.02f, "Sets the sanity gain when hearing other players through walkie talkies.\nValue Constraints: 0.0 - 1.0").Value;
            sanityGainInsideShip = Config.Bind("Sanity Gain", "Sanity gain inside ship", 0.32f, "Sets the base sanity gain when inside the ship.\nValue Constraints: 0.0 - 1.0").Value;
            sanityGainLightOutside = Config.Bind("Sanity Gain", "Sanity gain outside during daytime", 0.08f, "Sets the base sanity gain when you are outside during day.\nValue Constraints: 0.0 - 1.0").Value;

            hallucinationRNGMultiplier = Config.Bind("Hallucinations", "Multiplier for hallucination RNG check", 1.0f, "A multiplier that affects the frequency of hallucinations. Lower values increase the frequency.\nValue Constraints: 0.1 - 4.0").Value;
            messageHallucinationsEnabled = Config.Bind("Hallucinations", "Enable message hallucinations", true, "Enables cryptic hallucination messages from the system.").Value;
            itemHallucinationsEnabled = Config.Bind("Hallucinations", "Enable item hallucinations", true, "Enables hallucinations of fake items.").Value;
            modelHallucinationsEnabled = Config.Bind("Hallucinations", "Enable model hallucinations", true, "Enables hallucinations of fake players or enemy models.").Value;
            lightsOffEventEnabled = Config.Bind("Hallucinations", "Enable Lights Off event", true, "Enables a hallucination event in which the lights are shut off.").Value;
            auditoryHallucinationsEnabled = Config.Bind("Hallucinations", "Enable auditory hallucinations", true, "Enables auditory hallucinations.").Value;
            customSFXEnabled = Config.Bind("Misc", "Enable custom SFX", true, "Use custom sound effects for auditory hallucinations.").Value;
            skinwalkerWalkiesEnabled = Config.Bind("Hallucinations", "Enable skinwalker walkies", false, "Enables walkie talkies to play skinwalker clips.").Value;
            skinwalkerWalkiesFrequency = Config.Bind("Hallucinations", "Multiplier for skinwalker walkie frequency", 0.35f, "Enables walkie talkies to play skinwalker clips.\nValue Constraints: 0.1 - 1.0").Value;

            useThunderstoreFolderPath = Config.Bind("Misc", "Use Thunderstore plugin path", true, "This uses the folder path for Thunderstore plugins to load the assets.").Value;
            useExperimentalSkinwalkerVersion = Config.Bind("Misc", "Use experimental skinwalker version", false, "Allows InsanityRemastered to load the experimental version of Skinwalker.").Value;
            logDebugVariables = Config.Bind("Misc", "Log variables for debugging", false, "Logs variables intended for debugging and balancing.").Value;
        }

        public static void ValidateSettings()
        {
            if (configPreset.Equals("Slow", StringComparison.OrdinalIgnoreCase))
            {
                sanityLossNearPlayersReduction = 0.65f;
                sanityLossLightsOffEvent = 0.11f;
                sanityLossLookingAtModelHallucination = 0f;
                sanityLossPanicAttack = 0.22f;
                sanityLossInsideFactory = 0.22f;
                sanityLossDarkOutside = 0.08f;

                sanityGainLight = 0.11f;
                sanityGainHearingWalkies = 0.02f;
                sanityGainInsideShip = 0.32f;
                sanityGainLightOutside = 0.08f;
            }
            if (configPreset.Equals("Fast", StringComparison.OrdinalIgnoreCase))
            {
                sanityLossNearPlayersReduction = 0.85f;
                sanityLossLightsOffEvent = 0.15f;
                sanityLossLookingAtModelHallucination = 0.15f;
                sanityLossPanicAttack = 0.4f;
                sanityLossInsideFactory = 0.35f;
                sanityLossDarkOutside = 0.15f;

                sanityGainLight = 0.35f;
                sanityGainHearingWalkies = 0.05f;
                sanityGainInsideShip = 0.6f;
                sanityGainLightOutside = 0.3f;
            }

            SFXVolume = Math.Clamp(SFXVolume, 0f, 1f);

            sanityLossSoloReduction = Math.Clamp(sanityLossSoloReduction, 0.1f, 1f);
            sanityLossNearPlayersReduction = Math.Clamp(sanityLossNearPlayersReduction, 0.1f, 1f);
            sanityLossLightsOffEvent = Math.Clamp(sanityLossLightsOffEvent, 0f, 1f);
            sanityLossLookingAtModelHallucination = Math.Clamp(sanityLossLookingAtModelHallucination, 0f, 1f);
            sanityLossPanicAttack = Math.Clamp(sanityLossPanicAttack, 0f, 1f);
            sanityLossInsideFactory = Math.Clamp(sanityLossInsideFactory, 0f, 1f);
            sanityLossDarkOutside = Math.Clamp(sanityLossDarkOutside, 0f, 1f);

            sanityGainLight = Math.Clamp(sanityGainLight, 0f, 1f);
            sanityGainHearingWalkies = Math.Clamp(sanityGainHearingWalkies, 0f, 1f);
            sanityGainInsideShip = Math.Clamp(sanityGainInsideShip, 0f, 1f);
            sanityGainLightOutside = Math.Clamp(sanityGainLightOutside, 0f, 1f);

            hallucinationRNGMultiplier = Math.Clamp(hallucinationRNGMultiplier, 0.1f, 4.0f);
            skinwalkerWalkiesFrequency = Math.Clamp(skinwalkerWalkiesFrequency, 0.1f, 1f);
        }
    }
}