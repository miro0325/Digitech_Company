using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using DG.Tweening;
using Photon.Pun;

[RequireComponent(typeof(PhotonView))]
public class CompanySell : MonoBehaviourPun, IInteractable
{
    [SerializeField] private Vector3 offset;
    [SerializeField] private Vector3 halfExtends;
    [SerializeField] private GameObject blockingColliders;
    [SerializeField] private Transform leftDoor;
    [SerializeField] private Transform rightDoor;

    private Vector3 leftDoorDefaultPos;
    private Vector3 rightDoorDefaultPos;
    private bool isSelling;

    private void Start()
    {
        leftDoorDefaultPos = leftDoor.localPosition;
        rightDoorDefaultPos = rightDoor.localPosition;
    }

    public string GetInteractionExplain(UnitBase unit)
    {
        return isSelling ? "작동 중" : "판매";
    }

    public float GetInteractRequireTime(UnitBase unit)
    {
        return 2;
    }

    public InteractID GetTargetInteractID(UnitBase unit)
    {
        return InteractID.ID1;
    }

    public bool IsInteractable(UnitBase unit)
    {
        return !isSelling;
    }

    public void OnInteract(UnitBase unit)
    {
        isSelling = true;
        SellTask().Forget();
    }

    private async UniTask SellTask()
    {
        blockingColliders.SetActive(true);
        await UniTask.WaitForSeconds(1f);

        //+-7.5f
        var leftDoorTargetPos = leftDoorDefaultPos;
        leftDoorTargetPos.z -= 7.5f;
        var rightDoorTargetPos = rightDoorDefaultPos;
        rightDoorTargetPos.z += 7.5f;
        
        leftDoor.DOLocalMove(leftDoorTargetPos, 6f).SetEase(Ease.InQuad);
        rightDoor.DOLocalMove(rightDoorTargetPos, 6f).SetEase(Ease.InQuad);
        await UniTask.WaitForSeconds(8f);
        
        leftDoor.DOLocalMove(leftDoorDefaultPos, 2f).SetEase(Ease.Unset);
        rightDoor.DOLocalMove(rightDoorDefaultPos, 2f).SetEase(Ease.Unset);

        await UniTask.WaitForSeconds(2f);
        blockingColliders.SetActive(false);

        isSelling = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position + offset, halfExtends * 2);
    }
}