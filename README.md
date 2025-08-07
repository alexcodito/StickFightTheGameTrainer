<div align="center">
    <img src="https://cdn2.steamgriddb.com/file/sgdb-cdn/logo_thumb/a667f4e7b0c8a3babe331569d3eac6bd.png" width="256" alt="Logo"> 
    <br>
    <h3>Stick Fight: The Game [+12 Trainer]</h1>
    A <b>trainer/mod</b> for Stick Fight: The Game that adds new online and local features.
</div>


<div align="center">    
    <br> 
    <a href="https://www.moddb.com/mods/stick-fight-online-trainer-loxa" title="View Stick Fight +12 Online Trainer [loxa] on Mod DB" target="_blank"><img src="https://button.moddb.com/popularity/medium/mods/35572.png" alt="Stick Fight +12 Online Trainer [loxa]" /></a>    <a href="https://www.moddb.com/mods/stick-fight-online-trainer-loxa/downloads/stick-fight-online-trainer-loxa" title="Download Stick Fight +12 Online Trainer (v1.0.0.2) [loxa] - Mod DB" target="_blank"><img src="https://button.moddb.com/download/medium/132847.png" alt="Stick Fight +12 Online Trainer (v1.0.0.2) [loxa]" /></a>
    <br> 
    <br> 
<img src="https://media.moddb.com/images/mods/1/36/35572/Bot_Mayhem_2.png" alt="Bot Mayhem">
</div>

<br>

## Overview

Injecting IL code directly into the game's Assembly-CSharp.dll file allows this trainer to work accross all versions, including future updates. The intention is to have a maintenance-free trainer that does not need to reflect changes that are made to every update of the game. 

#### Features

- No recoil
- Full auto
- Scoreboard
- Flying mode
- Skip map/boss
- Display health
- Uncap fire-rate
- Unlimited health
- Unlimited ammunition
- Spawn weapon on demand (extra weapons)
- Browse and select weapons (extra weapons)
- Spawn different types of AI Bots (new Single Player mode)

Now supports playing online in Public Mode (cheats off), and with Modders-only Mode (cheats on).

## Project Building

The `Build Action` property is set to `None` for the following source code files.

- `/Trainer/TrainerLogic/Singleton.cs` (Generic singleton to allow reusing instances of TrainerManager and TrainerOptions)
- `/Trainer/TrainerLogic/TrainerManager.cs` (Contains most of the logic and UI)
- `/Trainer/TrainerLogic/TrainerOptions.cs` (Basic settings store)

These files are compiled at run-time during the trainer patching process using `System.CodeDom.Compiler.CodeDomProvider`. Several of the game's own reference DLLs are expected to be discovered in the 
same directory as the user-supplied target `Assembly-CSharp.dll` file, including `UnityEngine` and `TextMeshPro`. Important to note that the output binary is built with .NET Framework 3.5 to match the game's build.

Following successful compilation, the produced binary is immediately reloaded. Finally, the newly compiled `TypeDef`s are injected into the game's target `Assembly-CSharp.dll` file.

As a basic tampering prevention measure, the raw source files are encrypted with the use of AES 256, and are only momentarily decrypted at run-time for the aforementioned compilation step. 

A utility is available when the VS project is built in `Debug` mode to assist with this task. Pressing the `Dev. Options` button brings up the `Generate encrypted Logic Module sources & keys` option. 
This will load the three relevant `.cs` files and generate correspondingly named `.data` files, along with two additional files called `iv.data` and `key.data`.

![Dev. Options](Reference/Media/sf-trainer-dev-options.png "Dev. Options")

Finally, the generated data must be *hard coded* within the `StickFightTheGameTrainer.Trainer.TrainerLogic.TrainerLogicModule` static class. While this is far from ideal,
it prevents script-kiddies from enabling cheats in online public lobbies.

Please note that when installing the patch in `Release` mode, the target assembly is additionally obfuscated with the use of ConfuserEx.

