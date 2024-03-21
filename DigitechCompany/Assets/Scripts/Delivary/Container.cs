using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Container : MonoBehaviour
{
    [SerializeField] private Transform[] doors;
    [SerializeField] private float pushSpeed;
    [SerializeField] private Transform spawnRadius;

    private List<ItemBase> items = new();
    
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void Seperate(List<ItemBase> itmeList)
    {
        items = itmeList;
        SpawnItems();
        transform.parent = null;
        var r = gameObject.AddComponent<Rigidbody>();
        r.mass = r.mass * 4;
        DoorControl();
        
    }

    private void SpawnItems()
    {
        foreach(var item in items)
        {
            var spawnPos = spawnRadius.position;
            Instantiate(item,transform.position,Quaternion.identity);
        }
    }

    private void DoorControl()
    {
        for(int i = 0; i < doors.Length; i++)
        {
            var r = doors[i].gameObject.AddComponent<Rigidbody>();
            r.AddForce(transform.forward * pushSpeed);
        }

    }
}
