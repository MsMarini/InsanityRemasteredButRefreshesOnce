# Insanity Remastered (WIP, currently fixing the decompiled code errors)

# This may be my one and only update. We'll see. I just wanted to fix the audio assets not loading, but I ended up doing a bit more.

A mod for Lethal Company that adds more features to the insanity mechanic and tweaks a few things related to it.

This is a client-sided mod as it would be odd if other players could see your hallucinations.

Installation:
Drag InsanityRemastered.dll and the folder named "InsanityRemastered" into the BepInEx plugins folder.


### Losing Sanity:
   - Changed the amount of insanity gained when alone inside the facility.
   - During the Light Shutoff hallucination, you will lose a bit more sanity than normal.
   - Panic attacks will cause the most sanity loss.

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

This can manifest in them wandering around the facility aimlessly, staring at you, or even them chasing after you.

Be wary of them though. They can cause you to go insane really quickly.

### Fake Items:

While inside the facility, you can stumble upon a piece of scrap that manifests around you.

Scanning it will reveal that its value is above what it should normally be. Picking it up will make you uneasy.

It will despawn if it is not picked up within a few seconds of spawning.

## Panic Attacks:

At the highest level of insanity, you will slowly experience a panic attack.

During one, some hallucinations become lethal and you experience one of the following symptoms:
- Slowness
- Cloudy Vision
- Death

Panic attacks can be helped by returning to the ship for awhile, or being around players and lights.

## Configuration:

A lot of things in this mod are tweakable. It's recommended to keep the values as they are, or scale them equally, since the code may not 

## Suggestions and Bug Reports:

If you have any suggestions please leave them on them in the Discussions tab on the GitHub.

Bug reports/compatibility issues should be reported in the Issues tab.

## Known Issues:

The Lights Off feature may cause the lights to turn on again after you take the apparatus.

The Skinwalkers mod is not supported in my fork. I left the code, but it is disabled by default and will not be maintained as of now. I prefer Mirage anyway, but I don't know how to do most of the compatibility code.

This was made from decompiled code and by a newbie programmer.
