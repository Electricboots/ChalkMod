<h1>ChalkMod</h1>
<img width="400" height="400" alt="chalk-2025-11-08  6-45-13 02" src="https://github.com/user-attachments/assets/e973c236-69d8-4eb4-87bd-9ad95f563b94" />

<p>A chalk management/moderation plugin for <a href="https://github.com/DrMeepso/WebFishingCove" target="_blank" rel="noopener">Webfishing Cove</a> dedicated server for the <a href="https://store.steampowered.com/app/3146520/WEBFISHING/" target="_blank" rel="noopener">Webfishing</a> game.</p>
<p>If you look at the source code, you might think "Did a monkey write this?" This is very insulting. I am the equivalent of at least 10 monkeys. 🐵 (Seriously though, I barely know what I'm doing...)</p>
<h2>Installation</h2>
<ul>
<li>All files go into your Cove plugins folder, except for <i>chalkmod.json</i> which goes into your main Cove folder.</li>
<li><i>chalkmod.json</i> is the configuration file for the plugin.</li>
  <ul>
    <li><i>webhookuser</i>: it is the name used in Discord for posting the chalk.</li>
    <li><i>webhookurl</i>: put your Discord webhook URL in here so that an image of the chalk is posted on your Discord every time a backup is made. Leave blank (ie: empty quotes) if you don't want to post anything to Discord.</li>
    <li><i>checkseconds</i>: it is used to autobackup the chalk file if it has been updated at some point during the last [<i>checkseconds</i>] seconds (default 300 seconds)</li>
    <li><i>palette</i>: it is the color palette info used when converting the chalk data to a PNG file. <u>Leave this one alone</u>, unless you really know what kind of effect your changes would have, or if you like weird colors that don't make sense I guess.</li>
  </ul>
</ul>
<h2>Cove Console Commands</h2>
<ul>
<li><i>backupchalk</i>: Creates a backup of the current chalk into a timestamped JSON file. Also creates an image of the current chalk into a timestamped PNG file. If you have a Discord webhook URL in chalkmod.json, it will send the PNG to that URL.</li>
<li><i>loadchalk</i>: Loads the chalk data from the chalk JSON file into memory. File must be in the Cove server folder. After the filename you can specify a canvas number (0 to 3) to load only that canvas.</li>
<li><i>clearchalk</i>: Removes all chalk data from memory, unless you specify a canvas ID (0 to 3), then it will only clear that canvas.</li>
<li><i>cleanupchalk</i>: Removes all non-standard canvas data from memory. Non-standard is defined as canvas ID values of less than 0 or more than 3.</li>
</ul>
