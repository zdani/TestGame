using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float airControlMultiplier = 0.5f;
    [SerializeField] private float maxAirSpeed = 8f;
    
    [Header("Ground Detection")]
    [SerializeField] private LayerMask groundLayerMask;
    [SerializeField] private float groundCheckDistance = 0.1f; // Reduced since we're now at the exact bottom
    
    [Header("Enemy Interaction")]
    [SerializeField] private float slideForce = 20f; // Much more aggressive sliding
    [SerializeField] private float downwardForce = 15f; // Much stronger downward force
    
    // Components
    private Rigidbody2D rb;
    private Animator animator;
    private PhysicsMaterial2D noFrictionMaterial;
    
    // State
    public bool isGrounded;
    private bool isMoving;
    
    // Ground check position - use actual collider bounds
    private Vector2 GroundCheckPosition
    {
        get
        {
            Collider2D playerCollider = GetComponent<Collider2D>();
            if (playerCollider != null)
            {
                // Use the bottom of the collider bounds
                return new Vector2(transform.position.x, playerCollider.bounds.min.y);
            }
            // Fallback to transform position if no collider
            return (Vector2)transform.position + Vector2.down * 0.5f;
        }
    }
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        
        // Configure rigidbody
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.freezeRotation = true;
        //rb.gravityScale = 1f;

        // Create a physics material with zero friction to prevent sticking to walls
        noFrictionMaterial = new PhysicsMaterial2D();
        noFrictionMaterial.friction = 0f;
        rb.sharedMaterial = noFrictionMaterial;

        // Set up ground layer mask properly
        groundLayerMask = LayerMask.GetMask("Ground");
    }
    
    private void Update()
    {
        HandleJump();
    }
    
    private void FixedUpdate()
    {
        CheckGrounded();
        HandleMovement();
        UpdateAnimations();
    }
    
    private void CheckGrounded()
    {
        RaycastHit2D hit = Physics2D.Raycast(GroundCheckPosition, Vector2.down, groundCheckDistance, groundLayerMask);
        isGrounded = hit.collider != null;
    }
    
    private void HandleMovement()
    {
        // Get input
        float horizontalInput = 0f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            horizontalInput -= 1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            horizontalInput += 1f;
        
        // Clamp input to prevent diagonal movement being faster
        horizontalInput = Mathf.Clamp(horizontalInput, -1f, 1f);
        
        // Handle character flipping
        if (Mathf.Abs(horizontalInput) > 0.1f)
        {
            transform.localScale = new Vector3(Mathf.Sign(horizontalInput), 1f, 1f);
        }
        
        // Movement logic
        if (isGrounded)
        {
            // Ground movement - direct velocity control
            Vector2 velocity = rb.linearVelocity;
            velocity.x = horizontalInput * moveSpeed;
            rb.linearVelocity = velocity;
            isMoving = Mathf.Abs(horizontalInput) > 0.1f;
        }
        else
        {
            // Air movement - limited control to prevent hovering
            if (Mathf.Abs(horizontalInput) > 0.1f)
            {
                // Apply reduced force in air
                float airForce = horizontalInput * moveSpeed * airControlMultiplier;
                rb.AddForce(Vector2.right * airForce, ForceMode2D.Force);
                
                // Clamp horizontal speed in air
                if (Mathf.Abs(rb.linearVelocity.x) > maxAirSpeed)
                {
                    Vector2 clampedVelocity = rb.linearVelocity;
                    clampedVelocity.x = Mathf.Sign(rb.linearVelocity.x) * maxAirSpeed;
                    rb.linearVelocity = clampedVelocity;
                }
            }
            else
            {
                // Reduce horizontal velocity when no input
                Vector2 velocity = rb.linearVelocity;
                velocity.x *= 0.95f;
                rb.linearVelocity = velocity;
            }
            
            isMoving = Mathf.Abs(rb.linearVelocity.x) > 0.5f;
        }
    }
    
    private void HandleJump()
    {
        // Jump only when grounded and space is pressed
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
        
        // Variable jump height - release space early to jump lower
        if (Input.GetKeyUp(KeyCode.Space) && rb.linearVelocity.y > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);
        }
    }
    
    private void UpdateAnimations()
    {
        if (animator != null)
        {
            animator.SetBool("isWalking", isMoving);
            animator.SetBool("isGrounded", isGrounded);
        }
    }
    
    // Handle collisions with enemies
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.TryGetComponent<Enemy>(out var enemy))
        {
            HandleEnemyCollision(collision, enemy, isInitialContact: true);
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.TryGetComponent<Enemy>(out var enemy))
        {
            HandleEnemyCollision(collision, enemy, isInitialContact: false);
        }
    }

    private void HandleEnemyCollision(Collision2D collision, Enemy enemy, bool isInitialContact)
    {
        if (IsLandingOnEnemy(collision))
        {
            // We are on top of the enemy, so apply the slide effect.
            // This is checked on both Enter and Stay to ensure the player slides off.
            ApplySlideEffect(enemy);
        }
        else if (isInitialContact)
        {
            // It's a side collision on the first contact, so the player takes damage.
            if (TryGetComponent<Player>(out var player) && enemy.CanDealContactDamage)
            {
                player.TakeDamage(enemy.DamageAmount, enemy.EnemyName);
            }
        }
    }

    private bool IsLandingOnEnemy(Collision2D collision)
    {
        // To be "landing", the player must be moving downwards.
        if (rb.linearVelocity.y >= 0)
        {
            return false;
        }

        // Check the contact points to see if we are on top of the collider.
        foreach (var contact in collision.contacts)
        {
            // The contact normal is a vector pointing perpendicular to the collision surface.
            // If we are landing on top, the normal should point upwards (positive y).
            // We use a threshold to account for curved surfaces (like a head).
            if (contact.normal.y > 0.5f)
            {
                // This is a reliable indicator that we are on top of the enemy.
                return true;
            }
        }

        return false;
    }

    private void ApplySlideEffect(Enemy enemy)
    {
        // Determine which side of the enemy the player should be pushed towards.
        float slideDirection = (transform.position.x > enemy.transform.position.x) ? 1f : -1f;

        // Apply a direct and immediate force to the player's velocity.
        // This bypasses the regular movement physics to ensure a consistent push-off effect.
        Vector2 slideVelocity = new Vector2(slideDirection * slideForce, -downwardForce);
        rb.linearVelocity = slideVelocity;
    }



    // Visual debugging
    private void OnDrawGizmos()
    {
        // Draw ground check (always visible)
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(GroundCheckPosition, groundCheckDistance);
        
        // Draw the raycast line (always visible)
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawLine(GroundCheckPosition, GroundCheckPosition + Vector2.down * groundCheckDistance);
    }
} 