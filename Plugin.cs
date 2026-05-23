using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Localization.Settings;

namespace PaApi;

[BepInPlugin(Guid, Name, Version)]
internal class Plugin : BaseUnityPlugin
{
    internal new static ManualLogSource Logger;
    Harmony _harmony;
    public const string Guid = "me.ytarame.PaApi";
    const string Name = "PaApi";
    const string Version = "1.0.2";


    private void Awake()
    {
        Logger = base.Logger;
        
        _harmony = new Harmony(Guid);
        _harmony.PatchAll();
        
        LocalizationSettings.StringDatabase.TableProvider = new PaApiTableProvider();
        
        // Plugin startup logic
        Logger.LogInfo($"Plugin {Guid} is loaded!");
    }
    
}
