using System.Collections.Generic;
using Photon.Pun;
using UniRx;
using UnityEngine;

public class ItemBase : NetworkObject, IPunObservable, IInteractable, IUseable
{
    //service
    private DataContainer _dataContainer;
    private DataContainer dataContainer => _dataContainer ??= ServiceLocator.ForGlobal().Get<DataContainer>();
    private ItemManager _itemManager;
    private ItemManager itemManager => _itemManager ??= ServiceLocator.For(this).Get<ItemManager>();

    //inspector field
    [SerializeField] protected Vector3 holdPos;
    [SerializeField] protected Vector3 holdRot;
    [SerializeField] protected Vector3 camHoldPos;
    [SerializeField] protected Vector3 camHoldRot;
    [SerializeField] protected Transform leftHandPoint;
    [SerializeField] protected Transform rightHandPoint;

    //field
    protected float layRotation;
    protected float sellPrice;
    protected string key;
    protected ReactiveProperty<int> ownUnitViewId = new();
    protected new Collider collider;
    protected Rigidbody rb;
    protected PhotonTransformView transformView;
    protected MeshRenderer meshRenderer;

    //property
    protected UnitBase OwnUnit { get; private set; }

    public bool InHand => ownUnitViewId.Value != 0;
    public virtual float SellPrice => sellPrice;
    public float LayRotation => layRotation;
    public string Key => key;
    public Transform LeftHandPoint => leftHandPoint;
    public Transform RightHandPoint => rightHandPoint;
    public ItemData ItemData => dataContainer.loadData.itemDatas[key];
    public MeshRenderer MeshRenderer => meshRenderer;
    public UnitBase CurUnit => OwnUnit;

    //method
    public virtual void Initialize(string key)
    {
        this.key = key;
        sellPrice = Random.Range(ItemData.sellPriceMin, ItemData.sellPriceMax);
    }

    public virtual InteractID GetTargetInteractID(UnitBase unit) => InteractID.ID1;

    public virtual float GetInteractRequireTime(UnitBase unit) => 0;

    public virtual string GetInteractionExplain(UnitBase unit)
    {
        var player = unit as InGamePlayer;
        if (player)
        {
            if (!player.Inventory.IsInsertable()) // if player item container is not full or two hand
                return "º’¿Ã ≤À¬¸";
            else if (player.MaxStats.GetStat(Stats.Key.Weight) <= player.Inventory.WholeWeight + ItemData.weight) // if player strength lack
                return "»˚ ∫Œ¡∑";
            else
                return "¡›±‚";
        }
        return "";
    }

    public virtual bool IsInteractable(UnitBase unit)
    {
        var player = unit as InGamePlayer;
        if (player)
        {
            if (!player.Inventory.IsInsertable()) // if player item container is not full or two hand
                return false;
            else if (player.MaxStats.GetStat(Stats.Key.Weight) <= player.Inventory.WholeWeight + ItemData.weight) // if player strength lack
                return false;
            else
                return true;
        }
        return true;
    }

    public virtual bool IsUsable(InteractID id)
    {
        return false;
    }

    public virtual string GetUseExplain(InteractID id, UnitBase unit)
    {
        return "";
    }

    public void SetLayRotation(float f)
    {
        photonView.RPC(nameof(SetLayRotationRPC), RpcTarget.All, f);
    }

    [PunRPC]
    protected void SetLayRotationRPC(float f)
    {
        layRotation = f;
    }

    public override void OnCreate()
    {
        base.OnCreate();

        //getcomponent
        rb = GetComponent<Rigidbody>();
        collider = GetComponent<Collider>();
        transformView = GetComponent<PhotonTransformView>();
        meshRenderer = GetComponentInChildren<MeshRenderer>();

        ownUnitViewId
            .Subscribe(viewId =>
            {
                if (viewId == 0)
                {
                    OwnUnit = null;
                    transform.SetParent(null);
                    return;
                }

                var unit = PhotonView.Find(viewId).GetComponent<UnitBase>();
                OwnUnit = unit;

                var player = unit as InGamePlayer;
                if (player)
                    //if player view is camera set camera holder other is body holder
                    transform.SetParent(player.photonView.IsMine ? player.ItemHolderCamera : player.ItemHolder);
                else
                    transform.SetParent(unit.ItemHolder);
            });
    }

