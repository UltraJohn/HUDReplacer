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
    onScene = SPACECENTER
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
* <strong>onScene (optional)</strong> <em>\<int></em>
  ```
  Specify a scene to only load this directory in. Useful if you want to change the visuals of the same texture depending on scene. Priority is still taken into consideration for this.
  Valid options are: LOADING, LOADINGBUFFER, MAINMENU, SETTINGS, CREDITS, SPACECENTER, EDITOR, FLIGHT, TRACKSTATION, PSYSTEM, MISSIONBUILDER
  ```
  NOTE: It is recommended to use a directory named PluginData as your root folder for textures, as it will let KSP know not to load it into its GameDatabase. Since HUDReplacer loads textures directly from disk, there is no reason to also load it into the database, which just makes it load every texture twice, wasting memory.

There is also the option for recoloring some specific UI elements, which are not colored through the texture itself, but rather the code. This is due to some elements changing color depending on whats currently happening, such as the top left clock that changes text color depending on the games lag/performance. Below is an example config for all the possible recolor options.

```
HUDReplacerRecolor:NEEDS[HUDReplacer]
{
	priority = 1
	tumblerColorPositive = 1,1,1,1
	tumblerColorNegative = 1,1,1,1
	PAWTitleBar = 1,1,1,1
	PAWBlueButton = 1,1,1,1
	PAWBlueButtonToggle = 1,1,1,1
	PAWVariantSelectorNext = 1,1,1,1
	PAWVariantSelectorPrevious = 1,1,1,1
	PAWResourcePriorityIncrease = 1,1,1,1
	PAWResourcePriorityDecrease = 1,1,1,1
	PAWResourcePriorityReset = 1,1,1,1
	PAWFuelSliderColor = 1,1,1,1
	PAWFuelSliderTextColor = 1,1,1,1
	KALTitleBar = 1,1,1,1
	gaugeNeedleYawPitchRoll = 1,1,1,1
	gaugeNeedleYawPitchRollPrecision = 1,1,1,1
	METDisplayColorRed = 1,1,1,1
	METDisplayColorYellow = 1,1,1,1
	METDisplayColorGreen = 1,1,1,1
	speedDisplayColorText = 1,1,1,1
	navBallHeadingColor = 1,1,1,1
	speedDisplayColorSpeed = 1,1,1,1
	stageTotalDeltaVColor = 1,1,1,1
	stageGroupDeltaVTextColor = 1,1,1,1
	stageGroupDeltaVNumberColor = 1,1,1,1
	stageGroupDeltaVBackgroundColor = 1,1,1,1
	navballCursor = 1,1,1,1
	verticalSpeedGaugeNeedleColor = 1,1,1,1
	maneuverNodeEditorTextColor = 1,1,1,1
	stageEngineFuelGaugeTextColor = 1,1,1,1
	stageEngineHeatGaugeTextColor = 1,1,1,1
	stageEngineFuelGaugeBackgroundColor = 1,1,1,1
	stageEngineHeatGaugeBackgroundColor = 1,1,1,1
	stageEngineFuelGaugeFillColor = 1,1,1,1
	stageEngineHeatGaugeFillColor = 1,1,1,1
	stageEngineFuelGaugeFillBackgroundColor = 1,1,1,1
	stageEngineHeatGaugeFillBackgroundColor = 1,1,1,1
	SASDisplayOnColor = 1,1,1,1
	SASDisplayOffColor = 1,1,1,1
	RCSDisplayOnColor = 1,1,1,1
	RCSDisplayOffColor = 1,1,1,1	
	EditorCategoryButtonColor = 1,1,1,1
	EditorCategoryModuleButtonColor = 1,1,1,1
	EditorCategoryResourceButtonColor = 1,1,1,1
	EditorCategoryManufacturerButtonColor = 1,1,1,1
	EditorCategoryTechButtonColor = 1,1,1,1
	EditorCategoryProfileButtonColor = 1,1,1,1
	EditorCategorySubassemblyButtonColor = 1,1,1,1
	EditorCategoryVariantsButtonColor = 1,1,1,1
	EditorCategoryCustomButtonColor = 1,1,1,1
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
