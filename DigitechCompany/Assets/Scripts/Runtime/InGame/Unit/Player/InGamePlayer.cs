using Photon.Pun;
using UnityEngine;
using UnityEngine.Rendering;
using UniRx;
using System.Collections;
using DG.Tweening;
using UnityEngine.Animations.Rigging;
using System;
using UnityEngine.InputSystem;
using UniRx.Triggers;
using UnityEngine.InputSystem.Composites;

public enum InteractID
{
    None,
    ID1,
    ID2,
    ID3,
    ID4,
    End
}

public partial class InGamePlayer : UnitBase, IService, IPunObservable
{
    //service
    //service
    private DataContainer _dataContainer;
    private DataContainer dataContainer => _dataContainer ??= ServiceLocator.ForGlobal().Get<DataContainer>();
    private ItemManager _itemManager;
    private ItemManager itemManager => _itemManager ??= ServiceLocator.For(this).Get<ItemManager>();

    //inspector field
    [Space(20)]
    [Header("Player")]
    [SerializeField] private float gravity;
    [SerializeField] private float jumpScale;
    [SerializeField] private float camRotateXClamp;
    [SerializeField] private float interactionDistance;
    [SerializeField] private Vector3 groundCastOffset;
    [SerializeField] private float groundCastRadius;
    [SerializeField] private LayerMask groundCastMask;
    [Header("Reference")]
    [SerializeField] private Transform camView;
    [SerializeField] private Transform camHolder;
    [SerializeField] private Transform itemHolderCamera;
    [SerializeField] private Transform scanSphere;

    //field
    private bool isRun;
    private bool isCrouch;
    private bool isGround;
    private bool isJump;
    private bool isDie = true;
    private float velocityY;
    private float camRotateX;
    private float scanWaitTime;
    private float runStaminaRecoverWaitTime;
    private float[] interactRequireTimes = new float[(int)InteractID.End];
    private Camera cam;
    private Vector2 moveInput;
    private ScanData scanData;
    private Material scanSphereMaterial;
    private CharacterController cc;
    private IInteractable lookInteractable;
    private Stats testBaseStat; //test base stat(need to change)
    private ReactiveProperty<int> curHandItemViewId = new();
    private UserInputAction userInput;
    private InGamePlayerAnimator animator;

    //property
    public bool IsRun => isRun;
    public bool IsCrouch => isCrouch;
    public bool IsGround => isGround;
    public bool IsJump => isJump;
    public bool IsDie => isDie;
    public Vector2 MoveInput => moveInput;
    public float[] InteractRequireTimes => interactRequireTimes;
    public IInteractable LookInteractable => lookInteractable;
    public ScanData ScanData => scanData;
    public Transform ItemHolderCamera => itemHolderCamera;
    public IReadOnlyReactiveProperty<int> CurrentHandItemViewID => curHandItemViewId;
    public override Stats BaseStats => testBaseStat;

    public void SetPosition(Vector3 pos)
    {
        cc.enabled = false;
        transform.position = pos;
        cc.enabled = true;
    }

