using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Container : MonoBehaviour
{
    [SerializeField] private Transform[] doors;
    [SerializeField] private float pushSpeed;
    
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void Seperate()
    {
        transform.parent = null;
        var r = gameObject.AddComponent<Rigidbody>();
        r.mass = r.mass * 4;
        DoorControl();
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
