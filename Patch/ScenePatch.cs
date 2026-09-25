using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Eflatun.SceneReference;
using HarmonyLib;
using Systems.SceneManagement;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.SceneManagement;

namespace PaApi.Patch;

[HarmonyPatch(typeof(SceneLoader))]
internal static class ScenePatch
{
    private static bool _initializded = false;

    [HarmonyPatch(nameof(SceneLoader.Start))]
    [HarmonyPostfix]
    private static void Start()
    {
        if (_initializded)
        {
            return;
        }

        _initializded = true;
        
        List<SceneGroup> groups = new(SceneLoader.Inst.sceneGroups);

        foreach (var customSceneGroup in SceneHelpers.GroupsToAdd)
        {
            string scenePath = customSceneGroup.ScenePath;
            
            byte[] bytes = new byte[16];
            var text = Encoding.UTF8.GetBytes(scenePath);
            
            new Span<byte>(text, 0, Math.Min(16, text.Length)).CopyTo(bytes);
            
            string guid = new Guid(bytes).ToString().Replace("-", "");
            
            SceneGroup sceneGroup = new()
            {
                GroupName = customSceneGroup.GroupName,
                SceneTitle = new LocalizedString(),
                SceneDescription =  new LocalizedString(),
                GroupType = customSceneGroup.SceneGroupType,
                Scenes = [new SceneData()
                {
                    SceneType = SceneType.ACTIVE,
                    Reference = new SceneReference()
                    {
                        guid = guid
                    }
                }]
            };
            
            //load scene group makes use of these
            SceneGuidToPathMapProvider._sceneGuidToPathMap.Add(guid, scenePath);
            SceneGuidToPathMapProvider._scenePathToGuidMap.Add(scenePath, guid);
            
            groups.Add(sceneGroup);
        }
        
        SceneLoader.Inst.sceneGroups = groups.ToArray();
        
        SceneManager.sceneLoaded += (scene, _) =>
        {
            //asset bundles dont load custom scripts, we have to add it here on scene load
            //terrible ass code jesus 
            foreach (var customSceneGroup in SceneHelpers.GroupsToAdd)
            {
                if (customSceneGroup.ScenePath == scene.path)
                {
                    if (customSceneGroup.RootScript != typeof(MonoBehaviour))
                    {
                        scene.GetRootGameObjects()[0].AddComponent(customSceneGroup.RootScript);
                    }
                    return;
                }
            }
        };
    }
}

[HarmonyPatch(typeof(SceneReference))]
internal static class SceneReferencePatch
{
    [HarmonyPatch(nameof(SceneReference.State), MethodType.Getter)]
    [HarmonyPrefix]
    static bool GetStatePatch(SceneReference __instance, ref SceneReferenceState __result)
    {
        __result = SceneReferenceState.Unsafe;
        if (__instance.HasValue)
        {
            if (SceneGuidToPathMapProvider.SceneGuidToPathMap.TryGetValue(__instance.Guid, out var path))
            {
                __result = SceneReferenceState.Regular;
            }
        }
        return false;
    }
}