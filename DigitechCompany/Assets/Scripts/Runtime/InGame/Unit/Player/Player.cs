using Photon.Pun;
using UnityEngine;
using UnityEngine.Rendering;
using UniRx;

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
    private bool isRun;
    private bool isCrouch;
    private float runStaminaRecoverWaitTime;
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
            itemContainer[pre]?.OnDisable();
            itemContainer[cur]?.OnActive();
        };

        testBaseStat = new();
        testBaseStat.SetStat(Stats.Key.Hp, x => 100);
        testBaseStat.SetStat(Stats.Key.Strength, x => 10);
        testBaseStat.SetStat(Stats.Key.Weight, x => 80);
        testBaseStat.SetStat(Stats.Key.Speed, x => 3);
        testBaseStat.SetStat(Stats.Key.Stamina, x => 5);
        maxStats.ChangeFrom(testBaseStat);

        cc = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();

        playerModelAnimator.GetComponentsInChildren<SkinnedMeshRenderer>().For((i, ele) => ele.shadowCastingMode = ShadowCastingMode.ShadowsOnly);
        camView.gameObject.SetActive(true);

        playerInput
            .ObserveEveryValueChanged(i => i.MouseWheel)
            .Where(x => x != 0)
            .ThrottleFrame(5)
            .Subscribe(x => itemContainer.Index += x > 0 ? 1 : -1);

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
        DoItem();
        DoInteract();
        DoMovement();
        DoRotation();
        DoAnimator();
    }

    private void DoItem()
    {
        if (!photonView.IsMine) return;

        var item = itemContainer.GetCurrentSlotItem();
        if (item != null)
        {
            for (int i = 0; i < (int)InteractID.End; i++)
            {
                if (playerInput.InteractInputs[i] && item.IsUsable((InteractID)i))
                    item.OnUse((InteractID)i);
            }

            if (playerInput.DiscardInput)
                DiscardCurrentItem();
        }
    }

    public void DiscardCurrentItem()
    {
        var item = itemContainer.GetCurrentSlotItem();
        if(item == null) return;

        itemContainer.PopCurrentItem();
        item.LayRotation = transform.eulerAngles.y;
        item.OnDiscard();
        item.transform.SetParent(null);

        photonView.RPC(nameof(SetItemParentRpc), RpcTarget.Others, item.guid, true);
    }

    [PunRPC]
    private void SetItemParentRpc(string guid, bool isThrow)
    {
        var item = NetworkObject.GetNetworkObject(guid);
        item.transform.SetParent(isThrow ? null : itemHolder);
    }

    private void DoInteract()
    {
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out var hit, interactionDistance, ~LayerMask.GetMask("Player")))
            hit.collider.TryGetComponent(out lookInteractable);
        else
            lookInteractable = null;

        if (!photonView.IsMine) return;
        if (lookInteractable != null && LookInteractable.IsInteractable(this) && playerInput.InteractInputs[(int)lookInteractable.TargetInteractID])
        {
            if (lookInteractable is ItemBase)
            {
                var item = lookInteractable as ItemBase;
                if (itemContainer.TryInsertItem(item))
                {
                    item.transform.SetParent(itemHolderCamera);
                    item.OnInteract(this);
                    item.OnActive();
                    photonView.RPC(nameof(SetItemParentRpc), RpcTarget.Others, item.guid, false);
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

        //movement
        var inputMag = Mathf.Clamp01(playerInput.MoveInput.magnitude);
        var relativeDir = transform.TransformDirection(new Vector3(playerInput.MoveInput.x, 0, playerInput.MoveInput.y)).normalized;
        var weightShave = Mathf.Lerp(1f, 0.5f, itemContainer.WholeWeight / curStats.GetStat(Stats.Key.Weight));
        var crouchShave = isCrouch ? 0.5f : 1f;
        var speed = curStats.GetStat(Stats.Key.Speed) * crouchShave * weightShave;

        if (!isRun)
        {
            //wait recover time
            if (runStaminaRecoverWaitTime > 0)
            {
                runStaminaRecoverWaitTime -= Time.deltaTime;
            }
            else
            {
                //recover stamina
                if (curStats.GetStat(Stats.Key.Stamina) <= maxStats.GetStat(Stats.Key.Stamina))
                    curStats.SetStat(Stats.Key.Stamina, x => x += Time.deltaTime);
            }

            //setting stamina minimums for running
            if (playerInput.RunInput && curStats.GetStat(Stats.Key.Stamina) >= 0.5f) isRun = true;
        }
        else
        {
            speed *= 1.5f;
            curStats.SetStat(Stats.Key.Stamina, x => x -= Time.deltaTime);
            runStaminaRecoverWaitTime = 1.5f;

            if (curStats.GetStat(Stats.Key.Stamina) <= 0) isRun = false;
            if (!playerInput.RunInput) isRun = false;
        }
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
        else playerModelAnimator.SetInteger(Animator_MoveStateHash, isRun ? 2 : 1); // move : 1, run : 2

        if (photonView.IsMine) //body view
        {
            //arm animator
            armModelAnimator.SetFloat(Animator_MoveXHash, playerInput.MoveInput.x);
            armModelAnimator.SetFloat(Animator_MoveYHash, playerInput.MoveInput.y);
            armModelAnimator.SetBool(Animator_IsGroundHash, playerInput.IsGround);

            if (playerInput.MoveInput == Vector2.zero) armModelAnimator.SetInteger(Animator_MoveStateHash, 0);
            else armModelAnimator.SetInteger(Animator_MoveStateHash, isRun ? 2 : 1); // move : 1, run : 2

            //camera animator
            if (playerInput.MoveInput == Vector2.zero) camAnimator.SetInteger(Animator_MoveStateHash, 0);
            else camAnimator.SetInteger(Animator_MoveStateHash, isRun ? 2 : 1); // move : 1, run : 2

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