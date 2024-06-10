using Photon.Pun;
using UniRx;
using UnityEngine;

public class ItemBase : NetworkObject, IPunObservable, IInteractable
{
    //service
    private DataContainer dataContainer;
    private DataContainer DataContainer => dataContainer ??= ServiceLocator.ForGlobal().Get<DataContainer>();
    private ItemManager itemManager;
    private ItemManager ItemManager => itemManager ??= ServiceLocator.For(this).Get<ItemManager>();

    //inspector field
    [SerializeField] protected Transform leftHandPoint;
    [SerializeField] protected Transform rightHandPoint;

    //field
    protected float layRotation;
    protected float sellPrice;
    protected string key;
    protected ReactiveProperty<int> ownUnit = new();
    protected new Collider collider;
    protected Animator animator;
    protected Rigidbody rb;
    protected PhotonTransformView transformView;
    protected MeshRenderer meshRenderer;

    //property
    public bool InHand => ownUnit.Value != 0;
    public virtual float SellPrice => sellPrice;
    public float LayRotation => layRotation;
    public string Key => key;
    public Transform LeftHandPoint => leftHandPoint;
    public Transform RightHandPoint => rightHandPoint;
    public ItemData ItemData => DataContainer.itemDatas[key];
    public MeshRenderer MeshRenderer => meshRenderer;

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
        //getcomponent
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        collider = GetComponent<Collider>();
        transformView = GetComponent<PhotonTransformView>();
        meshRenderer = GetComponentInChildren<MeshRenderer>();

        ownUnit
            .Subscribe(viewId => 
            {                
                if(viewId == 0)
                {
                    transform.SetParent(null);
                    return;
                }

                var unit = PhotonView.Find(viewId).GetComponent<UnitBase>();
                var player = unit as InGamePlayer;

                if(player)
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
        ItemManager.Items.Remove(photonView.ViewID);
        NetworkObject.Destory(photonView.ViewID);
    }

    public virtual void OnInteract(UnitBase unit)
    {
        ownUnit.Value = unit.photonView.ViewID;
        transformView.enabled = false;
        animator.enabled = true;
        rb.isKinematic = true;
        rb.detectCollisions = false;

        //send rpc
        photonView.RPC(nameof(OnInteractRpc), RpcTarget.Others, ownUnit.Value);
    }

    [PunRPC]
    protected virtual void OnInteractRpc(int viewId)
    {
        //to chest position
        collider.enabled = false;
        ownUnit.Value = viewId;
        transformView.enabled = false;
        animator.enabled = true;
        rb.isKinematic = true;
        rb.detectCollisions = false;
        

        //chest view weight set
        animator.SetLayerWeight(1, 0);
    }

    public virtual void OnActive()
    {
        gameObject.SetActive(true);
        animator.SetLayerWeight(1, 1);

        //invoke rpc
        photonView.RPC(nameof(OnActiveRpc), RpcTarget.Others);
    }

    [PunRPC]
    protected virtual void OnActiveRpc()
    {
        Debug.Log("active true");
        gameObject.SetActive(true);
        animator.SetLayerWeight(1, 0);
    }

    public virtual void OnDisable()
    {
        Debug.Log("active false");
        gameObject.SetActive(false);

        //invoke rpc
        photonView.RPC(nameof(OnDisableRpc), RpcTarget.Others);
    }

    [PunRPC]
    protected virtual void OnDisableRpc()
    {
        gameObject.SetActive(false);
    }

    public virtual void OnUse(InteractID id)
    {
        photonView.RPC(nameof(OnUseRpc), RpcTarget.Others, (int)id);
    }

    [PunRPC]
    protected virtual void OnUseRpc(int id)
    {

    }

    public virtual void OnDiscard()
    {
        transform.position = transform.parent.position;

        collider.enabled = true;
        ownUnit.Value = 0;
        transformView.enabled = true;
        animator.enabled = false;
        rb.isKinematic = false;
        rb.detectCollisions = true;

        photonView.RPC(nameof(OnDiscardRpc), RpcTarget.Others);
    }

    [PunRPC]
    protected virtual void OnDiscardRpc()
    {
        transform.position = transform.parent.position;

        collider.enabled = true;
        ownUnit.Value = 0;
        transformView.enabled = true;
        animator.enabled = false;
        rb.isKinematic = false;
        rb.detectCollisions = true;
    }

    protected virtual void Update()
    {
        if (!InHand)
            transform.localRotation = Quaternion.RotateTowards(transform.localRotation, Quaternion.Euler(0, layRotation, 0), Time.deltaTime * 1080);
    }

    public virtual void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if(stream.IsWriting)
        {
            stream.SendNext(key);
            stream.SendNext(layRotation);
            stream.SendNext(ownUnit.Value);
            stream.SendNext(sellPrice);
        }
        else
        {
            key = (string)stream.ReceiveNext();
            layRotation = (float)stream.ReceiveNext();
            ownUnit.Value = (int)stream.ReceiveNext();
            sellPrice = (float)stream.ReceiveNext();
        }
    }
}