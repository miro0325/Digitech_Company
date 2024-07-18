using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ServiceLocator
{
    private static ServiceContainer globalContainer;
    private static Dictionary<Scene, ServiceContainer> sceneContainers = new();

    public static ServiceContainer Global
    {
        get
        {
            if(!globalContainer)
            {
                globalContainer = new GameObject("Service Container [Global]").AddComponent<ServiceContainer>();
                globalContainer.Initialize(true);
                GameObject.DontDestroyOnLoad(globalContainer);
            }
            return globalContainer;
        }
    }

    public static ServiceContainer ForGlobal()
        => Global;
    
    public static ServiceContainer For(MonoBehaviour mb)
        => ForSceneOf(mb.gameObject.scene.name);

    public static ServiceContainer ForActiveScene()
        => ForSceneOf(SceneManager.GetActiveScene().name);

    public static ServiceContainer ForSceneOf(string name)
    {
        var scene = SceneManager.GetSceneByName(name);
        if(!sceneContainers.TryGetValue(scene, out var container))
        {
            container = new GameObject($"Service Container [Scene]").AddComponent<ServiceContainer>();
            container.Initialize(false);
            SceneManager.MoveGameObjectToScene(container.gameObject, scene);
            sceneContainers.Add(scene, container);
        }
        return container;
    }

    public static TService GetEveryWhere<TService>() where TService : class, IService
    {
        if(Global.TryGet<TService>(out var fromGlobal))
            return fromGlobal as TService;
        
        foreach(var services in sceneContainers)
            if(services.Value.TryGet<TService>(out var fromScene))
                return fromScene as TService;

        return null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        SceneManager.sceneUnloaded += scene =>
        {
            if(sceneContainers.TryGetValue(scene, out _))
                sceneContainers.Remove(scene);
        };
    }
}