using System.Collections.Generic;
using DG.Tweening;
using Photon.Pun;
using UnityEngine;

public enum TestBasementState
{
    Up,
    TakingOff,
    Down,
    Landing,
}

public class TestBasement : MonoBehaviourPun, IService, IPunObservable
{
    [SerializeField] private Vector3 upPos;
    [SerializeField] private Vector3 downPos;

    private Vector3 position;
    private TestBasementState state;
    private Dictionary<int, ItemBase> items = new();

    public TestBasementState State => state;
    public Dictionary<int, ItemBase> Items => items;

    private void Awake()
    {
        ServiceLocator.For(this).Register(this);
    }

    public void MoveUp()
    {
        photonView.RPC(nameof(MoveRpc), RpcTarget.All, (int)TestBasementState.Up);
    }

    public void MoveDown()
    {
        photonView.RPC(nameof(MoveRpc), RpcTarget.All, (int)TestBasementState.Down);
    }

    [PunRPC]
    private void MoveRpc(int state)
    {
        var eState = (TestBasementState)state;
        if(eState == TestBasementState.Landing) return;
        if(eState == TestBasementState.TakingOff) return;

        this.state = eState == TestBasementState.Up ? TestBasementState.TakingOff : TestBasementState.Landing;
        transform
            .DOMove(eState == TestBasementState.Up ? upPos : downPos, 8f)
            .SetEase(Ease.InOutQuart)
            .OnComplete(() => this.state = eState);
    }

    private void OnTriggerStay(Collider other)
    {
        if(other.transform.parent == null)
        {
            other.transform.SetParent(transform);
            
            if(other.TryGetComponent<ItemBase>(out var comp))
                items.Add(comp.photonView.ViewID, comp);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(ReferenceEquals(other.transform.parent, transform))
            other.transform.SetParent(null);
    }

    private void Update()
    {
        position = transform.position;
        transform.position = Vector3.Lerp(transform.position, position, 8 * Time.deltaTime);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if(stream.IsWriting)
        {
            stream.SendNext(position);
        }
        else
        {
            position = (Vector3)stream.ReceiveNext();
        }
    }
}