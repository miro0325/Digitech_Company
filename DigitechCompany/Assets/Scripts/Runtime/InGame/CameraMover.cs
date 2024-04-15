using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Game.Service;

namespace Game.InGame
{
    public class CameraMover : MonoBehaviour
    {
        private Player player;
        private DataContainer dataContainer;

        [SerializeField] private Vector3 offset;
        [SerializeField] private float rotXClamp;

        private float rotX;

        private void Start()
        {
            player = Services.Get<Player>();
            dataContainer = Services.Get<DataContainer>();

            // transform.SetParent(player.transform);
            transform.localPosition = Vector3.zero + offset;
            transform.localEulerAngles = Vector3.zero;
        }

        private void Update()
        {
            var mouseY = Input.GetAxis("Mouse Y");
            rotX -= mouseY * Time.deltaTime * dataContainer.settingData.mouseSensivity.y;
            rotX = Mathf.Clamp(rotX, -rotXClamp, rotXClamp);
            transform.localEulerAngles = new Vector3(rotX, player.transform.eulerAngles.y, 0);
        }

        private void LateUpdate()
        {
            transform.position = player.transform.position + Vector3.up * 1.5f;
        }
    }
}
