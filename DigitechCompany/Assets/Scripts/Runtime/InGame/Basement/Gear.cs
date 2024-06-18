using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Gear : MonoBehaviour,IInteractable
{
    private GameManager gameManager;
    private GameManager GameManager
    {
        get
        {
            if (ReferenceEquals(gameManager, null))
                gameManager = ServiceLocator.For(this).Get<GameManager>();
            return gameManager;
        }
    }

    private Basement basement;
    private Basement Basement
    {
        get
        {
            if (ReferenceEquals(basement, null))
                basement = ServiceLocator.For(this).Get<Basement>();
            return basement;
        }
    }

    [SerializeField] private Transform gearStick;
    [SerializeField] private Vector3 rotAngle;

    public string GetInteractionExplain(UnitBase unit)
    {
        if (!Basement.IsArrive) return "출발하기";
        else if (Basement.IsMoving) return "";
        else return "떠나기";
    }

    public float GetInteractRequireTime(UnitBase unit)
    {
        return 0.4f;
    }

    public InteractID GetTargetInteractID(UnitBase unit)
    {
        return InteractID.ID1;
    }

    public bool IsInteractable(UnitBase unit)
    {
        if(Basement.IsMoving) return false;
        else return true;
    }

    public void OnInteract(UnitBase unit)
    {
        if(!Basement.IsArrive)
        {
            Basement.Arrive();
            gearStick.DOLocalRotate(rotAngle, 0.25f);
        }
        else
        {
            Basement.Leave();
            gearStick.DOLocalRotate(Vector3.zero, 0.25f);
        }
    }
}
