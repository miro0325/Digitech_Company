using Photon.Pun;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.Rendering;

public class Player : UnitBase
{
    //const
    private readonly static int Animator_MoveStateHash = Animator.StringToHash("MoveState"); //0 : idle, 1 : move, 2 : run
    private readonly static int Animator_MoveXHash = Animator.StringToHash("MoveX");
    private readonly static int Animator_MoveYHash = Animator.StringToHash("MoveY");
    private readonly static int Animator_IsGroundHash = Animator.StringToHash("IsGround");
    private readonly static int Animator_JumpHash = Animator.StringToHash("Jump");

    //service
    private DataContainer dataContainer;

    //inspector field
    [Header("Value")]
    [SerializeField] private float gravity;
    [SerializeField] private float jumpScale;
    [SerializeField] private float camRotateXClamp;
    [SerializeField] private float interactionDistance;
    [Header("Reference")]
    [SerializeField] private Transform camView;
    [SerializeField] private Camera cam;
    [SerializeField] private Transform itemHolder;
    [SerializeField] private Transform itemHolderCamera;
    [Header("Animator")]
    [SerializeField] private Animator camAnimator;
    [SerializeField] private Animator playerModelAnimator;
    [SerializeField] private Animator armModelAnimator;

    //field
    private bool isCrouch;
    private float camRotateX;
    private float velocityY;
    private PlayerInput playerInput;
    private CharacterController cc;
    private Stats testBaseStat; //test base stat(need to change)
    private IInteractable lookInteractable;
    private ItemContainer itemContainer;

    //property
    public IInteractable LookInteractable => lookInteractable;
    public override Stats BaseStats => testBaseStat;

    public override void OnCreate()
    {
        base.OnCreate();

        if (!photonView.IsMine) return;
        itemContainer = new(4);
        itemContainer.OnIndexChanged += (pre, cur) =>
        {
            itemContainer[pre]?.gameObject.SetActive(false);
            itemContainer[cur]?.gameObject.SetActive(true);
        };

        testBaseStat = new();
        testBaseStat.ModifyStat(Stats.Key.Hp, x => 100);
        testBaseStat.ModifyStat(Stats.Key.Speed, x => 3);
        testBaseStat.ModifyStat(Stats.Key.Stamina, x => 10);
        testBaseStat.ModifyStat(Stats.Key.Strength, x => 10);
        maxStats.ChangeFrom(testBaseStat);

        cc = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();

        playerModelAnimator.GetComponentsInChildren<SkinnedMeshRenderer>().For((i, ele) => ele.shadowCastingMode = ShadowCastingMode.ShadowsOnly);
        camView.gameObject.SetActive(true);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Services.Register(this);
    }

    private void Start()
    {
        dataContainer = Services.Get<DataContainer>();
    }

    private void Update()
    {
        Debug.Log(itemContainer.GetCurrentSlotItem()?.LeftHandPoint);
        DoInteract();
        DoMovement();
        DoRotation();
        DoAnimator();
    }

    [PunRPC]
    private void SetItemParemtRpc(string guid)
    {
        var item = NetworkObject.GetNetworkObject(guid);
        item.transform.SetParent(itemHolder);
    }

