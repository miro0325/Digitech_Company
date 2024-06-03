using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;


public class Basement : MonoBehaviour, IService
{
    public bool IsOpenDoor => isOpenDoor;
    public bool IsMovingDoor => isMovingDoor;
    
    [SerializeField] Transform doorOpenTrans;
    [SerializeField] Transform backDoor;
    [SerializeField] float openDelay;
    private Vector3 prevPos, prevRot;
    private bool isOpenDoor = true;
    private bool isMovingDoor = false;

    [SerializeField] Transform[] tires;
    [SerializeField] float tireRotDelay;
    [SerializeField] float tireRotAngle;
    [SerializeField] Ease ease;

    [SerializeField] Animator animator;

    private Sequence sequence;
    // Start is called before the first frame update
    void Start()
    {
        ServiceLocator.For(this).Register(this);
        sequence = DOTween.Sequence();
        InteractDoor();
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void InteractDoor()
    {
        
        sequence.Kill();
        isOpenDoor = !isOpenDoor;
        if (isOpenDoor)
            CloseDoor();
        else
            OpenDoor();

    }

    private void OpenDoor()
    {
        isMovingDoor = true;
        prevPos = backDoor.localPosition;
        prevRot = backDoor.localEulerAngles;

        sequence.Append(backDoor.DOLocalMove(doorOpenTrans.localPosition, openDelay).OnComplete(() => isMovingDoor = false));
        sequence.Append(backDoor.DOLocalRotate(doorOpenTrans.localEulerAngles,openDelay));
    }

    private void CloseDoor()
    {
        isMovingDoor = true;
        sequence.Append(backDoor.DOLocalMove(prevPos, openDelay).OnComplete(() => isMovingDoor = false));
        sequence.Append(backDoor.DOLocalRotate(prevRot, openDelay));
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
