using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    private static object _lock = new object();
    private static bool _applicationQuit = false;

    protected virtual bool IsDontDestoryed => false;

    public static T Instance
    {
        get
        {
            if(_applicationQuit)
            {
                return null;
            }
            lock( _lock)
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType(typeof(T)) as T;
                    if (_instance == null)
                    {
                        var obj = new GameObject();
                        obj.name = typeof(T).ToString();
                        _instance = obj.AddComponent<T>();
                    }
                    return _instance;
                }
                else
                {
                    return _instance;
                }
            }
        }
    }

    protected void Awake()
    {
        if (_instance == null)
            _instance = this as T;
        if(IsDontDestoryed) DontDestroyOnLoad(this.gameObject);
    }

    protected void OnApplicationQuit()
    {
        _applicationQuit = true;
    }

    public void OnDestroy()
    {
        _applicationQuit = true;
    }
}
