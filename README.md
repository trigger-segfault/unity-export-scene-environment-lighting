# Unity Exportable Skybox Settings
Exportable settings for **Window** &gt; **Rendering** &gt; **Lighting**'s **Environment** tab. AKA **Skybox** settings. Settings can be copy and pasted between scenes, or exported to and imported from asset files. Importing or exporting from the scene can **only** be done with the currently-loaded scene. Undo is supported for changes made to the scene or assets.

**Disclaimer:** Designed for Unity 2022.3.22f1 (for VRChat), so it's possible this may not work with newer or older Unity versions, as it relies on some reflection for properties with no API access.

Note that **Copy Values**/**Paste Values** menu items do not affect the real clipboard. For example, copying an asset file in the Project window will not change the clipboard made from the **Copy Values** menu items. The clipboard is stored in a special file located in the same folder as the script file.

Exported settings *can* be edited in the Inspector, but it's recommended to edit them in the scene itself instead.

## How To Use
Place the files `SceneSkyboxSettings.cs` **and** `SceneSkyboxSettings.cs.meta` into your Unity project in `Assets/Editor/` (or under any `Editor/` folder). The `.meta` file is only required in order to allow reusing settings between projects, or other users of this tool.

Settings can be exported to an asset by right clicking in a folder in the Project window and going to **Assets** &gt; **Trigger Segfault** &gt; **Scene Skybox Settings** &gt; **Export new from Scene**. The scene must be loaded to export from it.

Settings can be copied between scenes by using the menu items under **Tools** &gt; **Trigger Segfault** &gt; **Scene Skybox Settings**. The scene must be loaded to copy or paste from it.

## Menus

### Tools &gt; Trigger Segfault &gt; Scene Skybox Settings
This menu is for copying or pasting settings to and from the currently-loaded scene.
* **Copy Values from Scene:** Copy settings from the scene into a special clipboard file.
* **Paste Values to Scene:** Paste settings into the scene from the special clipboard file.

### Assets &gt; Trigger Segfault &gt; Scene Skybox Settings
This menu is for managing settings exported to asset files.

* **Import to Scene:** Import settings into the scene from the selected asset.
* **Export new from Scene:** Create a new asset and export scene settings into it.
* **Overwrite from Scene:** Export settings from the scene into the selected asset.
* **Copy Values:** Copy settings from the selected asset into a special clipboard file.
* **Paste Values:** Paste settings into the selected asset from the special clipboard file.


## Image Previews



*(Tools menu)*<br>
<img width="705" height="132" alt="Tools menu" src="https://github.com/user-attachments/assets/d7fc39c8-04c3-4be1-b1ab-d6056aaa9622" />

***

*(Assets menu)*<br>
<img width="865" height="200" alt="Assets menu" src="https://github.com/user-attachments/assets/284934c7-fffc-41a0-9920-89b6dfcb8175" />

***

*(Asset Inspector)*<br>
<img width="531" height="654" alt="Asset inspector" src="https://github.com/user-attachments/assets/4cf75819-7fa3-4d70-a404-c179f65ca304" />
