using Photon.Pun;
using UniRx;
using UnityEngine;

public class ItemBase : NetworkObject, IPunObservable, IInteractable
{
    //service
    private DataContainer dataContainer;

    //inspector field
    [SerializeField] protected Transform leftHandPoint;
    [SerializeField] protected Transform rightHandPoint;

    //field
    protected float layRotation;
    protected string key;
    protected string ownUnit;
    protected Animator animator;
    protected Rigidbody rb;
    protected PhotonTransformView transformView;
    protected MeshRenderer meshRenderer;

    //property
    public bool InHand => ownUnit != null;
    public virtual float SellPrice { get; protected set; }
    public Transform LeftHandPoint => leftHandPoint;
    public Transform RightHandPoint => rightHandPoint;
    public ItemData ItemData => dataContainer.itemDatas[key];
    public MeshRenderer MeshRenderer => meshRenderer;

    //method
    public virtual void Initialize(string key)
    {
        this.key = key;
        SellPrice = Random.Range(ItemData.sellPriceMin, ItemData.sellPriceMax);
    }

    public virtual InteractID GetTargetInteractID(UnitBase unit) => InteractID.ID1;

    public virtual float GetInteractRequireTime(UnitBase unit) => 0;

    public virtual string GetInteractionExplain(UnitBase unit)
    {
        var player = unit as Player;
        if (player)
        {
            if (!player.ItemContainer.IsInsertable()) // if player item container is not full or two hand
                return "손이 꽉참";
            else if (player.MaxStats.GetStat(Stats.Key.Weight) <= player.ItemContainer.WholeWeight + ItemData.weight) // if player strength lack
                return "힘 부족";
            else
                return "줍기";
        }
        return "";
    }

    public virtual bool IsInteractable(UnitBase unit)
    {
        var player = unit as Player;
        if (player)
        {
            if (!player.ItemContainer.IsInsertable()) // if player item container is not full or two hand
                return false;
            else if (player.MaxStats.GetStat(Stats.Key.Weight) <= player.ItemContainer.WholeWeight + ItemData.weight) // if player strength lack
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
        layRotation = f;
    }

    public override void OnCreate()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        transformView = GetComponent<PhotonTransformView>();
        meshRenderer = GetComponentInChildren<MeshRenderer>();

        dataContainer = ServiceLocator.GetEveryWhere<DataContainer>();

        this.ObserveEveryValueChanged(x => x.ownUnit)
            .Subscribe(guid => 
            {
                Debug.Log(guid);
                
                if(string.IsNullOrEmpty(guid))
                {
                    transform.SetParent(null);
                    return;
                }

                var unit = NetworkObject.GetNetworkObject(guid) as UnitBase;
                var player = unit as Player;

                if(player)
                {
                    //if player view is camera set camera holder other is body holder 
                    transform.SetParent(player.photonView.IsMine ? player.ItemHolderCamera : player.ItemHolder);
                    return;
                }

                transform.SetParent(unit.ItemHolder);
            });
    }

    public virtual void OnInteract(UnitBase unit)
    {
        ownUnit = unit.guid;
        transformView.enabled = false;
        animator.enabled = true;
        rb.isKinematic = true;
        rb.detectCollisions = false;

        //send rpc
        photonView.RPC(nameof(OnInteractRpc), RpcTarget.Others, unit.guid);
    }

    [PunRPC]
    protected virtual void OnInteractRpc(string guid)
    {
        //to chest position
        ownUnit = guid;
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
        photonView.RPC(nameof(OnActiveRpc), RpcTarget.OthersBuffered);
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
        photonView.RPC(nameof(OnDisableRpc), RpcTarget.OthersBuffered);
    }

    [PunRPC]
    protected virtual void OnDisableRpc()
    {
        gameObject.SetActive(false);
    }

    public virtual void OnUse(InteractID id)
    {
        photonView.RPC(nameof(OnUseRpc), RpcTarget.OthersBuffered, (int)id);
    }

    [PunRPC]
    protected virtual void OnUseRpc(int id)
    {

    }

    public virtual void OnDiscard()
    {
        ownUnit = null;
        transformView.enabled = true;
        animator.enabled = false;
        rb.isKinematic = false;
        rb.detectCollisions = true;

        photonView.RPC(nameof(OnDiscardRpc), RpcTarget.OthersBuffered);
    }

    [PunRPC]
    protected virtual void OnDiscardRpc()
    {
        ownUnit = null;
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

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if(stream.IsWriting)
        {
            stream.SendNext(key);
            stream.SendNext(layRotation);
            stream.SendNext(ownUnit);
        }
        else
        {
            key = (string)stream.ReceiveNext();
            layRotation = (float)stream.ReceiveNext();
            ownUnit = (string)stream.ReceiveNext();
        }
    }
}