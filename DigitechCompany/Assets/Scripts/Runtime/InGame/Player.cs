using Photon.Pun;
using UnityEngine;
using UnityEngine.Rendering;
using Game.Service;

namespace Game.InGame
{
    public class Player : MonoBehaviourPun
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
        [SerializeField] private Vector3 groundCastOffset;
        [SerializeField] private float groundCastRadius;
        [SerializeField] private LayerMask groundCastMask;
        [Header("Reference")]
        [SerializeField] private Camera cam;
        [Header("Animator")]
        [SerializeField] private Animator playerModel;
        [SerializeField] private Animator armModel;

        //field
        private float camRotateX;
        private float velocityY;
        private CharacterController cc;

        private void Awake()
        {
            cc = GetComponent<CharacterController>();

            if (!photonView.IsMine) return;
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
            var input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            var isGround = Physics.CheckSphere(transform.position + groundCastOffset, groundCastRadius, groundCastMask);

            playerModel.SetFloat(Animator_MoveXHash, input.x);
            playerModel.SetFloat(Animator_MoveYHash, input.y);
            playerModel.SetBool(Animator_IsRunHash, input != Vector2.zero);
            playerModel.SetBool(Animator_IsGroundHash, isGround);

            if (!photonView.IsMine) return;
            var mouseInput = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
            var inputMag = Mathf.Clamp01(input.magnitude);
            var relativeDir = transform.TransformDirection(new Vector3(input.x, 0, input.y)).normalized;
            var velocity = moveSpeed * inputMag * relativeDir;

            if(cc.isGrounded) velocityY = Mathf.Clamp(velocityY, 0, velocityY);
            else velocityY -= gravity * Time.deltaTime;

            if(isGround && Input.GetKeyDown(KeyCode.Space)) velocityY = jumpScale;
            velocity.y = velocityY;
            cc.Move(velocity * Time.deltaTime);

            camRotateX -= mouseInput.y * dataContainer.settingData.mouseSensivity.y;
            camRotateX = Mathf.Clamp(camRotateX, -camRotateXClamp, camRotateXClamp);
            cam.transform.localEulerAngles = new Vector3(camRotateX, 0, 0);
            transform.Rotate(0, mouseInput.x * dataContainer.settingData.mouseSensivity.x, 0);

            armModel.SetFloat(Animator_MoveXHash, input.x);
            armModel.SetFloat(Animator_MoveYHash, input.y);
            armModel.SetBool(Animator_IsRunHash, input != Vector2.zero);
            armModel.SetBool(Animator_IsGroundHash, isGround);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position + groundCastOffset, groundCastRadius);
        }
    }
}