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
using Cysharp.Threading.Tasks;

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
    private DataContainer _dataContainer;
    private DataContainer dataContainer => _dataContainer ??= ServiceLocator.ForGlobal().Get<DataContainer>();
    private ItemManager _itemManager;
    private ItemManager itemManager => _itemManager ??= ServiceLocator.For(this).Get<ItemManager>();
    private Basement _basement;
    private Basement basement => _basement ??= ServiceLocator.For(this).Get<Basement>();
    private GameManager _gameManager;
    private GameManager gameManager => _gameManager ??= ServiceLocator.For(this).Get<GameManager>();
    private UserInput input => UserInput.input;

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
    private bool isInBasement;
    private float velocityY;
    private float camRotateX;
    private float scanWaitTime;
    private float runStaminaRecoverWaitTime;
    private float[] interactRequireTimes = new float[(int)InteractID.End];
    private Camera cam;
    private Vector2 moveInput;
    private Vector3 velocity;
    private ScanData scanData;
    private Material scanSphereMaterial;
    private CharacterController cc;
    private IInteractable lookInteractable;
    private Stats testBaseStat; //test base stat(need to change)
    private ReactiveProperty<int> curHandItemViewId = new();
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
    public Transform Head => animator.GetHeadTransform();

    public void Damage(float damage)
    {
        photonView.RPC(nameof(SendDamageToOwnerRpc), photonView.Owner, damage);
    }

    [PunRPC]
    private void SendDamageToOwnerRpc(float damage)
    {
        curStats.SetStat(Stats.Key.Hp, x => x - damage);
    }

    public void SetPositionAndRotation(Vector3 position, Quaternion rotation)
    {
        photonView.RPC(nameof(SetPositionAndRotationRpc), RpcTarget.All, position, rotation);
    }

    [PunRPC]
    private void SetPositionAndRotationRpc(Vector3 position, Quaternion rotation)
    {
        transform.SetPositionAndRotation(position, rotation);
        Debug.LogError(transform.position);
    }

    public void SetCamera()
    {
        cam.transform.SetParent(camHolder);
        cam.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.Euler(0, 0, 0));
    }

    public void Revive()
    {
        photonView.RPC(nameof(SendReviveToOwnerRpc), photonView.Owner);
    }

    [PunRPC]
    private void SendReviveToOwnerRpc()
    {
        animator.SetActiveArmModel(true);
        animator.SetActivePlayerModel(false);

        input.Player.Enable();
        gameObject.SetActive(true);
        curStats.ChangeFrom(maxStats);
        gameManager.SendPlayerState(PhotonNetwork.LocalPlayer, true);
        SetCamera();
        cc.enabled = true;
        isDie = false;
    }

    public override void OnCreate()
    {
        base.OnCreate();

        gameObject.SetActive(false);

        //cache and initialize member
        cc = GetComponent<CharacterController>();
        animator = GetComponent<InGamePlayerAnimator>();
        animator.Initialize(this);

        //ik setting
        if (!photonView.IsMine) return;

        curStats.OnStatChanged += (key, prev, cur) =>
        {
            if (key == Stats.Key.Hp)
                Debug.LogError(curStats.GetStat(key));
        };

        cc.enabled = true;

        //set item container
        inventory = new(4);
        inventory.OnIndexChanged += (pre, cur) =>
        {
            inventory[pre]?.OnInactive();
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
        scanSphereMaterial = scanSphere.GetComponent<MeshRenderer>().sharedMaterial;
        cam = Camera.main;

        //scroll
        this
            .UpdateAsObservable()
            .Select(_ => input.Player.MouseWheel.ReadValue<float>())
            .Where(x => x != 0)
            .ThrottleFrame(2)
            .Subscribe(x =>
            {
                Debug.Log($"mouse wheel: {x}");
                inventory.Index += x > 0 ? -1 : 1;
            });

        ServiceLocator.For(this).Register(this);
    }

    private void Start()
    {
        this
            .ObserveEveryValueChanged(t => t.isInBasement)
            .Subscribe(x =>
            {
                if (isInBasement) transform.SetParent(basement.transform);
                else transform.SetParent(null);
            });

        transform
            .ObserveEveryValueChanged(t => transform.parent)
            .Subscribe(parent => isInBasement = ReferenceEquals(parent, basement.transform));
    }

    private void Update()
    {
        DoLife();
        DoScan();
        DoItem();
        DoInteract();
        DoMovement();
        DoRotation();
    }

    private void FixedUpdate()
    {
        cc.Move(velocity * Time.fixedDeltaTime);
    }

    private void DoLife()
    {
        if (!photonView.IsMine) return;

        if (!isDie && curStats.GetStat(Stats.Key.Hp) <= 0)
        {
            Debug.LogError("Die");
            isDie = true;
            input.Player.Disable();
            animator.SetActivePlayerModel(true);
            animator.SetActiveArmModel(false);
            cc.enabled = false;
            this.Invoke(() => gameManager.SendPlayerState(PhotonNetwork.LocalPlayer, false), 1.5f);
        }
    }
    private void DoScan()
    {
        if (isDie) return;
        if (!photonView.IsMine) return;

        if (scanWaitTime > 0)
        {
            scanWaitTime -= Time.deltaTime;
        }
        else
        {
            if (input.Player.Scan.WasPressedThisFrame())
            {
                scanWaitTime = 1.33f;
                ScanRoutine().Forget();
            }
        }
    }

    private async UniTask ScanRoutine()
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
        await UniTask.WaitForSeconds(0.5f);

        var startColor = scanSphereMaterial.color;
        var targetColor = new Color(startColor.r, startColor.g, startColor.b, 0);
        scanSphereMaterial.DOColor(targetColor, 0.5f);
        await UniTask.WaitForSeconds(0.5f);

        scanSphere.gameObject.SetActive(false);
        scanSphereMaterial.color = startColor;
        scanSphere.localScale = Vector3.one * 0.1f;
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
        if (isDie) return;
        if (!photonView.IsMine) return;

        var item = inventory.GetCurrentSlotItem();
        if (item != null)
        {
            curHandItemViewId.Value = item.photonView.ViewID;

            if (input.Player.Interact.WasPressedThisFrame())
            {
                var interactId = (InteractID)(int)input.Player.Interact.ReadValue<float>();
                if (item.IsUsable(interactId))
                    item.OnUsePressed(interactId);
            }

            if (input.Player.Interact.WasReleasedThisFrame())
                item.OnUseReleased();

            if (input.Player.Discard.WasPressedThisFrame())
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
        if (isDie) return;
        if (!photonView.IsMine) return;
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out var hit, interactionDistance, ~LayerMask.GetMask("Ignore Raycast", "Player")))
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
                if (input.Player.Interact.WasPressedThisFrame() && input.Player.Interact.ReadValue<float>() == (int)targetInteractId)
                    interactRequireTimes[(int)targetInteractId] += Time.deltaTime;

                //if released target interact id => set require time 0
                if (input.Player.Interact.WasReleasedThisFrame())
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

        if (isDie) return;
        if (!photonView.IsMine) return;

        //crouch
        if (input.Player.Crouch.WasPressedThisFrame())
            isCrouch = !isCrouch;

        if (isCrouch)
        {
            if (input.Player.Run.IsPressed())
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
        moveInput = input.Player.Move.ReadValue<Vector2>();
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
            if (input.Player.Run.IsPressed() && curStats.GetStat(Stats.Key.Stamina) >= 0.5f) isRun = true;
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
            if (!input.Player.Run.IsPressed()) isRun = false;
        }
        velocity = speed * moveInputMag * relativeDir;

        //gravity
        if (cc.isGrounded) velocityY = Mathf.Clamp(velocityY, 0, velocityY);
        else velocityY -= gravity * Time.deltaTime;

        if (input.Player.Jump.WasPressedThisFrame() && isGround)
        {
            isJump = true;
            velocityY = jumpScale;
        }
        else
        {
            isJump = false;
        }

        velocity.y = velocityY;
    }

    private void DoRotation()
    {
        if (!photonView.IsMine) return;

        var mouseInput = input.Player.Mouse.ReadValue<Vector2>();// * Time.deltaTime;

        //camera rotation
        camRotateX -= mouseInput.y * dataContainer.userData.mouseSensivity.y;
        camRotateX = Mathf.Clamp(camRotateX, -camRotateXClamp, camRotateXClamp);
        camView.localEulerAngles = new Vector3(camRotateX, 0, 0);

        //transfom rotate
        transform.Rotate(0, mouseInput.x * dataContainer.userData.mouseSensivity.x, 0);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position + groundCastOffset, groundCastRadius);

        // Gizmos.color = Color.green;
        // Gizmos.DrawLine(cam.transform.position, cam.transform.position + cam.transform.forward * interactionDistance);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(isInBasement);
            stream.SendNext(gameObject.activeSelf);
            stream.SendNext(curHandItemViewId.Value);
        }
        else
        {
            isInBasement = (bool)stream.ReceiveNext();
            gameObject.SetActive((bool)stream.ReceiveNext());
            curHandItemViewId.Value = (int)stream.ReceiveNext();
        }
    }
}