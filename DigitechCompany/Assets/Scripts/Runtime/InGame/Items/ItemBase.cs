using Photon.Pun;
using UnityEngine;

public class ItemBase : NetworkObject, IInteractable
{
    //service
    private GameManager gameManager;
    private NetworkObjectManager networkObjectManager;

    //inspector field
    [SerializeField] protected Transform leftHandPoint;
    [SerializeField] protected Transform rightHandPoint;

    //field
    protected UnitBase ownUnit;
    protected Animator animator;
    protected Rigidbody rb;
    protected PhotonTransformView transformView;

    //property
    public bool InHand => ownUnit != null;
    public Transform LeftHandPoint => leftHandPoint;
    public Transform RightHandPoint => rightHandPoint;
    public float LayRotation { get; set; }
    public virtual InteractID TargetInteractID => InteractID.ID1;
    public virtual float Weight { get; }

    //method
    public virtual string GetInteractionExplain(UnitBase unit) => "줍기";

    public virtual bool IsInteractable(UnitBase unit)
    {
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

    public override void OnCreate()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        transformView = GetComponent<PhotonTransformView>();
    }
    
    public virtual void OnInteract(UnitBase unit)
    {
        ownUnit = unit;
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
        ownUnit = NetworkObject.GetNetworkObject(guid) as UnitBase;
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
        gameObject.SetActive(false);
        animator.SetLayerWeight(1, 0);
    }

    public virtual void OnDisable()
    {
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
        ownUnit = null;
        transformView.enabled = true;
        animator.enabled = false;
        rb.isKinematic = false;
        rb.detectCollisions = true;

        photonView.RPC(nameof(OnDiscardRpc), RpcTarget.Others);
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

    protected virtual void Start()
    {
        gameManager = Services.Get<GameManager>();
        networkObjectManager = Services.Get<NetworkObjectManager>();
    }

    protected virtual void Update()
    {
        if(!InHand)
            transform.localRotation = Quaternion.RotateTowards(transform.localRotation, Quaternion.Euler(0, LayRotation, 0), Time.deltaTime * 1080);
    }
}