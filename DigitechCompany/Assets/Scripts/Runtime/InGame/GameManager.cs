using System;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UnityEngine;

public class GameManager : MonoBehaviourPunCallbacks
{
    private ItemManager itemManager;

    [SerializeField] private MeshRenderer[] rooms;

    private void Awake()
    {
        NetworkObject.Instantiate("Prefabs/Player", Vector3.up * 9, Quaternion.identity);
        NetworkObject.Instantiate("Prefabs/TestShovel", new Vector3(1, 8));
    }

    private void Start()
    {
        itemManager = Services.Get<ItemManager>();
        itemManager.SpawnItem(1, rooms.Select(r => r.bounds).ToArray());
    }
}