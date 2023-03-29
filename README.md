# HUDReplacer (WIP)

This is a framework for Kerbal Space Program that allows you to replace HUD/UI textures at runtime.

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

## Example texture modding
Let's say we want to replace the texture for the Admin building in the KSC sidebar:

![KSC 1](https://i.imgur.com/KwzfnZN.png)

If we install the texture pack development version of HUDReplacer, we can go ingame and open the console, then press D on the keyboard to log the name of the texture that the mouse is currently over.
In this case, we should get the result:

`Image.mainTexture.name: Buttons_Admin - WxH=256x256`

This tells us that the texture for the Admin building icon, is called `Buttons_Admin` and the dimensions of the file is 256x256.
Now we can go ahead and create our own texture, or possibly just recolor the original texture.

We make sure to save our new texture as: `Buttons_Admin.png` and place it in our folder containing the new textures that we specified in the config file.
Start up the game and you should see the icon is now our new texture!

In the case that a texture has multiple versions that have the same name but different sizes (e.g. "rect_round_dark"), we must ensure that we append the dimensions of the texture to the filename, like this:
`rect_round_dark#64x64.png` or `rect_round_dark#69x69.png` otherwise, we will be replacing the wrong size texture and that will scale incorrectly.

Make sure to install the regular HUDReplacer version when you're done creating a texture pack as the developer version is not meant to be used while playing.
