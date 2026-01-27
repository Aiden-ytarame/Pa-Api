PaApi aims to add helper functions to make modding Project Arrhythmia easier

Right now its main function is to facilitate calling events and setting up mod settings in the main

The simplest usage is 

```csharp
PaApi.SettingsHelper.RegisterModSettings(ModGuid, ModName, Color.red, ConfigFile, builder =>
        {
            builder.Label("<b>YOUR LABEL</b> - generic settings");
        
            builder.Toggle("Your Toggle", ConfigFile.Bind(...));
            builder.Slider("Your Value Slider", ConfigFile.Bind(...), UI_Slider.VisualType.line, "35%", "50%", "85%");
            builder.Spacer();
        
            builder.Label("<b>MISCELLANEOUS</b> - other settings");
        
            builder.Slider("Your Range Slider", ConfigFile.Bind(...), UI_Slider.VisualType.dot, new Vector2(0, 5));
            builder.Toggle("Another Toggle", ConfigFile.Bind(...));
            builder.EndPage(); //optional
        });
```

Which should be done only once in your plugin initialization


# Installation

### for developers

* Download the PaApi.dll from the latest github release and reference it in your plugin's project
* Put *[BepInDependency("me.ytarane.PaApi")] attribute in your plugin
* Remember to set aiden_ytarame-PaApi-1.0.0 as a dependency in your manifest if you're uploading to thunderstore
  
### r2modman (recommended)

* Download the [r2modman](https://thunderstore.io/package/ebkr/r2modman/) mod manager.
* Search for **Project Arrhythmia** and go the **Online** tab, look for **PaApi** by aiden_ytarame and click download.
* Then click **Start Modded** in the top left of r2modman and youre all set.

### Manual
* Download bepinex from [BepInEx 5.4](https://thunderstore.io/c/project-arrhythmia/p/BepInEx/BepInExPack/).
* Extract the files from the BepInEx zip into your game's exe folder (this can be found by right clicking PA on your steam library and going "manage->browse local files").
  * Note: Extract the zip contents into the PA folder like the image below, not a new folder.
  
<img src="https://github.com/user-attachments/assets/7d57b1cf-39cf-4441-8841-e23ad77ed3e6" width="50%">

* Download the [PaApi](https://thunderstore.io/c/project-arrhythmia/p/aiden_ytarame/PaApi/) zip from latest release available on thunderstore.
* In your game's folder there will be a new folder called "bepinex". Merge the Bepinex folder on the PaApi zip file with the one in your game's folder.
* Open the game and enjoy!