    private void DoInteract()
    {
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out var hit, interactionDistance, ~LayerMask.GetMask("Player")))
            hit.collider.TryGetComponent(out lookInteractable);
        else
            lookInteractable = null;

        if (!photonView.IsMine) return;
        if (playerInput.InteractInput && lookInteractable != null)
        {
            if (lookInteractable is ItemBase)
            {
                var item = lookInteractable as ItemBase;
                if (itemContainer.TryInsertItem(item))
                {
                    item.transform.SetParent(itemHolderCamera);
                    item.OnInteract(this);
                    photonView.RPC(nameof(SetItemParemtRpc), RpcTarget.Others, item.guid);
                }
            }
            else
            {
                lookInteractable.OnInteract(this);
            }
        }
    }

    private void DoMovement()
    {
        if (!photonView.IsMine) return;

        //crouch
        if (playerInput.CrouchInput)
            isCrouch = !isCrouch;

        if (isCrouch)
        {
            if (playerInput.RunInput)
                isCrouch = false;

            camView.localPosition =
                Vector3.Lerp(camView.localPosition, new Vector3(0, 0.75f, 0), Time.deltaTime * 8f);
        }
        else
        {
            camView.localPosition =
                Vector3.Lerp(camView.localPosition, new Vector3(0, 1.5f, 0), Time.deltaTime * 8f);
        }

        //direction
        var inputMag = Mathf.Clamp01(playerInput.MoveInput.magnitude);
        var relativeDir = transform.TransformDirection(new Vector3(playerInput.MoveInput.x, 0, playerInput.MoveInput.y)).normalized;
        var speed = curStats.GetStat(Stats.Key.Speed) * (playerInput.RunInput ? 1.5f : 1f) * (isCrouch ? 0.5f : 1f);
        var velocity = speed * inputMag * relativeDir;

        //gravity
        if (cc.isGrounded) velocityY = Mathf.Clamp(velocityY, 0, velocityY);
        else velocityY -= gravity * Time.deltaTime;

        if (playerInput.JumpInput && playerInput.IsGround)
            velocityY = jumpScale;

        velocity.y = velocityY;

        //move
        cc.Move(velocity * Time.deltaTime);
    }

    private void DoRotation()
    {
        if (!photonView.IsMine) return;

        //camera rotation
        camRotateX -= playerInput.MouseInput.y * dataContainer.userData.mouseSensivity.y;
        camRotateX = Mathf.Clamp(camRotateX, -camRotateXClamp, camRotateXClamp);
        camView.transform.localEulerAngles = new Vector3(camRotateX, 0, 0);

        //transfom rotate
        transform.Rotate(0, playerInput.MouseInput.x * dataContainer.userData.mouseSensivity.x, 0);
    }

    private void DoAnimator()
    {
        playerModelAnimator.SetFloat(Animator_MoveXHash, playerInput.MoveInput.x);
        playerModelAnimator.SetFloat(Animator_MoveYHash, playerInput.MoveInput.y);
        playerModelAnimator.SetBool(Animator_IsGroundHash, playerInput.IsGround);

        if (playerInput.MoveInput == Vector2.zero) playerModelAnimator.SetInteger(Animator_MoveStateHash, 0);
        else playerModelAnimator.SetInteger(Animator_MoveStateHash, playerInput.RunInput ? 2 : 1); // move : 1, run : 2

        if (photonView.IsMine) //body view
        {
            //arm animator
            armModelAnimator.SetFloat(Animator_MoveXHash, playerInput.MoveInput.x);
            armModelAnimator.SetFloat(Animator_MoveYHash, playerInput.MoveInput.y);
            armModelAnimator.SetBool(Animator_IsGroundHash, playerInput.IsGround);

            if (playerInput.MoveInput == Vector2.zero) armModelAnimator.SetInteger(Animator_MoveStateHash, 0);
            else armModelAnimator.SetInteger(Animator_MoveStateHash, playerInput.RunInput ? 2 : 1); // move : 1, run : 2

            //camera animator
            if (playerInput.MoveInput == Vector2.zero) camAnimator.SetInteger(Animator_MoveStateHash, 0);
            else camAnimator.SetInteger(Animator_MoveStateHash, playerInput.RunInput ? 2 : 1); // move : 1, run : 2

            if (playerInput.JumpInput && playerInput.IsGround)
                camAnimator.SetTrigger(Animator_JumpHash);
            camAnimator.SetBool(Animator_IsGroundHash, playerInput.IsGround);
        }
    }

    private void OnAnimatorIK(int layerIndex)
    {
        //item hand ik
        Debug.Log("test");
        var curItem = itemContainer.GetCurrentSlotItem();

        //body view
        if (curItem != null) //in hand
        {
            if (curItem.LeftHandPoint != null)
            {
                playerModelAnimator.SetIKPosition(AvatarIKGoal.LeftHand, curItem.LeftHandPoint.position);
                playerModelAnimator.SetIKRotation(AvatarIKGoal.LeftHand, curItem.LeftHandPoint.rotation);
                playerModelAnimator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
                playerModelAnimator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
            }
            else
            {
                playerModelAnimator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0);
                playerModelAnimator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0);
            }

            if (curItem.RightHandPoint != null)
            {
                playerModelAnimator.SetIKPosition(AvatarIKGoal.RightHand, curItem.RightHandPoint.position);
                playerModelAnimator.SetIKRotation(AvatarIKGoal.RightHand, curItem.RightHandPoint.rotation);
                playerModelAnimator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
                playerModelAnimator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);
            }
            else
            {
                playerModelAnimator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0);
                playerModelAnimator.SetIKRotationWeight(AvatarIKGoal.RightHand, 0);
            }
        }
        else
        {
            playerModelAnimator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0);
            playerModelAnimator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0);
            playerModelAnimator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0);
            playerModelAnimator.SetIKRotationWeight(AvatarIKGoal.RightHand, 0);
        }

        //camera view
        if (photonView.IsMine)
        {
            if (curItem != null) //in hand
            {
                if (curItem.LeftHandPoint != null)
                {
                    armModelAnimator.SetIKPosition(AvatarIKGoal.LeftHand, curItem.LeftHandPoint.position);
                    armModelAnimator.SetIKRotation(AvatarIKGoal.LeftHand, curItem.LeftHandPoint.rotation);
                    armModelAnimator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
                    armModelAnimator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
                }
                else
                {
                    armModelAnimator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0);
                    armModelAnimator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0);
                }

                if (curItem.RightHandPoint != null)
                {
                    armModelAnimator.SetIKPosition(AvatarIKGoal.RightHand, curItem.RightHandPoint.position);
                    armModelAnimator.SetIKRotation(AvatarIKGoal.RightHand, curItem.RightHandPoint.rotation);
                    armModelAnimator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
                    armModelAnimator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);
                }
                else
                {
                    armModelAnimator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0);
                    armModelAnimator.SetIKRotationWeight(AvatarIKGoal.RightHand, 0);
                }
            }
            else
            {
                armModelAnimator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0);
                armModelAnimator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0);
                armModelAnimator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0);
                armModelAnimator.SetIKRotationWeight(AvatarIKGoal.RightHand, 0);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(cam.transform.position, cam.transform.position + cam.transform.forward * interactionDistance);
    }
}