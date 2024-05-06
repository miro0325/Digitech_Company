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
    public virtual string InteractionExplain => "줍기";
    public Transform LeftHandPoint => leftHandPoint;
    public Transform RightHandPoint => rightHandPoint;

    public override void OnCreate()
    {
        base.OnCreate();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        transformView = GetComponent<PhotonTransformView>();
    }

    private void Start()
    {
        gameManager = Services.Get<GameManager>();
        networkObjectManager = Services.Get<NetworkObjectManager>();
    }

    public virtual void OnInteract(UnitBase unit)
    {
        //to camera position
        ownUnit = unit;
        transformView.enabled = false;
        animator.enabled = true;
        rb.isKinematic = true;
        rb.detectCollisions = false;
        
        //camera view weight set
        animator.SetLayerWeight(1, 1);

        //send rpc
        photonView.RPC(nameof(OnInteractRpc), RpcTarget.Others, unit.guid);
    }

    [PunRPC]
    protected virtual void OnInteractRpc(string guid)
    {
        //to chest position
        transformView.enabled = false;
        ownUnit = NetworkObject.GetNetworkObject(guid) as UnitBase;

        //chest view weight set
        animator.SetLayerWeight(1, 0);
    }

    public virtual void OnUse() { }

    public virtual void OnThrow() { }
}