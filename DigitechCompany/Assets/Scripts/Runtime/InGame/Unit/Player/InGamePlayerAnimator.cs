using Photon.Pun;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.Rendering;
using UniRx;
using System.Collections.Generic;

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
    [SerializeField] private InGamePlayerIKHandler bodyHandIK;
    [SerializeField] private InGamePlayerIKHandler camHandIK;

    private InGamePlayer player;
    private bool isGround;
    private bool isRun;
    private bool isJump;
    private Vector2 moveInput;
    private SkinnedMeshRenderer[] playerModelRenderers;

    public void Initialize(InGamePlayer player)
    {
        this.player = player;

        player
            .CurrentHandItemViewID
            .Subscribe(x =>
            {
                if (x == 0)
                {
                    if (photonView.IsMine) camHandIK.SetHandIKTarget(null, null);
                    else bodyHandIK.SetHandIKTarget(null, null);
                    return;
                }

                var curItem = PhotonView.Find(x).GetComponent<ItemBase>();

                if (photonView.IsMine) camHandIK.SetHandIKTarget(curItem.LeftHandPoint, curItem.RightHandPoint);
                else bodyHandIK.SetHandIKTarget(curItem.LeftHandPoint, curItem.RightHandPoint);
            });

        if (!photonView.IsMine) return;
        playerModelRenderers = playerModelAnimator.GetComponentsInChildren<SkinnedMeshRenderer>();
    }

    public void SetActivePlayerModel(bool active)
    {
        playerModelRenderers.For((i, ele) => ele.shadowCastingMode = active ? ShadowCastingMode.On : ShadowCastingMode.ShadowsOnly);
    }

    public void SetActiveArmModel(bool active)
    {
        camAnimator.gameObject.SetActive(active);
    }

    public Transform GetHeadTransform()
    {
        return playerModelAnimator.GetBoneTransform(HumanBodyBones.Head);
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
        if(player.IsDie)
        {
            playerModelAnimator.enabled = false;
            return;
        }

        playerModelAnimator.enabled = true;

        playerModelAnimator.SetFloat(Animator_MoveXHash, moveInput.x);
        playerModelAnimator.SetFloat(Animator_MoveYHash, moveInput.y);
        playerModelAnimator.SetBool(Animator_IsGroundHash, isGround);

        if (moveInput == Vector2.zero) playerModelAnimator.SetInteger(Animator_MoveStateHash, 0);
        else playerModelAnimator.SetInteger(Animator_MoveStateHash, isRun ? 2 : 1); // move : 1, run : 2

        if (!photonView.IsMine) return;

        //arm animator
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