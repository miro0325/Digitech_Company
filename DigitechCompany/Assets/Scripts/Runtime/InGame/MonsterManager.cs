using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;

public class MonsterManager : MonoBehaviourPun, IService
{
    [SerializeField] Transform[] waypoints;
    [SerializeField] List<string> monsters = new List<string>();
    public List<MonsterBase> curMonsters = new();
    
    public void SpawnMonsters()
    {
        foreach (var monster in curMonsters)
        {
            NetworkObject.Destory(monster.photonView.ViewID);
        }
        curMonsters.Clear();
        foreach (string key in monsters)
        {
            var m = NetworkObject.Instantiate($"Prefabs/Monsters/{key}", new Vector3(-4.3261f, 1f, 16.69641f), Quaternion.identity) as MonsterBase;
            m.Inititalize(waypoints);
            curMonsters.Add(m);
        }
    }

    public void SpawnMonsters(int difficulty,Bounds[] spawnAreas,Transform[] waypoints)
    {
        foreach (var monster in curMonsters)
        {
            NetworkObject.Destory(monster.photonView.ViewID);
        }
        curMonsters.Clear();
        int wholeMonsterAmount = 8 * difficulty;
        int curMonsterAmount = 0;
        foreach (var area in spawnAreas)
        {
            int spawnItemAmount = Random.Range(0,2);
            if (curMonsterAmount > wholeMonsterAmount) break;
            for (int i = 0; i < spawnItemAmount; i++)
            {
                var randomPos =
                    new Vector3
                    (
                        Random.Range(area.min.x, area.max.x),
                        Random.Range(area.center.y - 1, area.center.y + 2),
                        Random.Range(area.min.z, area.max.z)
                    );

                if (NavMesh.SamplePosition(randomPos + Vector3.down * 50, out var hit, 3, ~0)) //~0 is all layer 
                {
                   
                    var randomMonsterKey = monsters[Random.Range(0, monsters.Count)];
                    var monster = NetworkObject.Instantiate($"Prefabs/Monsters/{randomMonsterKey}",     hit.position, Quaternion.identity) as MonsterBase;
                    curMonsterAmount++;
                    monster.Inititalize(waypoints);
                    curMonsters.Add(monster);
                }
            }
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
