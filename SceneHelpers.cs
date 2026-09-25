using System;
using System.Collections.Generic;
using Systems.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PaApi;

public static class SceneHelpers
{
    public struct CustomSceneGroup(string groupName, string scenePath, Type rootScript, SceneGroupType sceneGroupType)
    {
        public readonly string GroupName = groupName;
        public readonly string ScenePath = scenePath;
        public readonly Type RootScript = rootScript;
        public readonly SceneGroupType SceneGroupType = sceneGroupType;
    }

    internal static List<CustomSceneGroup> GroupsToAdd = new();
    
    /// <summary>
    /// adds a new scene as a scene group, this being what the game usually uses to load new scenes. This should be called in your plugins awake, otherwise it will not work.
    /// if a custom script is not required use the alternate overload of this function
    /// </summary>
    /// <param name="groupName">make this unique, if it overlaps with another mod or the game this will cause issues</param>
    /// <param name="scene">your scene, if loaded from an assed bundle make sure to not unload the scene</param>
    /// <param name="sceneGroupType">purely cosmetic shown on loading screen</param>
    /// <typeparam name="T">asset bundles don't load a custom script, specify your custom script thats gonna be loaded in the first object of the scene</typeparam>
    public static void AddNewSceneGroup<T>(string groupName, string scenePath, SceneGroupType sceneGroupType) where T : MonoBehaviour
    {
        GroupsToAdd.Add(new CustomSceneGroup(groupName, scenePath, typeof(T), sceneGroupType));
    }
    
    /// <summary>
    /// adds a new scene as a scene group, this being what the game usually uses to load new scenes. This should be called in your plugins awake, otherwise it will not work.
    /// if a custom script is required use the alternate overload of this function
    /// </summary>
    /// <param name="groupName">make this unique, if it overlaps with another mod or the game this will cause issues</param>
    /// <param name="scene">your scene, if loaded from an assed bundle make sure to not unload the scene</param>
    /// <param name="sceneGroupType">purely cosmetic shown on loading screen</param>
    public static void AddNewSceneGroup(string groupName, string scenePath, SceneGroupType sceneGroupType)
    {
        GroupsToAdd.Add(new CustomSceneGroup(groupName, scenePath, typeof(MonoBehaviour), sceneGroupType));
    }
}