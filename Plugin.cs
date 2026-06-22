using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;

namespace PaApi;

[BepInPlugin(Guid, Name, Version)]
internal class Plugin : BaseUnityPlugin
{
    internal new static ManualLogSource Logger;
    Harmony _harmony;
    public const string Guid = "me.ytarame.PaApi";
    const string Name = "PaApi";
    const string Version = "1.0.4";


    private void Awake()
    {
        Logger = base.Logger;
        
        _harmony = new Harmony(Guid);
        _harmony.PatchAll();

        SceneManager.sceneLoaded += (scene, mode) =>
        {
            if (scene.name == "Menu")
            {
                SettingsHelper.SetupMenu();
            }
        };
        LocalizationSettings.StringDatabase.TableProvider = new PaApiTableProvider();
        
        // Plugin startup logic
        Logger.LogInfo($"Plugin {Guid} is loaded!");
    }
    
}
