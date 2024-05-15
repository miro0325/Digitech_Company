using System;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UnityEngine;

public class GameManager : MonoBehaviourPunCallbacks, IService
{
    private ItemManager itemManager;

    [SerializeField] private MeshRenderer[] rooms;

    private void Awake()
    {
        NetworkObject.Instantiate("Prefabs/Player", Vector3.up, Quaternion.identity);
        // NetworkObject.Instantiate("Prefabs/TestShovel", new Vector3(1, 8));
    }

    private void Start()
    {
        itemManager = ServiceLocator.For(this).Get<ItemManager>();
        itemManager.SpawnItem(1, rooms.Select(r => r.bounds).ToArray());
    }
}