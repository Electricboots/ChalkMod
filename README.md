# ChalkMod
<img width="400" height="400" alt="chalk-2025-11-08  6-45-13 02" src="https://github.com/user-attachments/assets/e973c236-69d8-4eb4-87bd-9ad95f563b94" />

A chalk management/moderation plugin for [Webfishing Cove](https://github.com/DrMeepso/WebFishingCove) dedicated server for the [Webfishing](https://store.steampowered.com/app/3146520/WEBFISHING/) game.
If you look at the source code, you might think "Did a monkey write this?" This is very insulting. I am the equivalent of at least 10 monkeys. 🐵 (Seriously though, I barely know what I'm doing...)

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
 - *clearchalk*: Removes all chalk data from memory, unless you specify a canvas ID (0 to 3), then it will only clear that canvas.
 - *cleanupchalk*: Removes all non-standard canvas data from memory. Non-standard is defined as canvas ID values of less than 0 or more than 3.
