using System;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UnityEngine;

public class GameManager : MonoBehaviourPunCallbacks, IService
{
    //inspector
    [SerializeField] private Transform basement;
    [SerializeField] private MeshRenderer[] rooms;

    //field
    private bool isGameStart;

    private void Awake()
    {
        ServiceLocator.For(this).Register(this);
    }

    public void SpawnPlayer()
    {
        NetworkObject.InstantiateBuffered("Prefabs/Player", basement.position + Vector3.up * 2, Quaternion.identity);
    }
}