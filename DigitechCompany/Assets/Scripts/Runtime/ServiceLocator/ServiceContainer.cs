using System;
using System.Collections.Generic;
using UnityEngine;

public class ServiceContainer : MonoBehaviour
{
    private bool dontDestroyOnLoad;
    private Dictionary<Type, IService> services = new();

    internal void Initialize(bool dontDestroyOnLoad)
    {
        this.dontDestroyOnLoad = dontDestroyOnLoad;
    }

    public void Unregister<TService>() where TService : class, IService
    {
        services.Remove(typeof(TService));
    }

    public void Register<TService>(TService instance) where TService : class, IService
    {
        if (services.TryAdd(typeof(TService), instance))
        {
            if(dontDestroyOnLoad && instance is MonoBehaviour)
                (instance as MonoBehaviour).transform.SetParent(transform);
            return;
        }

        Debug.LogWarning($"ServiceContainer: Service of type {typeof(TService).FullName} already in service");
        if (instance is MonoBehaviour) Destroy(instance as MonoBehaviour);
    }

    public bool TryRegister<TService>(TService instance) where TService : class, IService
    {
        if (services.TryAdd(typeof(TService), instance))
        {
            if(dontDestroyOnLoad && instance is MonoBehaviour)
                DontDestroyOnLoad(instance as MonoBehaviour);
            return true;
        }
        Debug.LogWarning($"ServiceContainer: Service of type {typeof(TService).FullName} already in service");
        if (instance is MonoBehaviour) Destroy(instance as MonoBehaviour);
        return false;
    }

    public TService Get<TService>() where TService : class, IService
    {
        if (services.TryGetValue(typeof(TService), out var service))
            return service as TService;

        Debug.LogWarning($"ServiceContainer: Service of type {typeof(TService).FullName} not in container");
        return null;
    }

    public bool TryGet<TService>(out IService service) where TService : class, IService
    {
        if (services.TryGetValue(typeof(TService), out service))
            return true;

        Debug.LogWarning($"ServiceContainer: Service of type {typeof(TService).FullName} not in container");
        return false;
    }
}