This repository uses a forked version of [ConfuserEx](https://github.com/alexcodito/ConfuserEx/tree/master) that includes support for loading binaries from raw bytes instead of file paths.

The /lib directory contains the required DLLs for both ConfuserEx and dnSpy. These are necessary to successfully build the solution.  

## FAQ
[Why make it open source if you're just going to encrypt the code?](https://github.com/alexcodito/StickFightTheGameTrainer/issues/2)

## Installation

<a href="https://www.moddb.com/mods/stick-fight-online-trainer-loxa/videos/how-to-install-and-uninstall-stick-fight-the-game-trainer-by-loxa">How to install and uninstall - Watch on Mod DB</a>

1) Unzip / extract the downloaded .zip file.
2) Run the extracted trainer .exe program.
3) Specify the path to your Stick Fight The Game installation folder.
- Most common Steam location: `C:\Program Files (x86)\Steam\steamapps\common\StickFightTheGame\StickFight_Data\Managed`
4) Press `Install Mod` button.

## Controls
##### PC Keyboard

    - Toggle Trainer Menu: [SHIFT] + [M]
    - Skip Map: [SHIFT] + [S]
    - Spawn Random Weapon: [R] or [P]
    - Browse Weapons: [Q] for previous or [E] for next

##### Xbox 360 Controller

    - Toggle Trainer Menu: [RB] + [A]
    - Skip Map: [RB] + [B]
    - Spawn Random Weapon: [DPadUp] or [DPadDown]
    - Browse Weapons: [DPadLeft] or [DPadRight]

## Notes

    - The scoreboard displays wins (i.e. how many rounds you wear the crown for).
    - When you skip a level, no one wins the round.
    - This mod will only activate if you're playing locally or with other players that have it installed.
    - HP bars always display 100, even if your health is set to 300. Damage is scaled relatively.

## Media

#### Videos
<a href="https://www.moddb.com/mods/stick-fight-online-trainer-loxa/videos/1v3-bots-stick-fight-the-game-trainer-by-loxa">1v3 Bots - Stick Fight The Game Trainer by loxa - Mod DB</a>
<br>
<a href="https://www.moddb.com/mods/stick-fight-online-trainer-loxa/videos/4x-bots-max-stats-stick-fight-the-game-trainer-by-loxa">4x Bots Max Stats - Stick Fight The Game Trainer by loxa - Mod DB</a>

#### Screenshots
<img src="https://media.moddb.com/cache/images/mods/1/36/35572/thumb_620x2000/Trainer_Menu.png" alt="Bot Mayhem">
<img src="https://media.moddb.com/images/mods/1/36/35572/Bot_Mayhem.png" alt="Bot Mayhem">
<br>

## Release Information

##### Version 1.0.0.3 (Game version v1.2.05 and above)
- Improved AI logic.
- Added easter eggs.
- Hold weapon selection shortcuts to browse faster.
- Play online with friends who don't have the trainer installed.
- Improved anti-cheating protection for public lobbies.
- Some features are now optional if they happen to regress due to updates to the game.

##### Version 1.0.0.2 (Game version v1.2.05 and above)
- Added AI Bots / Single Player mode.
- Improved shortcuts.
- Spawn weapons as presents.
- Bug fixes and major performance improvements.

##### Version 1.0.0.1 (for all versions of the game)
- Added support for newer versions of the game (no regression in compatibility).
- Minor bug fixes and improvements.

##### Version 1.0.0.0 (for all versions of the game)
- Automated installer for all versions of the game.
- Bug fixes.

##### Build 05 (Game version v1.2.04) - *Not Included*
- Bug fixes.
- Added unlimited health.

##### Build 04 (Game version v1.2.03 LET v1.1) - *Not Included*
- Added an FPS counter.
- Online mode is now available.
- Other general bug fixes and improvements.

##### Build 03 (Game version v1.2.03) - *Not Included*
- Online mode is now restored, although still without the trainer itself.
- General bug fixes and improvements.
- HP bars & scoreboard no longer disappear when a player dies.

##### Build 02 (Game version v1.2.0) - *Not Included*
- Corrected a bug where pressing RB would sometimes cause the player's stick fighter to fly.