    public virtual void DestroyItem()
    {
        photonView.RPC(nameof(DestoryItemRpc), RpcTarget.All);
    }

    [PunRPC]
    protected virtual void DestoryItemRpc()
    {
        itemManager.Items.Remove(photonView.ViewID);
        NetworkObject.Destory(photonView.ViewID);
    }

    public virtual void OnInteract(UnitBase unit)
    {
        ownUnitViewId.Value = unit.photonView.ViewID;
        transformView.enabled = false;
        rb.isKinematic = true;
        rb.detectCollisions = false;

        //send rpc
        photonView.RPC(nameof(OnInteractRpc), RpcTarget.Others, ownUnitViewId.Value);
    }

    [PunRPC]
    protected virtual void OnInteractRpc(int viewId)
    {
        //to chest position
        collider.enabled = false;
        ownUnitViewId.Value = viewId;
        transformView.enabled = false;
        rb.isKinematic = true;
        rb.detectCollisions = false;
    }

    public virtual void OnActive()
    {
        transform.SetLocalPositionAndRotation(camHoldPos, Quaternion.Euler(camHoldRot));
        gameObject.SetActive(true);

        //invoke rpc
        photonView.RPC(nameof(OnActiveRpc), RpcTarget.Others);
    }

    [PunRPC]
    protected virtual void OnActiveRpc()
    {
        Debug.Log("active true");
        transform.SetLocalPositionAndRotation(holdPos, Quaternion.Euler(holdRot));
        gameObject.SetActive(true);
    }

    public virtual void OnInactive()
    {
        Debug.Log("active false");
        gameObject.SetActive(false);

        //invoke rpc
        photonView.RPC(nameof(OnInactiveRpc), RpcTarget.Others);
    }

    [PunRPC]
    protected virtual void OnInactiveRpc()
    {
        gameObject.SetActive(false);
    }

    public virtual void OnUsePressed(InteractID id)
    {
        photonView.RPC(nameof(OnUsePressedRpc), RpcTarget.Others, (int)id);
    }

    [PunRPC]
    protected virtual void OnUsePressedRpc(int id)
    {

    }

    public virtual void OnUseReleased()
    {
        photonView.RPC(nameof(OnUseReleasedRpc), RpcTarget.Others);
    }

    [PunRPC]
    protected virtual void OnUseReleasedRpc()
    {

    }

    public virtual void OnDiscard()
    {
        collider.enabled = true;
        ownUnitViewId.Value = 0;
        transformView.enabled = true;
        rb.isKinematic = false;
        rb.detectCollisions = true;

        photonView.RPC(nameof(OnDiscardRpc), RpcTarget.Others);
    }

    [PunRPC]
    protected virtual void OnDiscardRpc()
    {
        collider.enabled = true;
        ownUnitViewId.Value = 0;
        transformView.enabled = true;
        rb.isKinematic = false;
        rb.detectCollisions = true;
    }

    protected virtual void Update()
    {
        if (!InHand)
            transform.localRotation = Quaternion.RotateTowards(transform.localRotation, Quaternion.Euler(0, layRotation, 0), Time.deltaTime * 1080);
    }

    // protected override void OnSendData(List<System.Func<object>> send)
    // {
    //     send.Add(() => key);
    //     send.Add(() => layRotation);
    //     send.Add(() => ownUnitViewId.Value);
    //     send.Add(() => sellPrice);
    // }

    // protected override void OnReceiveData(List<System.Action<object>> receive)
    // {
    //     receive.Add(obj => key = (string)obj);
    //     receive.Add(obj => layRotation = (float)obj);
    //     receive.Add(obj => ownUnitViewId.Value = (int)obj);
    //     receive.Add(obj => sellPrice = (float)obj);
    // }

    public virtual void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(key);
            stream.SendNext(layRotation);
            stream.SendNext(ownUnitViewId.Value);
            stream.SendNext(sellPrice);
        }
        else
        {
            key = (string)stream.ReceiveNext();
            layRotation = (float)stream.ReceiveNext();
            ownUnitViewId.Value = (int)stream.ReceiveNext();
            sellPrice = (float)stream.ReceiveNext();
        }
    }
}