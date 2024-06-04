using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class InGameCamera : MonoBehaviour
{
    //service
    private InGamePlayer player;
    private InGamePlayer Player
    {
        get
        {
            if(ReferenceEquals(player, null))
                player = ServiceLocator.For(this).Get<InGamePlayer>();
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

    private SpectatorView spectatorView;
    private SpectatorView SpectatorView
    {
        get
        {
            if(ReferenceEquals(spectatorView, null))
                spectatorView = ServiceLocator.For(this).Get<SpectatorView>();
            return spectatorView;
        }
    }

    //inspector
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Vector3 offset;
    [SerializeField] private float rotXClamp;

    private float rotX;

    private void Start()
    {
        // transform.SetParent(player.transform);
        transform.localPosition = Vector3.zero + offset;
        transform.localEulerAngles = Vector3.zero;

        mainCamera.GetUniversalAdditionalCameraData().cameraStack.Add(SpectatorView.Cam);
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