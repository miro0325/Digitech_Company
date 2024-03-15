using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Linq;


public class BezierCurve
{
    public static Vector3 Lerp(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, float value)
    {
        Vector3 a = Vector3.Lerp(p1, p2, value);
        Vector3 b = Vector3.Lerp(p2, p3, value);
        Vector3 c = Vector3.Lerp(p3, p4, value);

        Vector3 d = Vector3.Lerp(a, b, value);
        Vector3 e = Vector3.Lerp(b, c, value);

        Vector3 f = Vector3.Lerp(d, e, value);
        return f;
    }
}

[System.Serializable]
public class MovePoint
{
    public Transform point;
    public float moveDelay;
    public Ease ease;
}

public class Delivary : MonoBehaviour
{
    [SerializeField] float delivaryDuration;
    private float curTime = 0;

    [Header("Move Settings"), Space(5)]
    [SerializeField] Transform spawnTrans;
    [SerializeField] Transform arriveTrans;

    [SerializeField] Ease ease;
    [SerializeField] float rotSpeed;
    [SerializeField] float leaveSpeed;
    [SerializeField] float moveDelay;
    [Header("Transformation Setting"), Space(5)]
    [SerializeField] GameObject delivaryObj;
    [SerializeField] Transform delivaryCompleteTrans;
    [SerializeField] float transformDelay;

    [SerializeField] Transform[] tires;
    [SerializeField] List<MovePoint> tireOriginPoint;
    [SerializeField] List<MovePoint> tireMovePoint;

    [SerializeField] ParticleSystem boostFX;

    [Header("Delivary Setting"), Space(5)]
    [SerializeField] Transform containerPos;
    [SerializeField] List<GameObject> delivaryItems = new();
    [SerializeField] GameObject containerObj;

    private bool isArrive = false;

    private Container container;

    private Vector3 originPos;
    private Vector3 originRot;
   

    private void Start()
    {
        container = containerPos.transform.GetChild(0).GetComponent<Container>();    
    }

    private void Update()
    {
        UpdateDelivaryTimer();
        if(Input.GetKeyDown(KeyCode.LeftAlt))
        {
            ArriveMovement();
        }
    }

    private void InitDelivary()
    {
        transform.rotation = Quaternion.Euler(-90, 0, 0);
        containerPos.localRotation = Quaternion.identity;
        delivaryObj.transform.localRotation = Quaternion.identity;
        if(container == null)
        {
            var c = Instantiate(containerObj, containerPos);
            c.transform.localPosition = Vector3.zero;
            container = c.GetComponent<Container>();
        }
        BoostOnOff();

    }

    private void UpdateDelivaryTimer()
    {
        if(isArrive)
        {
            curTime += Time.deltaTime;
            if(curTime >= delivaryDuration)
            {
                curTime = 0;
                isArrive = false;
                LeaveMovement();
            }
        }
    }

    private void ArriveMovement()
    {
        InitDelivary();
        Sequence sequence = DOTween.Sequence();
        transform.position = spawnTrans.position;
        var rot = transform.eulerAngles;
        rot.y = 0;
        Coroutine coroutine = StartCoroutine(Moving());
        transform.DOMove(arriveTrans.position, moveDelay).SetEase(ease).OnComplete(
            () =>
            {
                StopCoroutine(coroutine);
                transform.DORotate(rot, 0.3f);
                RotateDelivaryObj().SetDelay(1.5f).SetEase(Ease.OutExpo).OnComplete(() => SeperateContainer());
                Transformation();
                BoostOnOff();
                isArrive = true;
            });
        IEnumerator Moving()
        {
            while(true)
            {
                transform.Rotate(new Vector3(0, 0, rotSpeed * Time.deltaTime));
                yield return null;
            }
        }
    }

    private void LeaveMovement()
    {
        
        var rot = transform.eulerAngles;
        rot.y = 0;
        StartCoroutine(Moving());
        IEnumerator Moving()
        {
            float maxTime = 10f;
            float curTime = 0;
            while (curTime < maxTime)
            {
                transform.Translate(-transform.forward * Time.deltaTime * leaveSpeed);
                curTime += Time.deltaTime;
                yield return null;
            }
        }
    }
         
    private void Transformation()
    {
        if(!isArrive)
        {
            for(int i = 0; i < tires.Length; i++)
            {
                MovePoint p = tireOriginPoint[i];
                tires[i].DOLocalMove(p.point.localPosition,p.moveDelay).SetEase(p.ease);
                tires[i].DOLocalRotate(p.point.localEulerAngles, p.moveDelay).SetEase(p.ease);
            }
        } 
        else
        {
            for (int i = 0; i < tires.Length; i++)
            {
                MovePoint p = tireMovePoint[i];
                tires[i].DOLocalMove(p.point.localPosition, p.moveDelay).SetEase(p.ease);
                tires[i].DOLocalRotate(p.point.localEulerAngles, p.moveDelay).SetEase(p.ease);
            }
        }
    }

    private Tween RotateDelivaryObj()
    {
        Sequence sequence = DOTween.Sequence();
        Tween tween;
        if(!isArrive)
        {
            originPos = delivaryObj.transform.localPosition;
            originRot = delivaryObj.transform.localEulerAngles; 
            tween = delivaryObj.transform.DOLocalMove(delivaryCompleteTrans.localPosition, transformDelay);
            sequence.Append(tween);
            tween = delivaryObj.transform.DOLocalRotate(delivaryCompleteTrans.localEulerAngles, transformDelay);
            sequence.Join(tween);
            tween = containerPos.transform.DOLocalMove(delivaryCompleteTrans.localPosition, transformDelay);
            sequence.Join(tween);
            tween = containerPos.transform.DOLocalRotate(delivaryCompleteTrans.localEulerAngles, transformDelay);
            sequence.Join(tween);
        } else
        {
            tween = delivaryObj.transform.DOLocalMove(originPos, transformDelay);
            sequence.Append(tween);
            tween = delivaryObj.transform.DOLocalRotate(originRot, transformDelay);
            sequence.Join(tween);
            tween = containerPos.transform.DOLocalMove(delivaryCompleteTrans.localPosition, transformDelay);
            sequence.Join(tween);
            tween = containerPos.transform.DOLocalRotate(delivaryCompleteTrans.localEulerAngles, transformDelay);
            sequence.Join(tween);
        }
        return sequence;
    }

    private void SeperateContainer()
    {
        container.Seperate();
        container = null;
    }

    private void BoostOnOff()
    {
        var main = boostFX.main;
        main.loop = !main.loop;
        if(main.loop)
            boostFX.Play();
    }
    
}
