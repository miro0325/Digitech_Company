using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Photon.Pun;

[RequireComponent(typeof(PhotonView))]
public class Delivery : MonoBehaviourPun, IService
{
    [SerializeField] private GameObject truck;
    [SerializeField] private Container container;

    private Animator animator;
    private int orderItemCount;
    private bool isOrder;
    private List<string> orderedItems = new();

    private void Awake()
    {
        ServiceLocator.For(this).Register(this);
        animator = GetComponent<Animator>();

        DeliveryTask().Forget();
    }

    public void Order(string key, int amount)
    {
        for (int i = 0; i < amount; i++)
            orderedItems.Add(key);

        photonView.RPC(nameof(OrderRpc), RpcTarget.All);
    }

    [PunRPC]
    private void OrderRpc()
    {
        isOrder = true;
    }

    private async UniTaskVoid DeliveryTask()
    {
        while (true)
        {
            truck.SetActive(false);
            container.gameObject.SetActive(false);

            await UniTask.WaitUntil(() => orderedItems.Count != 0 || isOrder, cancellationToken: this.GetCancellationTokenOnDestroy());
            await UniTask.NextFrame();
            isOrder = false;

            await UniTask.WaitForSeconds(7f, cancellationToken: this.GetCancellationTokenOnDestroy());

            truck.SetActive(true);
            container.gameObject.SetActive(true);
            await UniTask.NextFrame();

            if(photonView.IsMine) container.SpawnItems(orderedItems);
            orderedItems.Clear();
            
            animator.Play("Arrive");
            await UniTask.WaitForSeconds(25f, cancellationToken: this.GetCancellationTokenOnDestroy());
            animator.Play("Depart");
            await UniTask.WaitForSeconds(2f, cancellationToken: this.GetCancellationTokenOnDestroy());
        }
    }
}
