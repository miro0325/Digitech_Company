using Photon.Pun;
using UnityEngine;
using Game.Service;

namespace Game.InGame
{
    public class Player : MonoBehaviourPun
    {
        private readonly static int Animator_MoveXHash = Animator.StringToHash("MoveX");
        private readonly static int Animator_MoveYHash = Animator.StringToHash("MoveY");
        private readonly static int Animator_IsRunHash = Animator.StringToHash("IsRun");

        private DataContainer dataContainer;

        [SerializeField] private float moveSpeed;

        private new Rigidbody rigidbody;
        private Animator animator;

        private void Awake()
        {
            rigidbody = GetComponent<Rigidbody>();
            animator = GetComponent<Animator>();

            if(!photonView.IsMine) return;
            rigidbody.interpolation = RigidbodyInterpolation.Interpolate;

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
            if(!photonView.IsMine) return;

            var mouseX = Input.GetAxis("Mouse X");
            transform.Rotate(0, mouseX * Time.deltaTime * dataContainer.settingData.mouseSensivity.x, 0);
            
            var h = Input.GetAxisRaw("Horizontal");
            var v = Input.GetAxisRaw("Vertical");
            var dir = transform.TransformDirection(new Vector3(h, 0, v)) * moveSpeed;
            if(h != 0 || v != 0) rigidbody.velocity = new Vector3(dir.x, rigidbody.velocity.y, dir.z);
            animator.SetBool(Animator_IsRunHash, h != 0 || v != 0);
            animator.SetFloat(Animator_MoveXHash, h);
            animator.SetFloat(Animator_MoveYHash, v);
        }
    }
}