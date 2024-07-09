using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Photon.Pun;
using UnityEngine;

public class SawTrap : NetworkObject, IPunObservable
{
    [SerializeField] Transform blade;
    [SerializeField] private Vector3[] points;
    [SerializeField] private float rotateSpeed;
    [SerializeField] private float speed;
    [SerializeField] private float waitTime;

    private int curPointIndex;

    private void Start()
    {
        MoveRoutine().Forget();
    }

    private void Update()
    {
        blade.Rotate(0, rotateSpeed * Time.deltaTime, 0);
    }

    private async UniTask MoveRoutine()
    {
        while(true)
        {
            transform.position = Vector3.MoveTowards(transform.position, points[curPointIndex], speed * Time.deltaTime);

            if(Vector3.Distance(transform.position, points[curPointIndex]) < speed * Time.deltaTime * 1.1f)
            {
                curPointIndex++; 
                if(curPointIndex >= points.Length)
                    curPointIndex = 0;
                
                await UniTask.WaitForSeconds(waitTime);
            }
            await UniTask.NextFrame();
        }
    }

    private void OnDrawGizmosSelected()
    {
        for(int i = 0; i < points.Length - 1; i++)
            DebugArrow.DrawGizmos(points[i], points[i + 1], Color.red);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if(stream.IsWriting)
        {
            stream.SendNext(curPointIndex);
        }
        else
        {
            curPointIndex = (int)stream.ReceiveNext();
        }
    }
}
