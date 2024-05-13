using UnityEngine;

public enum InteractID
{
    ID1, //Ex E
    ID2, //Ex Click
    ID3, //Ex R
    End
}

public class PlayerInput : MonoBehaviour
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

    private void Update()
    {
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
}