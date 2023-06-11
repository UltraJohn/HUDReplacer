# HUDReplacer (WIP)

This is a framework for Kerbal Space Program that allows you to replace HUD/UI textures at runtime.

## Requirements
[Module Manager](https://forum.kerbalspaceprogram.com/index.php?/topic/50533-*)

[HarmonyKSP](https://github.com/KSPModdingLibs/HarmonyKSP)


## How to create texture packs
Creating texture packs is quite simple. All you need is a folder containing your new textures and a single config file with the following:
```
HUDReplacer:NEEDS[HUDReplacer]
{
    filePath = GameData/HUDReplacer/PluginData/
    priority = 0
}
```
* <strong>filePath</strong> <em>\<string></em>
  ```
  The path to your texture folder. Files must be in PNG format.
  ```
* <strong>priority</strong> <em>\<int></em>
  ```
  Priority of your texture pack in case of duplicate files. Higher number will take priority over other texture packs.
  ```

There is also the option for recoloring some specific UI elements, which are not colored through the texture itself, but rather the code. This is due to some elements changing color depending on whats currently happening, such as the top left clock that changes text color depending on the games lag/performance. Below is an example config for all the possible recolor options.

```
HUDReplacerRecolor:NEEDS[HUDReplacer]
{
	priority = 1
	tumblerColorPositive = 0,1,0,1 // RGBA values from 0-1
	tumblerColorNegative = 0,0,1,1
	PAWTitleBar = 0,0,1,1
	KALTitleBar = 0,1,1,1
	gaugeNeedleYawPitchRoll = 1,0,0,1
	gaugeNeedleYawPitchRollPrecision = 0,1,0,1
	METDisplayColorRed = 0,0,1,1
	METDisplayColorYellow = 1,0,1,1
	METDisplayColorGreen = 0,1,1,1
	speedDisplayColorText = 1,0,0,1
	speedDisplayColorSpeed = 1,1,0,1
	navBallHeadingColor = 0,1,1,1
	stageTotalDeltaVColor = 1,0,0,1
	stageGroupDeltaVTextColor = 0,1,0,1
	stageGroupDeltaVNumberColor = 0,0,1,1
	navballCursor = 1,0,0,1
}
```

## Example texture modding
Let's say we want to replace the texture for the Admin building in the KSC sidebar:

![KSC 1](https://i.imgur.com/KwzfnZN.png)

To enable the developer features, go into GameData/HUDReplacer/Settings.cfg and change the setting **showDebugToolbar** from **false** to **true**.
After this, we can go ingame and click the HUDReplacer toolbar icon to enable debug mode. Now we can open the console and press D on the keyboard to log the name of the texture that the mouse is currently over.
In this case, we should get the result:

`Image.mainTexture.name: Buttons_Admin - WxH=256x256`

This tells us that the texture for the Admin building icon, is called `Buttons_Admin` and the dimensions of the file is 256x256.
Now we can go ahead and create our own texture, or possibly just recolor the original texture.

We make sure to save our new texture as: `Buttons_Admin.png` and place it in our folder containing the new textures that we specified in the config file.
Start up the game and you should see the icon is now our new texture!

In the case of a texture which has multiple versions that have the same name but different sizes (e.g. "rect_round_dark"), we must ensure that we append the dimensions of the texture to the filename, like this:
`rect_round_dark#64x64.png` or `rect_round_dark#69x69.png` otherwise, we will be replacing the wrong size texture and that will scale incorrectly.

# Debug mode keybindings
* <strong>Q:</strong> <em>Reload all replaced textures</em>
* <strong>E:</strong> <em>Dump all textures in scene to ksp.log</em>
* <strong>D:</strong> <em>Get all textures currently over mouse</em>
