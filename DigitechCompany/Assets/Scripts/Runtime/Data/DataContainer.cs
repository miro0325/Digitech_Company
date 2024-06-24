using UnityEngine;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System;
using UnityEngine.Networking;

public class DataContainer : MonoBehaviour, IService
{
    //load
    public SOLoadData loadData;

    //user
    public UserData userData;

    private void Awake()
    {
        ServiceLocator.ForGlobal().Register(this);
    }
}