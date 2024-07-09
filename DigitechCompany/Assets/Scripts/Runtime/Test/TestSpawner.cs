using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class TestSpawner : MonoBehaviourPun, IService
{
    [SerializeField] Transform[] waypoints;
    [SerializeField] List<string> monsters = new List<string>();
    
    public void SpawnMonsters()
    {
        foreach (string key in monsters)
        {
            var m = NetworkObject.Instantiate($"Prefabs/Monsters/{key}", new Vector3(-4.3261f, 0.4100053f, 16.69641f), Quaternion.identity) as MonsterBase;
            m.Inititalize(waypoints);
        }
    }
    
    private void Awake()
    {
        ServiceLocator.For(this).Register(this);
    }
    
    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
