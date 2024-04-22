using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempPlayer : MonoBehaviour
{
    [SerializeField] ItemBase testItem;
    [SerializeField] Transform itemGrab;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.E))
            PickDropItem();

    }

    void PickDropItem()
    {
        if(testItem.IsInHand)
        {
            testItem.transform.parent = null;
            testItem.OnDrop();
        } else
        {
            testItem.OnGet(this);
            testItem.transform.localScale = Vector3.one * 3;
            testItem.transform.eulerAngles = itemGrab.localEulerAngles;
            testItem.transform.parent = itemGrab.transform;
            testItem.transform.localPosition = Vector3.zero;
        }
    }
}
