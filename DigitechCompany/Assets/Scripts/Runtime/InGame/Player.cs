using Photon.Pun;
using UnityEngine;

namespace Game.InGame
{
    public class Player : MonoBehaviourPun
    {
        private DataContainer dataContainer => ServiceProvider.Get<DataContainer>();

        [SerializeField] private float moveSpeed;

        private new Rigidbody rigidbody;

        private void Awake()
        {
            rigidbody = GetComponent<Rigidbody>();

            if(!photonView.IsMine) return;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            ServiceProvider.Register(this);
        }

        private void Update()
        {
            if(!photonView.IsMine) return;

            var mouseX = Input.GetAxis("Mouse X");
            transform.Rotate(0, mouseX * Time.deltaTime * dataContainer.settingData.mouseSensivity.x, 0);
            
            var h = Input.GetAxis("Horizontal");
            var v = Input.GetAxis("Vertical");
            var dir = transform.TransformDirection(new Vector3(h, 0, v)) * moveSpeed;
            rigidbody.velocity = new Vector3(dir.x, rigidbody.velocity.y, dir.z);
        }
    }
}