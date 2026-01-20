using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace PaApi;

[BepInPlugin(Guid, Name, Version)]
[BepInProcess("Project Arrhythmia.exe")]
internal class Plugin : BaseUnityPlugin
{
    internal new static ManualLogSource Logger;
    Harmony _harmony;
    const string Guid = "me.ytarame.PaApi";
    const string Name = "PaApi";
    const string Version = "1.0.0";


    private void Awake()
    {
        Logger = base.Logger;
        
        _harmony = new Harmony(Guid);
        _harmony.PatchAll();

        // Plugin startup logic
        Logger.LogInfo($"Plugin {Guid} is loaded!");
    }
    
}
