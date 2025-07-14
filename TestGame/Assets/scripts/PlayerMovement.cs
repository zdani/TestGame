using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovementScript : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 7f;

    public Animator animator;
    public InputAction walkAction; // InputActions let us specify keybinds in the Unity Editor and makes the code more readable
    public InputAction jumpAction;

    private Rigidbody2D body;
    private LayerMask groundLayerMask; // This will be used to check whether the player is on the ground

    void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        body.freezeRotation = true; // Prevent tipping over
        walkAction.Enable(); // InputActions need to be enabled to work
        jumpAction.Enable();
        groundLayerMask = LayerMask.GetMask("Ground"); // Ground objects in the scene need to have their layer set to "Ground"
    }

    // Physics calculations should be done in FixedUpdate instead of Update
    void FixedUpdate()
    {
        Vector2 velocity = body.linearVelocity;

        // Jumping
        if (jumpAction.IsPressed() && PlayerIsGrounded()) // Checking whether the y velocity is close to 0 allows the player to sometimes jump again at the top of their jump. It's better to check whether the player is grounded using a raycast or collision detection (see PlayerIsGrounded() method below).
        {
            body.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse); // Adding an upward force lets Unity handle gravity and is simpler than manually setting the velocity via code every frame (as it was before)
        }

        // Horizontal movement
        Vector2 moveInput = walkAction.ReadValue<Vector2>(); // walkAction is a Vector2 so it'll read joystick inputs correctly

        if (moveInput.x > 0) // moveInput.x will be positive when moving right
        {
            // Flip scale to face right
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y);
        }
        else if (moveInput.x < 0) // moveInput.x will be negative when moving left
        {
            // Flip scale to face left
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y);
        }

        velocity.x = moveInput.x * moveSpeed; // Scale the input value by moveSpeed
        body.linearVelocity = new Vector2(velocity.x, body.linearVelocity.y); // Set the horizontal velocity without changing the vertical velocity

        animator.SetFloat("speed", Mathf.Abs(velocity.x)); // The intention would be clearer as a boolean isMoving, but this can stay for now
    }
    
    bool PlayerIsGrounded()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 0f, groundLayerMask);
        return hit.collider != null;
    }
}
