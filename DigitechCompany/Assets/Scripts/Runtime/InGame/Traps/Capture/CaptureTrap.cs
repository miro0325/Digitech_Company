using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Photon.Pun;
using UniRx;
using UnityEngine;

public class CaptureTrap : NetworkObject
{
    private enum State { Open, Capture, Close }

    [SerializeField] private float captureRadius;
    [SerializeField] private Transform leftPart;
    [SerializeField] private Transform rightPart;
    [SerializeField] private GameObject triggerParent;
    [SerializeField] private GameObject particle;

    private float reopenTime;
    private float capturePlayerDamageTime;
    private State state;
    private ReactiveProperty<int> capturedPlayerViewId = new();

    private InGamePlayer CapturedPlayer;
    public int CapturedPlayerViewId => capturedPlayerViewId.Value;

    private void Start()
    {
        state = State.Open;

        capturedPlayerViewId
            .Subscribe(id =>
            {
                if (id == 0) CapturedPlayer = null;
                else CapturedPlayer = PhotonView.Find(id).GetComponent<InGamePlayer>();
            });

        this
            .ObserveEveryValueChanged(t => t.state)
            .Subscribe(state =>
            {
                Debug.LogError(state);
                if (state == State.Open)
                {
                    leftPart.DORotate(new Vector3(0, 0, 0), 0.25f).SetEase(Ease.OutQuad);
                    rightPart.DORotate(new Vector3(0, 0, 0), 0.25f).SetEase(Ease.OutQuad);
                }
                else
                {
                    leftPart.DORotate(new Vector3(65, 0, 0), 0.1f).SetEase(Ease.InBack);
                    rightPart.DORotate(new Vector3(-65, 0, 0), 0.1f).SetEase(Ease.InBack);
                }

                triggerParent.SetActive(state == State.Capture);
                particle.SetActive(state == State.Capture);
            });
    }

    private void Update()
    {
        switch (state)
        {
            case State.Open:
                if (Physics.SphereCast(transform.position, captureRadius, Vector3.up, out var hit, captureRadius, LayerMask.GetMask("Player")))
                {
                    Debug.Log("Detect");
                    if (hit.collider.TryGetComponent<InGamePlayer>(out var comp))
                    {
                        capturedPlayerViewId.Value = comp.photonView.ViewID;
                        capturePlayerDamageTime = 1;
                        state = State.Capture;

                        if(photonView.IsMine) CapturedPlayer.Damage(20,null);
                        Debug.Log("Captured!");
                    }
                }
                break;
            case State.Capture:
                CapturedPlayer.transform.position = transform.position;

                if (photonView.IsMine)
                {
                    if (capturePlayerDamageTime > 0)
                    {
                        capturePlayerDamageTime -= Time.deltaTime;
                    }
                    else
                    {
                        CapturedPlayer.Damage(1,null);
                        capturePlayerDamageTime = 1;
                    }
                }
                break;
            case State.Close:
                if (reopenTime > 0)
                {
                    reopenTime -= Time.deltaTime;
                }
                else
                {
                    state = State.Open;
                }
                break;
        }
    }

    public void TryExit()
    {
        if (Random.Range(0f, 100f) > 50f) photonView.RPC(nameof(ExitRpc), RpcTarget.All);
    }

    [PunRPC]
    private void ExitRpc()
    {
        capturedPlayerViewId.Value = 0;
        state = State.Close;
        reopenTime = 7.5f;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, captureRadius);
    }

    // protected override void OnSendData(List<System.Func<object>> send)
    // {
    //     base.OnSendData(send);
    //     send.Add(() => (int)state);
    //     send.Add(() => capturedPlayerViewId.Value);
    // }

    // protected override void OnReceiveData(List<System.Action<object>> receive)
    // {
    //     base.OnReceiveData(receive);
    //     receive.Add(obj => state = (State)(int)obj);
    //     receive.Add(obj => capturedPlayerViewId.Value = (int)obj);
    // }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext((int)state);
            stream.SendNext(capturedPlayerViewId.Value);
        }
        else
        {
            state = (State)(int)stream.ReceiveNext();
            capturedPlayerViewId.Value = (int)stream.ReceiveNext();
        }
    }
}
