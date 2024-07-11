using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Photon.Pun;
using UniRx;
using Cysharp.Threading.Tasks;


public class Basement : MonoBehaviourPun, IService, IPunObservable
{
    public enum State
    {
        Up,
        TakingOff,
        Down,
        Landing,
    }

    private GameManager _gameManager;
    private GameManager gameManager => _gameManager ??= ServiceLocator.For(this).Get<GameManager>();

    private static int Animator_DownHash = Animator.StringToHash("Down");
    private static int Animator_UpHash = Animator.StringToHash("Up");

    public bool IsOpenDoor => isOpenDoor;
    public bool IsMovingDoor => isMovingDoor;
    public bool IsMoving => isMoving;
    public bool IsArrive => isArrive;

    [SerializeField] Transform doorOpenTrans;
    [SerializeField] Transform backDoor;
    [SerializeField] float openDelay;
    private Vector3 prevPos, prevRot;
    private bool isOpenDoor = true;
    private bool isMovingDoor = false;

    [SerializeField] Transform[] tires;
    [SerializeField] float tireRotDelay;
    [SerializeField] float tireRotAngle;
    [SerializeField] Ease ease;
    [SerializeField] private Transform camParent;

    private bool isMoving = false;
    private bool isArrive = true;
    private Sequence doorMoveSequence;

    private Animator animator;
    private Vector3 position;
    private State state;
    private Dictionary<int, ItemBase> wholeItems = new();
    private Dictionary<int, ItemBase> curGameItems = new();
    private Camera cam;

    public State CurState => state;
    public Dictionary<int, ItemBase> WholeItems => wholeItems;
    public Dictionary<int, ItemBase> CurGameItems => curGameItems;
    
    private void Awake()
    {
        Debug.Log("test");
        ServiceLocator.For(this).Register(this);
    }

    void Start()
    {
        animator = GetComponent<Animator>();
        doorMoveSequence = DOTween.Sequence();

        gameManager
            .ObserveEveryValueChanged(g => g.HasAlivePlayer)
            .Where(b => b == false)
            .Subscribe(_ =>
            {
                Debug.LogError("Set Camera basement");
                Camera.main.transform.SetParent(camParent);
                Camera.main.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.Euler(0, 0, 0));
            });
            
        gameManager
            .ObserveEveryValueChanged(g => g.State)
            .Where(state => state == GameState.StartWait)
            .Subscribe(_ => curGameItems.Clear());
    }

    public void MoveUp()
    {
        photonView.RPC(nameof(MoveRpc), RpcTarget.All, (int)State.Up);
    }

    public void MoveDown()
    {
        photonView.RPC(nameof(MoveRpc), RpcTarget.All, (int)State.Down);
    }

    [PunRPC]
    private void MoveRpc(int state)
    {
        var eState = (State)state;
        if (eState == State.Landing) return;
        if (eState == State.TakingOff) return;

        MoveRoutine(eState).Forget();
    }

    private async UniTask MoveRoutine(State state)
    {
        this.state = state == State.Up ? State.TakingOff : State.Landing;

        await UniTask.WaitForSeconds(2f);
        animator.Play(state == State.Up ? Animator_UpHash : Animator_DownHash);
        await UniTask.WaitForSeconds(7f);
        this.state = state;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.transform.parent == null)
        {
            other.transform.SetParent(transform);

            if (other.TryGetComponent<ItemBase>(out var comp))
            {
                wholeItems.TryAdd(comp.photonView.ViewID, comp);
                curGameItems.TryAdd(comp.photonView.ViewID, comp);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (ReferenceEquals(other.transform.parent, transform))
            other.transform.SetParent(null);
    }

    private void Update()
    {
        position = transform.position;
        transform.position = Vector3.Lerp(transform.position, position, 8 * Time.deltaTime);
    }

    //문 조작 함수
    public void InteractDoor()
    {
        photonView.RPC(nameof(InteractDoorRPC), RpcTarget.All);
    }

    [PunRPC]
    private void InteractDoorRPC()
    {
        doorMoveSequence.Kill();
        isOpenDoor = !isOpenDoor;
        if (isOpenDoor)
            CloseDoor();
        else
            OpenDoor();
    }

    //기지 문 열기 함수
    private void OpenDoor()
    {
        isMovingDoor = true;
        prevPos = backDoor.localPosition;
        prevRot = backDoor.localEulerAngles;

        doorMoveSequence.Append(backDoor.DOLocalMove(doorOpenTrans.localPosition, openDelay).OnComplete(() => isMovingDoor = false));
        doorMoveSequence.Append(backDoor.DOLocalRotate(doorOpenTrans.localEulerAngles, openDelay));
    }

    //기지 문 닫기 함수
    private void CloseDoor()
    {
        isMovingDoor = true;
        doorMoveSequence.Append(backDoor.DOLocalMove(prevPos, openDelay).OnComplete(() => isMovingDoor = false));
        doorMoveSequence.Append(backDoor.DOLocalRotate(prevRot, openDelay));
    }

    public void Leave()
    {
        isMoving = true;
        isArrive = false;
        RotateTire(true).OnComplete(
            () => { animator.SetTrigger("Leave"); }
        );
    }

    public void Arrive()
    {
        isMoving = true;
        isArrive = true;
        RotateTire(true).OnComplete(
            () => { animator.SetTrigger("Arrive"); }
        );
    }

    public void ResetTire()
    {
        RotateTire(false);
        if (isArrive) transform.localPosition = new Vector3(0, 0, 0);
        else transform.localPosition = new Vector3(0, 80, 300);
        isMoving = false;
    }

    private Tween RotateTire(bool isLeave = false)
    {
        //Sequence sequence = DOTween.Sequence();
        if (isLeave)
        {
            tires[0].DOLocalRotate(new Vector3(tireRotAngle, 90, 0), tireRotDelay).SetEase(ease);
            tires[1].DOLocalRotate(new Vector3(tireRotAngle, 90, 0), tireRotDelay).SetEase(ease);
            tires[2].DOLocalRotate(new Vector3(-tireRotAngle, 90, 0), tireRotDelay).SetEase(ease);
            return tires[3].DOLocalRotate(new Vector3(-tireRotAngle, 90, 0), tireRotDelay).SetEase(ease);
        }
        else
        {
            tires[0].DOLocalRotate(new Vector3(0, 90, 0), tireRotDelay).SetEase(ease);
            tires[1].DOLocalRotate(new Vector3(0, 90, 0), tireRotDelay).SetEase(ease);
            tires[2].DOLocalRotate(new Vector3(0, 90, 0), tireRotDelay).SetEase(ease);
            return tires[3].DOLocalRotate(new Vector3(0, 90, 0), tireRotDelay).SetEase(ease);
        }
        //return sequence;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(position);
        }
        else
        {
            position = (Vector3)stream.ReceiveNext();
        }
    }
}