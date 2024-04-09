using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Game.Service;

namespace Game.InGame
{
    public class CameraMover : MonoBehaviour
    {
        private Player player => Services.Get<Player>();
        private DataContainer dataContainer => Services.Get<DataContainer>();

        [SerializeField] private Vector3 offset;
        [SerializeField] private float rotXClamp;

        private float rotX;

        private void Start()
        {
            transform.SetParent(player.transform);
            transform.localPosition = Vector3.zero + offset;
            transform.localEulerAngles = Vector3.zero;
        }

        private void Update()
        {
            var mouseY = Input.GetAxis("Mouse Y");
            rotX -= mouseY * Time.deltaTime * dataContainer.settingData.mouseSensivity.y;
            rotX = Mathf.Clamp(rotX, -rotXClamp, rotXClamp);
            transform.localEulerAngles = new Vector3(rotX, 0, 0);
        }
    }
}
