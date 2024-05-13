using Photon.Pun;
using UnityEngine;

public enum InteractID
{
    None,
    ID1,
    ID2,
    ID3,
    End
}

public class PlayerInput : MonoBehaviourPun, IPunObservable
{
    [SerializeField] private Vector3 groundCastOffset;
    [SerializeField] private float groundCastRadius;
    [SerializeField] private LayerMask groundCastMask;

    private Vector2 moveInput;
    private Vector2 mouseInput;
    private float mouseWheel;
    private bool scanInput;
    private bool isGround;
    private bool runInput;
    private bool jumpInput;
    private bool crouchInput;
    private bool[] interactInputs = new bool[(int)InteractID.End];
    private bool discardInput;

    public Vector2 MoveInput => moveInput;
    public Vector2 MouseInput => mouseInput;
    public float MouseWheel => mouseWheel;
    public bool ScanInput => scanInput;
    public bool RunInput => runInput;
    public bool IsGround => isGround;
    public bool JumpInput => jumpInput;
    public bool CrouchInput => crouchInput;
    public bool[] InteractInputs => interactInputs;
    public bool DiscardInput => discardInput;

    private void Start()
    {
        interactInputs[(int)InteractID.None] = false;
    }

    private void Update()
    {
        if (!photonView.IsMine) return;
        moveInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        mouseInput = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        mouseWheel = Input.mouseScrollDelta.y;
        scanInput = Input.GetMouseButtonDown(1);
        isGround = Physics.CheckSphere(transform.position + groundCastOffset, groundCastRadius, groundCastMask);
        jumpInput = Input.GetKeyDown(KeyCode.Space);
        runInput = Input.GetKey(KeyCode.LeftShift);
        crouchInput = Input.GetKeyDown(KeyCode.LeftControl);

        interactInputs[(int)InteractID.ID1] = Input.GetKeyDown(KeyCode.E);
        interactInputs[(int)InteractID.ID2] = Input.GetMouseButtonDown(0);
        interactInputs[(int)InteractID.ID3] = Input.GetKeyDown(KeyCode.R);

        discardInput = Input.GetKeyDown(KeyCode.G);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position + groundCastOffset, groundCastRadius);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(moveInput);
            stream.SendNext(mouseInput);
            stream.SendNext(mouseWheel);
            stream.SendNext(scanInput);
            stream.SendNext(isGround);
            stream.SendNext(runInput);
            stream.SendNext(jumpInput);
            stream.SendNext(crouchInput);
            stream.SendNext(interactInputs[(int)InteractID.ID1]);
            stream.SendNext(interactInputs[(int)InteractID.ID2]);
            stream.SendNext(interactInputs[(int)InteractID.ID3]);
            stream.SendNext(discardInput);
        }
        else
        {
            moveInput = (Vector2)stream.ReceiveNext();
            mouseInput = (Vector2)stream.ReceiveNext();
            mouseWheel = (float)stream.ReceiveNext();
            scanInput = (bool)stream.ReceiveNext();
            isGround = (bool)stream.ReceiveNext();
            runInput = (bool)stream.ReceiveNext();
            jumpInput = (bool)stream.ReceiveNext();
            crouchInput = (bool)stream.ReceiveNext();
            interactInputs[(int)InteractID.ID1] = (bool)stream.ReceiveNext();
            interactInputs[(int)InteractID.ID2] = (bool)stream.ReceiveNext();
            interactInputs[(int)InteractID.ID3] = (bool)stream.ReceiveNext();
            discardInput = (bool)stream.ReceiveNext();
        }
    }
}