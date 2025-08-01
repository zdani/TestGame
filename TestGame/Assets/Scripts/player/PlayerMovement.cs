using System;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovementScript : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 7f;

    public Animator animator;
    public InputAction walkAction;
    public InputAction jumpAction;

    public bool IsOnGround { get; private set; }
    public bool IsWalking { get; private set; }

    private Rigidbody2D body;
    private LayerMask groundLayerMask; // This will be used to check whether the player is on the ground

    void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        body.freezeRotation = true; // Prevent tipping over
        walkAction.Enable();
        jumpAction.Enable();
        groundLayerMask = LayerMask.GetMask("Ground"); // Ground objects in the scene need to have their layer set to "Ground"
    }

    // Physics calculations should be done in FixedUpdate instead of Update
    void FixedUpdate()
    {
        IsOnGround = PlayerIsGrounded();

        Vector2 velocity = body.linearVelocity;

        // Jumping
        if (jumpAction.IsPressed() && IsOnGround)
        {
            body.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
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

        // Apply horizontal movement - different methods for ground vs air
        if (Mathf.Abs(moveInput.x) > 0.1f)
        {
            if (IsOnGround)
            {
                // On ground: use velocity for responsive movement
                float targetVelocityX = moveInput.x * moveSpeed;
                float currentVelocityX = body.linearVelocity.x;
                
                // Smoothly interpolate to target velocity
                float newVelocityX = Mathf.Lerp(currentVelocityX, targetVelocityX, 0.8f);
                body.linearVelocity = new Vector2(newVelocityX, body.linearVelocity.y);
            }
            else
            {
                // In air: use AddForce to prevent wall-sticking
                body.AddForce(new Vector2(moveInput.x * moveSpeed, 0), ForceMode2D.Force);
            }
        }
        else
        {
            // Apply friction when no input
            body.linearVelocity = new Vector2(body.linearVelocity.x * 0.8f, body.linearVelocity.y);
        }

        IsWalking = Mathf.Abs(moveInput.x) > 0.1f; // Consider the player walking if the input is above a small threshold

        if (IsWalking)
        {
            animator.SetBool("isWalking", true);
        }
        else
        {
            animator.SetBool("isWalking", false);
        }

        if (IsOnGround)
        {
            animator.SetBool("isGrounded", true);
        }
        else
        {
            animator.SetBool("isGrounded", false);
        }
    }
    
    bool PlayerIsGrounded()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 0.1f, groundLayerMask);
        return hit.collider != null;
    }
    

}
