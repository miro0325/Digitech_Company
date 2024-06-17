using System.Collections;
using System.Collections.Generic;
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

    [SerializeField] private Transform camHolder;
    [SerializeField] private float camXRotateClampAbs;
    [SerializeField] private float camDistanceClamp;
    [SerializeField] private float camCollisionRadius;
    [SerializeField] private LayerMask ignoreCamCollisionLayer;

    private int targetIndex;
    private float camXRotate;
    private float camDistance;
    private List<InGamePlayer> aliveInGamePlayers = new();
    private InGameInputAction inGameInput;
    private Camera cam;

    private void Start()
    {
        cam = Camera.main;
        inGameInput = new();
        gameManager
            .ObserveEveryValueChanged(gm => gm.AlivePlayers.Count)
            .Subscribe(_ =>
            {
                aliveInGamePlayers.Clear();

                foreach(var p in gameManager.AlivePlayers)
                    aliveInGamePlayers.Add(PhotonView.Find(p).GetComponent<InGamePlayer>());
                
                if(targetIndex >= aliveInGamePlayers.Count)
                    targetIndex = aliveInGamePlayers.Count - 1;
            });

        player
            .ObserveEveryValueChanged(p => p.IsDie)
            .Subscribe(isDie =>
            {
                targetIndex = isDie ? 0 : -1;

                //initialize
                if(isDie)
                {
                    inGameInput.Spectator.Enable();
                                            
                    cam.transform.SetParent(camHolder);
                    cam.transform.SetLocalPositionAndRotation(new Vector3(0, 0, -camDistanceClamp), Quaternion.Euler(0, 0, 0));
                    camDistance = camDistanceClamp;
                }
            });
    }

    private void Update()
    {
        Debug.Log(inGameInput.asset == player.testasset);        

        if(targetIndex == -1) return;
        if(aliveInGamePlayers.Count == 0) return;

        //set spectator
        if(inGameInput.Spectator.Change.WasPressedThisFrame())
        {
            targetIndex += (int)inGameInput.Spectator.Change.ReadValue<float>();

            if(targetIndex < 0) targetIndex = aliveInGamePlayers.Count - 1;
            if(targetIndex > aliveInGamePlayers.Count - 1) targetIndex = 0;
        }

        //rotation
        var mouseInput = inGameInput.Spectator.Mouse.ReadValue<Vector2>();

        transform.Rotate(0, mouseInput.x * dataContainer.userData.mouseSensivity.x, 0, Space.Self);
        camXRotate -= mouseInput.y * dataContainer.userData.mouseSensivity.y;
        camXRotate = Mathf.Clamp(camXRotate, -camXRotateClampAbs, camXRotateClampAbs);
        camHolder.localRotation = Quaternion.Euler(camXRotate, 0, 0);

        transform.position = aliveInGamePlayers[targetIndex].transform.position + Vector3.up * 1.5f;

        //cam collision
        if(Physics.Linecast(transform.position, cam.transform.position - cam.transform.forward * camCollisionRadius, out var hit, ~ignoreCamCollisionLayer)) camDistance = hit.distance;
        else camDistance = camDistanceClamp;
        camDistance = Mathf.Clamp(camDistance, 0, camDistanceClamp);
        cam.transform.localPosition = Vector3.back * camDistance;
    }
}
