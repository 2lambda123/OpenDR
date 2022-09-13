[![Travis CI](https://travis-ci.org/drogoganor/DarkReign.svg?branch=master)](https://travis-ci.org/drogoganor/DarkReign/builds#) [![Discord](https://img.shields.io/discord/102860784329052160.svg)](https://discord.gg/3MKcGSW)

# Dark Reign mod for OpenRA

A Dark Reign mod for the [OpenRA](https://github.com/OpenRA/OpenRA) strategy game engine. Created using the [OpenRAModSDK](https://github.com/OpenRA/OpenRAModSDK).

Please note this mod is a work-in-progress. Many features are missing or incomplete at this stage.

You can get the latest release from the [Releases page](https://github.com/drogoganor/DarkReign/releases). If you have trouble installing or would like to compile from source, please see the [Installation page](https://github.com/drogoganor/DarkReign/wiki/Installation).

This mod installs content from the freely-available Dark Reign demo.

Join us on the [Dark Reign discord](https://discord.gg/3MKcGSW) if you want to talk about the mod.

## A note on running the mod in Visual Studio

You will need to switch to OpenRA.Launcher and set the command line arguments to: `Engine.EngineDir=".." Engine.ModSearchPaths="..\\..\\mods" Game.Mod=dr`

Also for some reason you'll need to copy the mod DLL by setting the Post-build event for OpenRA.Mods.Dr to: `xcopy /Y /F $(OutDir)$(AssemblyName).dll ..\engine\bin\`

## Thanks to

* [OpenRA](http://www.openra.net/) and associates
* IceReaper and friends for [KKnD](https://www.kknd-game.com/) and [PixelMagic](https://eiveo.net/pixelmagic.html)
* Nolt and friends for [Shattered Paradise](https://www.moddb.com/mods/shattered-paradise/downloads)
* [darkreign.ws](http://darkreign.ws/), DR community site
* dr_zaphod for readspr.c
* btigi for [drExplorer](https://github.com/btigi/drExplorer)
* My family and friends