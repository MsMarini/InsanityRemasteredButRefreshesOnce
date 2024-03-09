# Insanity Remastered
## (Original by BudgetAirpods)
### Most credit goes to him, I wouldn't know what to do from scratch. Upstream GitHub: https://github.com/BudgetAirpods/InsanityRemastered/tree/main

## I do not plan on maintaining this mod.
### If anyone wants to take over, feel free to make your own fork or make pull requests at the respository.

A mod for Lethal Company that adds more features to the insanity mechanic and tweaks a few things related to it.

Manual Installation:
Drag the folder named "Epicool-InsanityRemastered" into the BepInEx plugins folder.


## Sanity Calculations:
   - Based on your current location, you will either gain or lose sanity. Various factors will futher adjust how much sanity you gain or lose.
   - There are two presets for the config values. The recommended, "Slow", aims to have sanity difficult to recover and gradually build up throughout the day. "Fast" will cause sanity to be gained and lost quickly.

### Losing Sanity:
   - Being in the factory or nighttime outside
   - Being around other players will drastically reduce your sanity loss
   - Being in the dark
   - Experiencing hallucinations (avoid the fake player hallucination!)

### Gaining Sanity:
   - Being on the ship or daytime outside
   - Being near a light or having an active flashlight
   - Consuming pills will reset your sanity level entirely

## Insanity Scaling:
   - There are presets that adjust how much sanity is gained and lost. "Slow" is the recommended preset!
   - Solo players can change a multiplier that affects their sanity loss as well.
     
**There are now three levels of insanity. High, Medium, and Low.**
   - As your insanity level progresses, you will experience more intense hallucinations.
   - Hallucinations are more frequent the more insane you are.

### Insanity notifiers:
While exploring the facility, your suit will occasionally let you know if your insanity level is increasing beyond what is deemed safe.

## Hallucinations

### Auditory Hallucinations:
In addition to the current sounds that play when inside the facility, this mod adds several new sounds to throw you off.

There's also a chance certain vanilla sound effects will play to make the sounds more believable.

### Lights Off:

While inside the facility, the lights have a chance to go out for you and you only.

They will power back on their own eventually, but you can step outside and re-enter the facility to force them on.

### Fake Players:

While exploring the facilities, you have a chance of running into a hallucination of a player.

They can simply wander around the facility, stare at you, or chase you.

### Fake Items:

While inside the facility, you can stumble upon a piece of scrap that manifests next to you.

Scanning it will reveal that its value is above what it should normally be. Picking it up is unadvised.

## Panic Attacks:

At the highest level of insanity, you will slowly experience a panic attack.

During one, some hallucinations become lethal and you experience one of the following symptoms:
- Impaired Movement
- Impaired Vision
- Death

You can recover from panic attacks by stepping outside the facility, being around light, or being next to a friend!

## Configuration:

A lot of things in this mod are tweakable. Currently the mechanics cause sanity to accumulate throughout the day, and being in the ship is the only way to quickly clear it.

## Suggestions and Bug Reports:

If you have any suggestions or advice, please leave them on them on GitHub's Discussion section.

Bug reports and compatibility issues should be reported in the Issues section.

## Known Issues:

-The Lights Off hallucination may cause bugs between days.

-The Skinwalkers mod is not supported in my fork. I left the code, but it is disabled by default and will not be maintained as of now. I prefer Mirage anyway, but I don't know how to do most of the compatibility code.

-IsHearingPlayersThroughWalkie may not be compatible with Advanced Company, and it may give sanity from hearing hallucinations from Skinwalker clips.

-Disabling hallucinations will reduce the hallucination frequency due to how hallucinations are called.

-Advanced Company lights may not be compatible as of right now, until I figure out why. (Please check out my GitHub and help!)

-This was made from decompiled code and by a newbie programmer.


### Features to implement:
-Being chased by monsters increases insanity
-Damage from hallucinations scale with insanity
