using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class GameManager : MonoBehaviourPunCallbacks
{
    private void Awake()
    {
        NetworkObject.Instantiate("Prefabs/Player", Vector3.up * 9, Quaternion.identity);
        NetworkObject.Instantiate("TestShovel", new Vector3(1, 8));
    }
}