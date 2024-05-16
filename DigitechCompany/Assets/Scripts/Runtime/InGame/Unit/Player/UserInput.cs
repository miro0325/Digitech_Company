using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;

public enum InteractID
{
    None,
    ID1,
    ID2,
    ID3,
    ID4,
    End
}

public class UserInput : MonoBehaviourPun, IPunObservable
{
    //field
    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction scanAction;
    private InputAction jumpAction;
    private InputAction runAction;
    private InputAction crouchAction;
    private InputAction discardAction;
    private InputAction[] interactActions = new InputAction[(int)InteractID.End];

    private Vector2 moveInput;
    private Vector2 mouseInput;
    private float mouseWheel;
    private bool scanInput;
    private bool isGround;
    private bool runInput;
    private bool jumpInput;
    private bool crouchInput;
    private bool[] interactInputPressed = new bool[(int)InteractID.End];
    private bool[] interactInputReleased = new bool[(int)InteractID.End];
    private bool discardInput;

    //property
    public Vector2 MoveInput => moveInput;
    public Vector2 MouseInput => mouseInput;
    public float MouseWheel => mouseWheel;
    public bool ScanInput => scanInput;
    public bool RunInput => runInput;
    public bool JumpInput => jumpInput;
    public bool CrouchInput => crouchInput;
    public bool[] InteractInputPressed => interactInputPressed;
    public bool[] InteractInputReleased => interactInputReleased;
    public bool DiscardInput => discardInput;

    //method
    private void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        interactInputPressed[(int)InteractID.None] = false;

        moveAction = playerInput.actions["Move"];
        scanAction = playerInput.actions["Scan"];
        jumpAction = playerInput.actions["Jump"];
        runAction = playerInput.actions["Run"];
        crouchAction = playerInput.actions["Crouch"];
        discardAction = playerInput.actions["Discard"];
        for(int i = 1; i < (int)InteractID.End; i++)
            interactActions[i] = playerInput.actions[$"Interact{i}"];
    }

    private void Update()
    {
        if (!photonView.IsMine) return;

        //fixed
        mouseWheel = Input.mouseScrollDelta.y;
        mouseInput = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

        //bindable
        moveInput = moveAction.ReadValue<Vector2>();
        scanInput = scanAction.WasPressedThisFrame();
        jumpInput = jumpAction.WasPressedThisFrame();
        runInput = runAction.IsPressed();
        crouchInput = crouchAction.WasPressedThisFrame();
        discardInput = discardAction.WasPressedThisFrame();
        for(int i = 1; i < (int)InteractID.End; i++)
        {
            interactInputPressed[i] = interactActions[i].WasPressedThisFrame();
            interactInputReleased[i] = interactActions[i].WasReleasedThisFrame();
        }
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
            for(int i = 1; i < (int)InteractID.End; i++)
            {
                stream.SendNext(interactInputPressed[i]);
                stream.SendNext(interactInputReleased[i]);
            }
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
            for(int i = 1; i < (int)InteractID.End; i++)
            {
                interactInputPressed[i] = (bool)stream.ReceiveNext();
                interactInputReleased[i] = (bool)stream.ReceiveNext();
            }
            discardInput = (bool)stream.ReceiveNext();
        }
    }
}