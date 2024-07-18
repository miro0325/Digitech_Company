using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using DG.Tweening;
using Photon.Pun;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(PhotonView))]
public class CompanySell : MonoBehaviourPun, IInteractable
{
    private GameManager _gameManager;
    private GameManager gameManager => _gameManager ??= ServiceLocator.For(this).Get<GameManager>();

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
        photonView.RPC(nameof(SellTaskRpc), RpcTarget.All);
    }

    [PunRPC]
    private void SellTaskRpc()
    {
        SellTask().Forget();
    }

    private async UniTask SellTask()
    {
        var colliders = Physics.OverlapBox(transform.position + offset, halfExtends);

        List<ItemBase> items = new();
        foreach(var col in colliders)
        {
            if(col.TryGetComponent<ItemBase>(out var comp))
                items.Add(comp);
        }

        blockingColliders.SetActive(true);
        await UniTask.WaitForSeconds(1f);

        //+-7.5f
        var leftDoorTargetPos = leftDoorDefaultPos;
        leftDoorTargetPos.z -= 7.5f;
        var rightDoorTargetPos = rightDoorDefaultPos;
        rightDoorTargetPos.z += 7.5f;
        
        leftDoor.DOLocalMove(leftDoorTargetPos, 4f).SetEase(Ease.InQuad);
        rightDoor.DOLocalMove(rightDoorTargetPos, 4f).SetEase(Ease.InQuad);
        await UniTask.WaitForSeconds(6f);

        if(items.Count > 0)
            gameManager.Earn(items.Select(item => item.photonView.ViewID).ToList().ToJson());

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