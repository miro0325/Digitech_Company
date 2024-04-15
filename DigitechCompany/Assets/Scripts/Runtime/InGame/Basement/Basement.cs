using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Basement : MonoBehaviour
{
    [SerializeField] Transform doorOpenTrans;
    [SerializeField] Transform backDoor;
    [SerializeField] float openDelay;
    private Vector3 prevPos, prevRot;
    private bool isOpenDoor = true;

    [SerializeField] Transform[] tires;
    [SerializeField] float tireRotDelay;
    [SerializeField] float tireRotAngle;
    [SerializeField] Ease ease;

    [SerializeField] Animator animator;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        InteractDoor();
    }

    private void InteractDoor()
    {
        if(Input.GetKeyUp(KeyCode.Space))
        {
            isOpenDoor = !isOpenDoor;
            if (isOpenDoor)
                CloseDoor();
            else
                OpenDoor();
        }
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            Leave();
        }

    }

    private void OpenDoor()
    {
        prevPos = backDoor.localPosition;
        prevRot = backDoor.localEulerAngles;
        backDoor.DOLocalMove(doorOpenTrans.localPosition, openDelay);
        backDoor.DOLocalRotate(doorOpenTrans.localEulerAngles,openDelay);
    }

    private void CloseDoor()
    {
        backDoor.DOLocalMove(prevPos, openDelay);
        backDoor.DOLocalRotate(prevRot, openDelay);
    }

    private void Leave()
    {
        RotateTire(true).OnComplete(
            () => { animator.SetTrigger("Leave"); }
        );
    }

    private Tween RotateTire(bool isLeave = false)
    {
        //Sequence sequence = DOTween.Sequence();
        if(isLeave)
        {
            tires[0].DOLocalRotate(new Vector3(tireRotAngle, 90, 0), tireRotDelay).SetEase(ease);
            tires[1].DOLocalRotate(new Vector3(tireRotAngle, 90, 0), tireRotDelay).SetEase(ease);
            tires[2].DOLocalRotate(new Vector3(-tireRotAngle, 90, 0), tireRotDelay).SetEase(ease);
            return tires[3].DOLocalRotate(new Vector3(-tireRotAngle, 90, 0), tireRotDelay).SetEase(ease);
        } else
        {
            tires[0].DOLocalRotate(new Vector3(0, 90, 0), tireRotDelay).SetEase(ease);
            tires[1].DOLocalRotate(new Vector3(0, 90, 0), tireRotDelay).SetEase(ease);
            tires[2].DOLocalRotate(new Vector3(0, 90, 0), tireRotDelay).SetEase(ease);
            return tires[3].DOLocalRotate(new Vector3(0, 90, 0), tireRotDelay).SetEase(ease);
        }
        //return sequence;
    }
}
