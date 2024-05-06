using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    [SerializeField] private Vector3 groundCastOffset;
    [SerializeField] private float groundCastRadius;
    [SerializeField] private LayerMask groundCastMask;

    private Vector2 moveInput;
    private Vector2 mouseInput;
    private bool isGround;
    private bool runInput;
    private bool interactInput;
    private bool jumpInput;
    private bool crouchInput;

    public Vector2 MoveInput => moveInput;
    public Vector2 MouseInput => mouseInput;
    public bool RunInput => runInput;
    public bool IsGround => isGround;
    public bool InteractInput => interactInput;
    public bool JumpInput => jumpInput;
    public bool CrouchInput => crouchInput;

    private void Update()
    {
        moveInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        mouseInput = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        isGround = Physics.CheckSphere(transform.position + groundCastOffset, groundCastRadius, groundCastMask);
        jumpInput = Input.GetKeyDown(KeyCode.Space);
        interactInput = Input.GetKeyDown(KeyCode.E);
        runInput = Input.GetKey(KeyCode.LeftShift);
        crouchInput = Input.GetKeyDown(KeyCode.LeftControl);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position + groundCastOffset, groundCastRadius);
    }
}