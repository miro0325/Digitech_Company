using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;


public class Basement : MonoBehaviour, IService
{
    public bool IsOpenDoor => isOpenDoor;
    public bool IsMovingDoor => isMovingDoor;
    public bool IsMoving => isMoving;
    public bool IsArrive => isArrive;
    
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

    private bool isMoving = false;
    private bool isArrive = true;
    private Sequence sequence;
    // Start is called before the first frame update
    void Start()
    {
        ServiceLocator.For(this).Register(this);
        sequence = DOTween.Sequence();
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

    public void Leave()
    {
        isMoving = true;
        isArrive = false;
        RotateTire(true).OnComplete(
            () => { animator.SetTrigger("Leave"); }
        );
    }

    public void Arrive()
    {
        isMoving = true;
        isArrive = true;
        RotateTire(true).OnComplete(
            () => { animator.SetTrigger("Arrive"); }
        );
    }

    public void ResetTire()
    {
        RotateTire(false);
        if(isArrive) transform.localPosition = new Vector3(0, 0, 0);
        else transform.localPosition = new Vector3(0, 80, 300);
        isMoving = false;
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

    private void OnTriggerStay(Collider other)
    {
        if (other.transform.parent == null)
        {
            other.transform.SetParent(transform);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (ReferenceEquals(other.transform.parent, transform))
            other.transform.SetParent(null);
    }
}
