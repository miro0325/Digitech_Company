using Photon.Pun;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.Rendering;
using UniRx;

public class InGamePlayerAnimator : MonoBehaviourPun, IPunObservable
{
    //const
    private readonly static int Animator_MoveStateHash = Animator.StringToHash("MoveState"); //0 : idle, 1 : move, 2 : run
    private readonly static int Animator_MoveXHash = Animator.StringToHash("MoveX");
    private readonly static int Animator_MoveYHash = Animator.StringToHash("MoveY");
    private readonly static int Animator_IsGroundHash = Animator.StringToHash("IsGround");
    private readonly static int Animator_JumpHash = Animator.StringToHash("Jump");

    [Header("Animator")]
    [SerializeField] private Animator camAnimator;
    [SerializeField] private Animator playerModelAnimator;
    [SerializeField] private Animator armModelAnimator;
    [Header("IK")]
    [SerializeField] private TwoBoneIKConstraint leftHandIK;
    [SerializeField] private TwoBoneIKConstraint rightHandIK;
    [SerializeField] private TwoBoneIKConstraint leftHandIKCam;
    [SerializeField] private TwoBoneIKConstraint rightHandIKCam;
    [SerializeField] private Transform leftHandIKTarget;
    [SerializeField] private Transform rightHandIKTarget;

    private InGamePlayer player;
    private bool isGround;
    private bool isRun;
    private bool isJump;
    private Vector2 moveInput;

    public void Initialize(InGamePlayer player)
    {
        player
            .CurrentHandItemViewID
            .Subscribe(x =>
            {
                if (x == 0)
                {
                    leftHandIKTarget.SetParent(transform);
                    leftHandIKTarget.SetLocalPositionAndRotation(Vector3.zero, Quaternion.Euler(0, 0, 0));
                    rightHandIKTarget.SetParent(transform);
                    rightHandIKTarget.SetLocalPositionAndRotation(Vector3.zero, Quaternion.Euler(0, 0, 0));
                    if (photonView.IsMine)
                    {
                        leftHandIKCam.weight = 0;
                        rightHandIKCam.weight = 0;
                    }
                    else
                    {
                        leftHandIK.weight = 0;
                        rightHandIK.weight = 0;
                    }
                    return;
                }

                var curItem = PhotonView.Find(x).GetComponent<ItemBase>();

                leftHandIKTarget.SetParent(curItem.LeftHandPoint != null ? curItem.LeftHandPoint : transform);
                leftHandIKTarget.SetLocalPositionAndRotation(Vector3.zero, Quaternion.Euler(0, 0, 0));
                rightHandIKTarget.SetParent(curItem.RightHandPoint != null ? curItem.RightHandPoint : transform);
                rightHandIKTarget.SetLocalPositionAndRotation(Vector3.zero, Quaternion.Euler(0, 0, 0));
                if (photonView.IsMine)
                {
                    leftHandIKCam.weight = curItem.LeftHandPoint != null ? 1 : 0;
                    rightHandIKCam.weight = curItem.RightHandPoint != null ? 1 : 0;
                }
                else
                {
                    leftHandIK.weight = curItem.LeftHandPoint != null ? 1 : 0;
                    rightHandIK.weight = curItem.RightHandPoint != null ? 1 : 0;
                }
            });

        if (!photonView.IsMine) return;

        this.player = player;
        playerModelAnimator.GetComponentsInChildren<SkinnedMeshRenderer>().For((i, ele) => ele.shadowCastingMode = ShadowCastingMode.ShadowsOnly);
    }

    private void DoSetting()
    {
        if (!photonView.IsMine) return;
        isGround = player.IsGround;
        isRun = player.IsRun;
        isJump = player.IsJump;
        moveInput = player.MoveInput;
    }

    private void DoAnimation()
    {
        playerModelAnimator.SetFloat(Animator_MoveXHash, moveInput.x);
        playerModelAnimator.SetFloat(Animator_MoveYHash, moveInput.y);
        playerModelAnimator.SetBool(Animator_IsGroundHash, isGround);

        if (moveInput == Vector2.zero) playerModelAnimator.SetInteger(Animator_MoveStateHash, 0);
        else playerModelAnimator.SetInteger(Animator_MoveStateHash, isRun ? 2 : 1); // move : 1, run : 2

        if (!photonView.IsMine) return;

        //arm animator
        armModelAnimator.SetFloat(Animator_MoveXHash, moveInput.x);
        armModelAnimator.SetFloat(Animator_MoveYHash, moveInput.y);
        armModelAnimator.SetBool(Animator_IsGroundHash, isGround);

        if (moveInput == Vector2.zero) armModelAnimator.SetInteger(Animator_MoveStateHash, 0);
        else armModelAnimator.SetInteger(Animator_MoveStateHash, isRun ? 2 : 1); // move : 1, run : 2

        //camera animator
        if (moveInput == Vector2.zero) camAnimator.SetInteger(Animator_MoveStateHash, 0);
        else camAnimator.SetInteger(Animator_MoveStateHash, isRun ? 2 : 1); // move : 1, run : 2

        if (isJump)
            camAnimator.SetTrigger(Animator_JumpHash);
        camAnimator.SetBool(Animator_IsGroundHash, isGround);
    }

    private void Update()
    {
        DoSetting();
        DoAnimation();
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(isGround);
            stream.SendNext(isRun);
            stream.SendNext(isJump);
            stream.SendNext(moveInput);
        }
        else
        {
            isGround = (bool)stream.ReceiveNext();
            isRun = (bool)stream.ReceiveNext();
            isJump = (bool)stream.ReceiveNext();
            moveInput = (Vector2)stream.ReceiveNext();
        }
    }
}