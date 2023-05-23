using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterMovement1 : MonoBehaviour
{
    // Input fields
    private InputAction move;

    // Movement fields
    private Rigidbody rb;
    [SerializeField] private float movementForce = 1f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float maxSpeed = 5f;
    private Vector3 forceDirection = Vector3.zero;

    // Animator field
    public Animator animator;

    private void Awake()
    {
        rb = this.GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        move = new InputAction("Move", InputActionType.Value, "<Gamepad>/leftStick");
        move.Enable();

        animator.SetBool("isRunning", true);
    }

    private void OnDisable()
    {
        move.Disable();

        animator.SetBool("isRunning", false);
    }

    private void FixedUpdate()
    {
        // Get the direction of movement from the input system
        Vector2 moveDirection = move.ReadValue<Vector2>();

        // Set the force direction based on the movement direction and the camera orientation
        forceDirection = moveDirection.x * Camera.main.transform.right +
                         moveDirection.y * Camera.main.transform.forward;
        forceDirection.y = 0; // Keep movement on the ground plane
        forceDirection.Normalize(); // Normalize to avoid faster diagonal movement

        // Add the movement force to the rigidbody
        rb.AddForce(forceDirection * movementForce, ForceMode.VelocityChange);

        // Limit the maximum speed of the character
        Vector3 horizontalVelocity = rb.velocity;
        horizontalVelocity.y = 0;
        if (horizontalVelocity.magnitude > maxSpeed)
        {
            horizontalVelocity = horizontalVelocity.normalized * maxSpeed;
            rb.velocity = new Vector3(horizontalVelocity.x, rb.velocity.y, horizontalVelocity.z);
        }

    }

    private void DoJump(InputAction.CallbackContext obj)
    {
        if (IsGrounded())
        {
            forceDirection += Vector3.up * jumpForce;
        }
    }

    private bool IsGrounded()
    {
        Ray ray = new Ray(this.transform.position = Vector3.up * 0.25f, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit, 0.3f))
            return true;
        else
            return false;
    }
}