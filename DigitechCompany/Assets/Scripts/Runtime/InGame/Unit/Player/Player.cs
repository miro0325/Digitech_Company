using Photon.Pun;
using UnityEngine;
using UnityEngine.Rendering;
using UniRx;
using System.Collections;
using DG.Tweening;
using System.Collections.Generic;

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
    private ItemManager itemManager;

    //inspector field
    [Space(20)]
    [Header("Player")]
    [SerializeField] private float gravity;
    [SerializeField] private float jumpScale;
    [SerializeField] private float camRotateXClamp;
    [SerializeField] private float interactionDistance;
    [Header("Reference")]
    [SerializeField] private Transform camView;
    [SerializeField] private Camera cam;
    [SerializeField] private Transform itemHolderCamera;
    [SerializeField] private Transform scanSphere;
    [Header("Animator")]
    [SerializeField] private Animator camAnimator;
    [SerializeField] private Animator playerModelAnimator;
    [SerializeField] private Animator armModelAnimator;

    //field
    private bool isRun;
    private bool isCrouch;
    private float runStaminaRecoverWaitTime;
    private float scanWaitTime;
    private float camRotateX;
    private float velocityY;
    private float interactRequireTime;
    private ScanData scanData;
    private Material scanSphereMaterial;
    private PlayerInput playerInput;
    private CharacterController cc;
    private IInteractable lookInteractable;
    private Stats testBaseStat; //test base stat(need to change)

    //property
    public IInteractable LookInteractable => lookInteractable;
    public ScanData ScanData => scanData;
    public Transform ItemHolderCamera => itemHolderCamera;
    public override Stats BaseStats => testBaseStat;

    public override void OnCreate()
    {
        base.OnCreate();
        playerInput = GetComponent<PlayerInput>();

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

        playerModelAnimator.GetComponentsInChildren<SkinnedMeshRenderer>().For((i, ele) => ele.shadowCastingMode = ShadowCastingMode.ShadowsOnly);
        camView.gameObject.SetActive(true);

        playerInput
            .ObserveEveryValueChanged(i => i.MouseWheel)
            .Where(x => x != 0)
            .ThrottleFrame(5)
            .Subscribe(x => itemContainer.Index += x > 0 ? 1 : -1);

        scanSphereMaterial = scanSphere.GetComponent<MeshRenderer>().sharedMaterial;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Services.Register(this);
    }

    private void Start()
    {
        dataContainer = Services.Get<DataContainer>();
        itemManager = Services.Get<ItemManager>();
    }

    private void Update()
    {
        DoScan();
        DoItem();
        DoInteract();
        DoMovement();
        DoRotation();
        DoAnimator();
    }

    private void DoScan()
    {
        if (!photonView.IsMine) return;

        if (scanWaitTime > 0)
        {
            scanWaitTime -= Time.deltaTime;
        }
        else
        {
            if (playerInput.ScanInput)
            {
                scanWaitTime = 1.25f;
                StartCoroutine(ScanRoutine());
            }
        }

        IEnumerator ScanRoutine()
        {
            //calculate
            scanData = new ScanData { gameTime = Time.time, items = new() };
            foreach (var item in itemManager.Items)
            {
                if (item.InHand) continue;

                if (IsInView(item.MeshRenderer))
                {
                    scanData.price += item.SellPrice;
                    scanData.items.Add(item);
                }
            }

            //animation
            scanSphere.gameObject.SetActive(true);
            scanSphere.DOScale(Vector3.one * 30, 1f).SetEase(Ease.OutQuart);
            yield return new WaitForSeconds(0.5f);

            var startColor = scanSphereMaterial.color;
            var targetColor = startColor;
            targetColor.a = 0;
            scanSphereMaterial.DOColor(targetColor, 0.5f);
            yield return new WaitForSeconds(0.5f);

            scanSphere.gameObject.SetActive(false);
            scanSphereMaterial.color = startColor;
            scanSphere.localScale = Vector3.one * 0.1f;
        }
    }

    private bool IsInView(Renderer toCheck)
    {
        Vector3 pointOnScreen = cam.WorldToScreenPoint(toCheck.bounds.center);

        //Is inactive
        if (!toCheck.gameObject.activeSelf)
            return false;

        //Is in front
        if (pointOnScreen.z < 0)
            return false;

        //Is in FOV
        if ((pointOnScreen.x < 0) || (pointOnScreen.x > Screen.width) || (pointOnScreen.y < 0) || (pointOnScreen.y > Screen.height))
            return false;

        //Is covered
        if (Physics.Linecast(cam.transform.position, toCheck.bounds.center, out _, LayerMask.GetMask("Ground")))
            return false;

        return true;
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
        if (item == null) return;

        itemContainer.PopCurrentItem();
        item.SetLayRotation(transform.eulerAngles.y);
        item.OnDiscard();
    }

    private void DoInteract()
    {
        if (!photonView.IsMine) return;
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out var hit, interactionDistance, ~LayerMask.GetMask("Player")))
            hit.collider.TryGetComponent(out lookInteractable);
        else
            lookInteractable = null;

        if (lookInteractable != null && LookInteractable.IsInteractable(this) && playerInput.InteractInputs[(int)lookInteractable.GetTargetInteractID(this)])
        {
            if (interactRequireTime < lookInteractable.GetInteractRequireTime(this))
            {
                interactRequireTime += Time.deltaTime;
            }
            else
            {
                interactRequireTime = 0;
                if (lookInteractable is ItemBase)
                {
                    var item = lookInteractable as ItemBase;
                    itemContainer.InsertItem(item);
                    item.OnInteract(this);
                    item.OnActive();
                }
                else
                {
                    lookInteractable.OnInteract(this);
                }
            }
        }
        else
        {
            interactRequireTime = 0;
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

        if (!photonView.IsMine) return;

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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(cam.transform.position, cam.transform.position + cam.transform.forward * interactionDistance);
    }
}