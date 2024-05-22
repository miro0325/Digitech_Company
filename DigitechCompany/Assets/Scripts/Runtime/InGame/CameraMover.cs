using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class CameraMover : MonoBehaviour
{
    //service
    private Player player;
    private Player Player
    {
        get
        {
            if(ReferenceEquals(player, null))
                player = ServiceLocator.For(this).Get<Player>();
            return player;
        }
    }

    private DataContainer dataContainer;
    private DataContainer DataContainer
    {
        get
        {
            if(ReferenceEquals(dataContainer, null))
                dataContainer = ServiceLocator.ForGlobal().Get<DataContainer>();
            return dataContainer;
        }
    }

    //inspector
    [SerializeField] private Vector3 offset;
    [SerializeField] private float rotXClamp;

    private float rotX;

    private void Start()
    {
        // transform.SetParent(player.transform);
        transform.localPosition = Vector3.zero + offset;
        transform.localEulerAngles = Vector3.zero;
    }

    private void Update()
    {
        var mouseY = Input.GetAxis("Mouse Y");
        rotX -= mouseY * Time.deltaTime * DataContainer.userData.mouseSensivity.y;
        rotX = Mathf.Clamp(rotX, -rotXClamp, rotXClamp);
        transform.localEulerAngles = new Vector3(rotX, Player.transform.eulerAngles.y, 0);
    }

    private void LateUpdate()
    {
        transform.position = Player.transform.position + Vector3.up * 1.5f;
    }
}