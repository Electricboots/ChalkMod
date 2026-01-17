# ChalkMod
<img width="400" height="400" alt="chalk-2025-11-08  6-45-13 02" src="https://github.com/user-attachments/assets/e973c236-69d8-4eb4-87bd-9ad95f563b94" />

A chalk management/moderation plugin for [Webfishing Cove](https://github.com/DrMeepso/WebFishingCove) dedicated server for the [Webfishing](https://store.steampowered.com/app/3146520/WEBFISHING/) game.
If you look at the source code, you might think "Did a monkey write this?" This is very insulting. I am the equivalent of at least 10 monkeys. 🐵 (Seriously though, I barely know what I'm doing...)

## IMPORTANT
 - For "the best user experience", you must use the 'official' Chalk Logs plugin together with Chalk Mod.
 - I offer no guarantee that this plugin works at all, but I have been using on my own Cove server for about 3 weeks now and it's been working well.

## Installation

 - All files go into your Cove plugins folder, except for *chalkmod.json* which goes into your main Cove folder.
 - *chalkmod.json* is the configuration file for the plugin.
    - *webhookuser*: it is the name used in Discord for posting the chalk.
    - *webhookurl*: put your Discord webhook URL in here so that an image of the chalk is posted on your Discord every time a backup is made. Leave blank (ie: empty quotes) if you don't want to post anything to Discord.
    - *checkseconds*: it is used to autobackup the chalk file if it has been updated at some point during the last [*checkseconds*] seconds (default 300 seconds)
    - *palette*: it is the color palette info used when converting the chalk data to a PNG file. <u>Leave this one alone</u>, unless you really know what kind of effect your changes would have, or if you like weird colors that don't make sense I guess.

## Cove Console Commands

 - *backupchalk*: Creates a backup of the current chalk into a timestamped JSON file. Also creates an image of the current chalk into a timestamped PNG file. If you have a Discord webhook URL in chalkmod.json, it will send the PNG to that URL.
 - *loadchalk*: Loads the chalk data from the chalk JSON file into memory. File must be in the Cove server folder. After the filename you can specify a canvas number (0 to 3) to load only that canvas.
    Example:
    ```
    loadchalk chalk_2026-01-10_01-10-19.json 0
    2026-01-10 10:19:38.095 -05:00 [INF] [ChalkMod] loadchalk command attempting to load data from canvas 0 in "chalk_2026-01-10_01-10-19.json"
    2026-01-10 10:19:38.098 -05:00 [INF] [ChalkMod] Chalk data file found. Loading chalk data...
    2026-01-10 10:19:38.347 -05:00 [INF] [ChalkMod] Restored Chalk Data for canvas 0
    ```
 - *clearchalk*: Removes all chalk data from memory, unless you specify a canvas ID (0 to 3), then it will only clear that canvas.
 - *cleanupchalk*: Removes all non-standard canvas data from memory. Non-standard is defined as canvas ID values of less than 0 or more than 3.

## Those canvas numbers

 - The canvas at spawn is 0
 - The canvas in front of the aquarium is 1
 - The canvas at the corner of the map is 2
 - The canvas up from spawn close to the small river is 3

<img width="518" height="503" alt="Webfishing map" src="https://github.com/user-attachments/assets/bdab8edb-31ef-4858-af2c-9d13daa7ae8e" />

(thanks to 'Andres Of Astoria' for the map)

<img width="300" height="300" alt="chalkmod_canvases" src="https://github.com/user-attachments/assets/41aedb66-9ad3-466b-ac64-330130946d0d" />

## Please note
 - the loadchalk, clearchalk and cleanupchalk commands are performed on the chalk data in the running server's memory. They do not edit the current chalk.json backup file from Persistent Chalk. Also, Cove by default only sends chalk data to users when they log on the lobby. I will eventually figure out how to push chalk updates to currently logged in users, but for now it is not being done. Therefore, if you make updates to the chalk data, users would need to leave and come back to the lobby in order to see the changes.
