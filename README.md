# Insanity Remastered
### (Original by BudgetAirpods)

## This may be my one and only update; I just wanted to fix the audio assets not loading, but I ended up doing a bit more.

A mod for Lethal Company that adds more features to the insanity mechanic and tweaks a few things related to it.

This is a client-sided mod as it would be odd if other players could see your hallucinations.

Manual Installation:
Drag InsanityRemastered.dll and the folder named "InsanityRemastered" into the BepInEx plugins folder.


### Losing Sanity:
   - Changed the amount of insanity gained when alone inside the facility.
   - During the Light Shutoff hallucination, you will lose a bit more sanity than normal.
   - Panic attacks will cause great sanity loss.

### Gaining Sanity:
   - Being near an active light source or using flashlights will regenerate sanity.
   - Being around players will drastically reduce your sanity loss.
   - Consuming pills will reset your sanity level entirely.

## Insanity Scaling:
   - Sanity loss will scale with how many players are in your lobby.
   - Solo players can change a multiplier that affects their sanity loss as well.
     
**There are now three levels of insanity. High, Medium, and Low.**
   - As your insanity level progresses, you will experience more intense hallucinations.
   - Hallucinations can also happen in success with each other when your insanity is high enough, leading to potentially chaotic results.

### Insanity notifier:
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

You can recover from panic attacks by stepping outside the facility or being around light.

## Configuration:

A lot of things in this mod are tweakable. Currently the mechanics cause sanity to accumulate throughout the day, and being in the ship is the only way to quickly clear it.

## Suggestions and Bug Reports:

If you have any suggestions or advice, please leave them on them on GitHub's Discussion section.

Bug reports and compatibility issues should be reported in the Issues section.

## Known Issues:

-The Lights Off feature may cause the lights to turn on again after you take the apparatus. Intraday issues should be resolved, but it may not be consistent across days.

-The Skinwalkers mod is not supported in my fork. I left the code, but it is disabled by default and will not be maintained as of now. I prefer Mirage anyway, but I don't know how to do most of the compatibility code.

-IsHearingPlayersThroughWalkie may not be compatible with Advanced Company, and it may give sanity from hearing hallucinations from Skinwalker clips.

-Disabling hallucinations will reduce the hallucination frequency due to how hallucinations are called.

-Advanced Company lights may not be compatible as of right now, until I figure out why. (Please check out my GitHub and help!)

-This was made from decompiled code and by a newbie programmer.


### features to implement?:
proximity/chased/looking at monsters increases insanity

damage from hallucinations scale with insanity

FlashlightOn method may be improved? (different method of detection)

bug list:
Turned off breaker -> took apparatus -> Lights Off hallucination -> left facility -> reentered facility -> lights were on!
