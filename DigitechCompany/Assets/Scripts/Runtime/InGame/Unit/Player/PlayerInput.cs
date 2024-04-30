using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    [SerializeField] private Vector3 groundCastOffset;
    [SerializeField] private float groundCastRadius;
    [SerializeField] private LayerMask groundCastMask;

    private Vector2 moveInput;
    private Vector2 mouseInput;
    private bool isGround;

    public Vector2 MoveInput => moveInput;
    public Vector2 MouseInput => mouseInput;
    public bool IsGround => isGround;

    private void Update()
    {
        moveInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        mouseInput = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        isGround = Physics.CheckSphere(transform.position + groundCastOffset, groundCastRadius, groundCastMask);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position + groundCastOffset, groundCastRadius);
    }
}