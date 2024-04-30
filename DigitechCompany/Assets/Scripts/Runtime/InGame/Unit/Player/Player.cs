using Photon.Pun;
using UnityEngine;
using UnityEngine.Rendering;
using Game.Service;

namespace Game.InGame
{
    public class Player : UnitBase
    {
        //const
        private readonly static int Animator_MoveXHash = Animator.StringToHash("MoveX");
        private readonly static int Animator_MoveYHash = Animator.StringToHash("MoveY");
        private readonly static int Animator_IsRunHash = Animator.StringToHash("IsRun");
        private readonly static int Animator_IsGroundHash = Animator.StringToHash("IsGround");

        //service
        private DataContainer dataContainer;

        //inspector field
        [Header("Value")]
        [SerializeField] private float moveSpeed;
        [SerializeField] private float gravity;
        [SerializeField] private float jumpScale;
        [SerializeField] private float camRotateXClamp;
        [Header("Reference")]
        [SerializeField] private Camera cam;
        [SerializeField] private Transform itemHolder;
        [SerializeField] private Transform itemCameraHolder;
        [Header("Animator")]
        [SerializeField] private Animator playerModel;
        [SerializeField] private Animator armModel;

        //field
        private int itemSlotIndex;
        private bool isGround;
        private float camRotateX;
        private float velocityY;
        private PlayerInput playerInput;
        private CharacterController cc;
        private Stats testBaseStat = new(); //test base stat(need to change)
        private IInteractable lookInteractable;
        private ItemBase[] itemSlots = new ItemBase[4];

        //property
        public IInteractable LookInteractable => lookInteractable;
        public override Stats BaseStats => testBaseStat;

        protected override void Awake()
        {
            base.Awake();
            if (!photonView.IsMine) return;

            cc = GetComponent<CharacterController>();
            playerInput = GetComponent<PlayerInput>();

            playerModel.GetComponentInChildren<SkinnedMeshRenderer>().shadowCastingMode = ShadowCastingMode.ShadowsOnly;
            cam.gameObject.SetActive(true);

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
            DoInteract();
            DoMovement();
            DoRotation();
            DoAnimator();
        }

        private void DoInteract()
        {
            if (!photonView.IsMine) return;

            if(Physics.Raycast(cam.transform.position, cam.transform.forward, out var hit, 10f, ~LayerMask.GetMask("Player")))
                hit.collider.TryGetComponent(out lookInteractable);
            else
                lookInteractable = null;

            if(Input.GetKeyDown(KeyCode.E) && lookInteractable != null)
                lookInteractable.OnInteract(this);
        }

        private void DoMovement()
        {
            if (!photonView.IsMine) return;

            //direction
            var inputMag = Mathf.Clamp01(playerInput.MoveInput.magnitude);
            var relativeDir = transform.TransformDirection(new Vector3(playerInput.MoveInput.x, 0, playerInput.MoveInput.y)).normalized;
            var velocity = moveSpeed * inputMag * relativeDir;

            //gravity
            if (cc.isGrounded) velocityY = Mathf.Clamp(velocityY, 0, velocityY);
            else velocityY -= gravity * Time.deltaTime;

            if (Input.GetKeyDown(KeyCode.Space) && playerInput.IsGround)
                velocityY = jumpScale;

            //jump
            if (isGround && Input.GetKeyDown(KeyCode.Space)) velocityY = jumpScale;

            velocity.y = velocityY;

            //move
            cc.Move(velocity * Time.deltaTime);
        }

        private void DoRotation()
        {
            if (!photonView.IsMine) return;

            //camera rotation
            camRotateX -= playerInput.MouseInput.y * dataContainer.settingData.mouseSensivity.y;
            camRotateX = Mathf.Clamp(camRotateX, -camRotateXClamp, camRotateXClamp);
            cam.transform.localEulerAngles = new Vector3(camRotateX, 0, 0);

            //transfom rotate
            transform.Rotate(0, playerInput.MouseInput.x * dataContainer.settingData.mouseSensivity.x, 0);
        }

        private void DoAnimator()
        {
            if (!photonView.IsMine) //other view
            {
                playerModel.SetFloat(Animator_MoveXHash, playerInput.MoveInput.x);
                playerModel.SetFloat(Animator_MoveYHash, playerInput.MoveInput.y);
                playerModel.SetBool(Animator_IsRunHash, playerInput.MoveInput != Vector2.zero);
                playerModel.SetBool(Animator_IsGroundHash, isGround);
            }
            else //my view
            {
                armModel.SetFloat(Animator_MoveXHash, playerInput.MoveInput.x);
                armModel.SetFloat(Animator_MoveYHash, playerInput.MoveInput.y);
                armModel.SetBool(Animator_IsRunHash, playerInput.MoveInput != Vector2.zero);
                armModel.SetBool(Animator_IsGroundHash, isGround);
            }
        }
    }
}