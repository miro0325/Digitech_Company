using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UniRx;
using UnityEngine;

public class SpectatorView : MonoBehaviour, IService
{
    private DataContainer _dataContainer;
    private DataContainer dataContainer => _dataContainer ??= ServiceLocator.ForGlobal().Get<DataContainer>();
    private GameManager _gameManager;
    private GameManager gameManager => _gameManager ??= ServiceLocator.For(this).Get<GameManager>();
    private InGamePlayer _player;
    private InGamePlayer player => _player ??= ServiceLocator.For(this).Get<InGamePlayer>();
    private UserInput input => UserInput.input;

    [SerializeField] private Transform camHolder;
    [SerializeField] private float camXRotateClampAbs;
    [SerializeField] private float camDistanceClamp;
    [SerializeField] private float camCollisionRadius;
    [SerializeField] private LayerMask ignoreCamCollisionLayer;

    private int targetIndex;
    private float remainTime;
    private float camXRotate;
    private float camDistance;
    private float curMaxDistance;
    private List<InGamePlayer> alivePlayers = new();
    private Camera cam;

    public void UpdateAlivePlayerList(List<InGamePlayer> players)
    {
        alivePlayers = players;
        if (targetIndex >= players.Count)
            targetIndex--;
    }

    private void Awake()
    {
        ServiceLocator.For(this).Register(this);
    }

    private void Start()
    {
        cam = Camera.main;

        ServiceLocator
            .For(this)
            .Get<GameManager>()
            .OnLoadComplete += () =>
            {
                player
                    .ObserveEveryValueChanged(p => p.IsDie)
                    .Subscribe(isDie =>
                    {
                        //initialize
                        if (isDie)
                        {
                            Debug.LogError("Set camera to spectate");
                            targetIndex = 0;
                            input.Spectator.Enable();
                            cam.transform.SetParent(camHolder);
                            cam.transform.SetLocalPositionAndRotation(new Vector3(0, 0, -camDistanceClamp), Quaternion.Euler(0, 0, 0));
                            camDistance = camDistanceClamp;
                            remainTime = 1.5f;
                        }
                        else
                        {
                            Debug.LogError("Set camera to play");
                            targetIndex = -1;
                            input.Spectator.Disable();
                        }
                    });
            };
    }

    private void Update()
    {
        if (ReferenceEquals(player, null)) return;

        if (!player.IsDie) return;
        if (targetIndex == -1) return;
        if (alivePlayers.Count == 0) return;

        if (remainTime > 0)
        {
            remainTime -= Time.deltaTime;
            transform.position = player.Head.position;
            return;
        }
        else
        {
            if (input.Spectator.Change.WasPressedThisFrame())
            {
                targetIndex += (int)input.Spectator.Change.ReadValue<float>();

                if (targetIndex < 0) targetIndex = alivePlayers.Count - 1;
                if (targetIndex > alivePlayers.Count - 1) targetIndex = 0;
            }

            transform.position = alivePlayers[targetIndex].transform.position + Vector3.up * 1.5f;
        }

        //rotation
        var mouseInput = input.Spectator.Mouse.ReadValue<Vector2>();

        transform.Rotate(0, mouseInput.x * dataContainer.userData.mouseSensivity.x, 0, Space.Self);
        camXRotate -= mouseInput.y * dataContainer.userData.mouseSensivity.y;
        camXRotate = Mathf.Clamp(camXRotate, -camXRotateClampAbs, camXRotateClampAbs);
        camHolder.localRotation = Quaternion.Euler(camXRotate, 0, 0);

        //cam collision
        curMaxDistance += input.Spectator.MouseWheel.ReadValue<float>() * 0.01f;
        curMaxDistance = Mathf.Clamp(curMaxDistance, 0, camDistanceClamp);

        if (Physics.Linecast(transform.position, cam.transform.position - cam.transform.forward * camCollisionRadius, out var hit, ~ignoreCamCollisionLayer)) camDistance = hit.distance;
        else camDistance = camDistanceClamp;
        camDistance = Mathf.Clamp(camDistance, 0, camDistanceClamp);
        camDistance = Mathf.Min(curMaxDistance, camDistance);
        cam.transform.localPosition = Vector3.back * camDistance;
    }
}
