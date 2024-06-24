using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Photon.Pun;
using UniRx;
using UnityEngine;

public class CaptureTrap : MonoBehaviourPun, IPunObservable
{
    private enum State { Open, Capture, Close }

    [SerializeField] private float captureRadius;
    [SerializeField] private Transform leftPart;
    [SerializeField] private Transform rightPart;
    [SerializeField] private GameObject triggerParent;

    private float reopenTime;
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
                if(id == 0) CapturedPlayer = null;
                else CapturedPlayer = PhotonView.Find(id).GetComponent<InGamePlayer>();
            });

        this
            .ObserveEveryValueChanged(t => t.state)
            .Subscribe(state =>
            {
                if (state == State.Open)
                {
                    leftPart.DORotate(new Vector3(0, 0, 0), 0.25f).SetEase(Ease.InBack);
                    rightPart.DORotate(new Vector3(0, 0, 0), 0.25f).SetEase(Ease.InBack);
                }
                else
                {
                    leftPart.DORotate(new Vector3(65, 0, 0), 0.25f).SetEase(Ease.InBack);
                    rightPart.DORotate(new Vector3(-65, 0, 0), 0.25f).SetEase(Ease.InBack);
                }

                triggerParent.SetActive(state == State.Capture);
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
                        state = State.Capture;
                        Debug.Log("Captured!");
                    }
                }
                break;
            case State.Capture:
                CapturedPlayer.SetPosition(transform.position);
                break;
            case State.Close:
                if (reopenTime > 0)
                {
                    reopenTime -= Time.deltaTime;
                    Debug.Log(reopenTime);
                }
                else state = State.Open;
                break;
        }
    }

    public void TryExit()
    {
        if (Random.Range(0f, 100f) > 50f)
        {
            capturedPlayerViewId.Value = 0;
            state = State.Close;
            reopenTime = 7.5f;
        }
        // photonView.RPC(nameof(TryExitRpc), RpcTarget.MasterClient);
    }

    // [PunRPC]
    // private void TryExitRpc()
    // {
    //     if (Random.Range(0f, 100f) > 50f)
    //     {
    //         capturedPlayerViewId = null;
    //         state = State.Close;
    //         reopenTime = 7.5f;
    //     }
    // }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, captureRadius);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if(stream.IsWriting)
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