    public void Revive()
    {
        cam.transform.SetParent(camHolder);
        cam.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.Euler(0, 0, 0));
        gameObject.SetActive(true);
        curStats.ChangeFrom(maxStats);
        isDie = false;
    }

    public override void OnCreate()
    {
        gameObject.SetActive(false);
        base.OnCreate();

        //cache and initialize member
        cc = GetComponent<CharacterController>();
        animator = GetComponent<InGamePlayerAnimator>();
        animator.Initialize(this);

        //ik setting

        if (!photonView.IsMine) return;

        //set item container
        inventory = new(4);
        inventory.OnIndexChanged += (pre, cur) =>
        {
            inventory[pre]?.OnDisable();
            inventory[cur]?.OnActive();
        };

        //set test stat
        testBaseStat = new();
        testBaseStat.SetStat(Stats.Key.Hp, x => 100);
        testBaseStat.SetStat(Stats.Key.Strength, x => 10);
        testBaseStat.SetStat(Stats.Key.Weight, x => 80);
        testBaseStat.SetStat(Stats.Key.Speed, x => 3);
        testBaseStat.SetStat(Stats.Key.Stamina, x => 5);
        maxStats.ChangeFrom(testBaseStat);

        camView.gameObject.SetActive(true);

        //cache initialize
        userInput = new();
        scanSphereMaterial = scanSphere.GetComponent<MeshRenderer>().sharedMaterial;
        cam = Camera.main;

        //enable player input
        this
            .ObserveEveryValueChanged(t => t.isDie)
            .Subscribe(isDie =>
            {
                if(isDie) userInput.Player.Disable();
                else userInput.Player.Enable();
            });

        //scroll
        this
            .UpdateAsObservable()
            .Select(_ => userInput.Player.MouseWheel.ReadValue<float>())
            .Where(x => x != 0)
            .ThrottleFrame(2)
            .Subscribe(x =>
            {
                Debug.Log($"mouse wheel: {x}");
                inventory.Index += x > 0 ? -1 : 1;
            });

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        ServiceLocator.For(this).Register(this);
    }

    private void Update()
    {
        DoScan();
        DoItem();
        DoInteract();
        DoMovement();
        DoRotation();
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
            if (userInput.Player.Scan.WasPressedThisFrame())
            {
                scanWaitTime = 1.33f;
                StartCoroutine(ScanRoutine());
            }
        }

        IEnumerator ScanRoutine()
        {
            //calculate
            scanData = new ScanData { gameTime = Time.time, items = new() };
            foreach (var kvp in itemManager.Items)
            {
                if (kvp.Value.InHand) continue;

                if (IsInView(kvp.Value.MeshRenderer))
                {
                    scanData.price += kvp.Value.SellPrice;
                    scanData.items.Add(kvp.Value);
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

        var item = inventory.GetCurrentSlotItem();
        if (item != null)
        {
            curHandItemViewId.Value = item.photonView.ViewID;

            if (userInput.Player.Interact.WasPressedThisFrame())
            {
                for (int i = 1; i < (int)InteractID.End; i++)
                {
                    if (userInput.Player.Interact.ReadValue<float>() == i && item.IsUsable((InteractID)i))
                        item.OnUse((InteractID)i);
                }
            }

            if (userInput.Player.Discard.WasPressedThisFrame())
                DiscardCurrentItem();
        }
        else
        {
            curHandItemViewId.Value = 0;
        }
    }

    public ItemBase DiscardCurrentItem()
    {
        var item = inventory.GetCurrentSlotItem();
        if (item == null) return null;

        inventory.PopCurrentItem();
        item.SetLayRotation(transform.eulerAngles.y);
        item.OnDiscard();
        return item;
    }

    private void DoInteract()
    {
        if (!photonView.IsMine) return;
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out var hit, interactionDistance, ~LayerMask.GetMask("Player")))
            hit.collider.TryGetComponent(out lookInteractable);
        else
            lookInteractable = null;

        if (lookInteractable != null && LookInteractable.IsInteractable(this))
        {
            Debug.Log("Has Interactable");
            var targetInteractId = lookInteractable.GetTargetInteractID(this);
            if (targetInteractId != InteractID.None)
            {
                Debug.Log("Target Interact ID is not None");
                //if pressed target interact id => mark interact is start
                if (userInput.Player.Interact.WasPressedThisFrame() && userInput.Player.Interact.ReadValue<float>() == (int)targetInteractId)
                    interactRequireTimes[(int)targetInteractId] += Time.deltaTime;

                //if released target interact id => set require time 0
                if (userInput.Player.Interact.WasReleasedThisFrame())
                    interactRequireTimes[(int)targetInteractId] = 0;

                //if interact marked
                if (interactRequireTimes[(int)targetInteractId] > 0)
                {
                    //less than require
                    if (interactRequireTimes[(int)targetInteractId] < lookInteractable.GetInteractRequireTime(this))
                    {
                        interactRequireTimes[(int)targetInteractId] += Time.deltaTime;
                    }
                    else //reached
                    {
                        interactRequireTimes[(int)targetInteractId] = 0;

                        if (lookInteractable is ItemBase)
                        {
                            var item = lookInteractable as ItemBase;
                            inventory.InsertItem(item);
                            item.OnInteract(this);
                            item.OnActive();
                        }
                        else
                        {
                            lookInteractable.OnInteract(this);
                        }
                    }
                }
            }
        }
        else
        {
            for (int i = 0; i < (int)InteractID.End; i++)
                interactRequireTimes[i] = 0;
        }
    }

    private void DoMovement()
    {
        isGround = Physics.CheckSphere(transform.position + groundCastOffset, groundCastRadius, groundCastMask);

        if (!photonView.IsMine) return;

        //crouch
        if (userInput.Player.Crouch.WasPressedThisFrame())
            isCrouch = !isCrouch;

        if (isCrouch)
        {
            if (userInput.Player.Run.IsPressed())
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
        moveInput = userInput.Player.Move.ReadValue<Vector2>();
        var moveInputMag = Mathf.Clamp01(moveInput.magnitude);
        var relativeDir = transform.TransformDirection(new Vector3(moveInput.x, 0, moveInput.y)).normalized;
        var weightShave = Mathf.Lerp(1f, 0.5f, inventory.WholeWeight / curStats.GetStat(Stats.Key.Weight));
        var crouchShave = isCrouch ? 0.5f : 1f;
        var speed = curStats.GetStat(Stats.Key.Speed) * crouchShave * weightShave * moveInputMag;

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
            if (userInput.Player.Run.IsPressed() && curStats.GetStat(Stats.Key.Stamina) >= 0.5f) isRun = true;
        }
        else
        {
            speed *= 1.5f;
            if (moveInputMag > 0)
            {
                curStats.SetStat(Stats.Key.Stamina, x => x -= Time.deltaTime);
                runStaminaRecoverWaitTime = 1.5f;
            }

            if (curStats.GetStat(Stats.Key.Stamina) <= 0) isRun = false;
            if (!userInput.Player.Run.IsPressed()) isRun = false;
        }
        var velocity = speed * moveInputMag * relativeDir;

        //gravity
        if (cc.isGrounded) velocityY = Mathf.Clamp(velocityY, 0, velocityY);
        else velocityY -= gravity * Time.deltaTime;

        if (userInput.Player.Jump.WasPressedThisFrame() && isGround)
        {
            isJump = true;
            velocityY = jumpScale;
        }
        else
        {
            isJump = false;
        }

        velocity.y = velocityY;

        //move
        cc.Move(velocity * Time.deltaTime);
    }

    private void DoRotation()
    {
        if (!photonView.IsMine) return;

        var mouseInput = userInput.Player.Mouse.ReadValue<Vector2>();// * Time.deltaTime;

        //camera rotation
        camRotateX -= mouseInput.y * dataContainer.userData.mouseSensivity.y;
        camRotateX = Mathf.Clamp(camRotateX, -camRotateXClamp, camRotateXClamp);
        camView.localEulerAngles = new Vector3(camRotateX, 0, 0);

        //transfom rotate
        transform.Rotate(0, mouseInput.x * dataContainer.userData.mouseSensivity.x, 0);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position + groundCastOffset, groundCastRadius);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(cam.transform.position, cam.transform.position + cam.transform.forward * interactionDistance);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        Debug.Log("Test");
        if (stream.IsWriting)
        {
            stream.SendNext(gameObject.activeSelf);
            stream.SendNext(curHandItemViewId.Value);
        }
        else
        {
            gameObject.SetActive((bool)stream.ReceiveNext());
            curHandItemViewId.Value = (int)stream.ReceiveNext();
        }
    }
}