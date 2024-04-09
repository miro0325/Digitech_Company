using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game
{
    public class ServiceProvider : MonoBehaviour
    {
        private static ServiceProvider _instance;

        public static bool Register<T>(T instance, bool isGlobal = false) where T : MonoBehaviour
        {
            if (_instance == null)
            {
                _instance = new GameObject("[ServiceProvider]").AddComponent<ServiceProvider>();
                DontDestroyOnLoad(_instance.gameObject);
                SceneManager.sceneUnloaded += _ =>
                {
                    foreach (var context in new Dictionary<string, MonoBehaviour>(_instance.sceneContext))
                    {
                        if (context.Value == null)
                            _instance.sceneContext.Remove(context.Key);
                    }
                };
            }

            if (isGlobal)
            {
                if (!_instance.globalContext.ContainsKey(typeof(T).FullName))
                {
                    _instance.globalContext.Add(typeof(T).FullName, instance);
                    DontDestroyOnLoad(instance.gameObject);
                    return true;
                }
                else
                {
                    Debug.LogWarning($"{typeof(T).FullName} type already exist in global context.");
                    Destroy(instance.gameObject);
                    return false;
                }
            }
            else
            {
                if (!_instance.sceneContext.ContainsKey(typeof(T).FullName))
                {
                    _instance.sceneContext.Add(typeof(T).FullName, instance);
                    return true;
                }
                else
                {
                    Debug.LogWarning($"{typeof(T).FullName} type already exist in scene context.");
                    Destroy(instance.gameObject);
                    return false;
                }
            }
        }

        public static T Get<T>() where T : MonoBehaviour
        {
            if (_instance.sceneContext.ContainsKey(typeof(T).FullName))
            {
                T inst = _instance.sceneContext[typeof(T).FullName] as T;
                return inst;
            }
            if (_instance.globalContext.ContainsKey(typeof(T).FullName))
            {
                T inst = _instance.globalContext[typeof(T).FullName] as T;
                return inst;
            }
            return null;
        }

        private readonly Dictionary<string, MonoBehaviour> globalContext = new();
        private readonly Dictionary<string, MonoBehaviour> sceneContext = new();
    }
}