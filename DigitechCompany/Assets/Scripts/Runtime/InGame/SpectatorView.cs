using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class SpectatorView : MonoBehaviour, IService
{
    private InputManager userInput;
    private InputManager UserInput
    {
        get
        {
            if(ReferenceEquals(userInput, null))
                userInput = ServiceLocator.For(this).Get<InputManager>();
            return userInput;
        }
    }

    private DataContainer dataContainer;
    private DataContainer DataContainer
    {
        get
        {
            if(ReferenceEquals(dataContainer, null))
                dataContainer = ServiceLocator.For(this).Get<DataContainer>();
            return dataContainer;
        }
    }

    [SerializeField] private Camera cam;
    private InGamePlayer inGamePlayer;

    public Camera Cam => cam;

    public void SetTargetPlayer(InGamePlayer gamePlayer)
    {
        inGamePlayer = gamePlayer;
        cam.gameObject.SetActive(!ReferenceEquals(gamePlayer, null));
    }

    private void Awake()
    {
        ServiceLocator.For(this).Register(this);
    }

    private void Update()
    {
        if(cam.gameObject.activeSelf)
        {
            transform.Rotate(userInput.MouseInput.y * DataContainer.userData.mouseSensivity.y, UserInput.MouseInput.x * DataContainer.userData.mouseSensivity.x, 0);
            transform.position = inGamePlayer.transform.position + Vector3.up * 1.5f;
        }
    }
}
