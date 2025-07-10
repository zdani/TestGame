using System;
using UnityEngine;

public class PlayerMovementScript : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 7f;
    private Rigidbody2D body;
    public Animator animator;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        body.freezeRotation = true; // Prevent tipping over
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 velocity = body.linearVelocity;

        // Jumping
        if (Input.GetKeyDown(KeyCode.Space) && Mathf.Abs(body.linearVelocity.y) < 0.01f)
        {
            velocity.y = jumpForce; // Jump force
        }
        else
        {
            // Only apply gravity if not jumping
            velocity.y += Physics2D.gravity.y * Time.deltaTime;
        }

        // Horizontal movement
        float moveInput = 0f;
        if (Input.GetKey(KeyCode.RightArrow))
        {
            // Flip scale to face right
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y);
            moveInput = moveSpeed;
            
        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            // Flip scale to face left
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y);
            moveInput = -moveSpeed;
        }
        
        velocity.x = moveInput;

        animator.SetFloat("speed", Mathf.Abs(moveInput));

        body.linearVelocity = velocity;
    }
}
