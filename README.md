# HUDReplacer (WIP)

Allows you to replace HUD/UI textures at runtime.


## Config example:

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
