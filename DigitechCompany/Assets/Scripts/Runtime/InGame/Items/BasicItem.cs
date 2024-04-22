using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicItem : ItemBase
{
    private float time = 0.45f;
    private float curTime = 0;
    private bool isUp = false;

    void Start()
    {
        curTime = time / 2;
    }

    void Update()
    {
        if (isInHand)
        {
            curTime += Time.deltaTime;
            if(curTime > time)
            {
                curTime = 0;
                isUp = !isUp;
            }
            transform.Translate(Vector3.up * Time.deltaTime * ((isUp) ? 1 : -1) * 2);
        }
    }

    public override void OnDrop()
    {
        base.OnDrop();
    }

    public override void OnGet(TempPlayer temp)
    {
        base.OnGet(temp);
    }

    public override void OnInteract(TempPlayer temp)
    {
        
    }
}
