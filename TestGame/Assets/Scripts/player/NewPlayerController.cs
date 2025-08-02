using UnityEngine;

public class NewPlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float airControlMultiplier = 0.5f;
    [SerializeField] private float maxAirSpeed = 8f;
    
    [Header("Ground Detection")]
    [SerializeField] private LayerMask groundLayerMask = 1;
    [SerializeField] private float groundCheckDistance = 0.2f;
    
    [Header("Enemy Interaction")]
    [SerializeField] private float slideForce = 8f;
    [SerializeField] private float downwardForce = 6f;
    
    // Components
    private Rigidbody2D rb;
    private Animator animator;
    
    // State
    private bool isGrounded;
    private bool isMoving;
    
    // Ground check position
    private Vector2 GroundCheckPosition => (Vector2)transform.position + Vector2.down * 0.5f;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        
        // Configure rigidbody
        rb.freezeRotation = true;
        rb.gravityScale = 1f;
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
        isGrounded = Physics2D.Raycast(GroundCheckPosition, Vector2.down, groundCheckDistance, groundLayerMask);
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
        // Debug: Check if space is being pressed
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log($"Space pressed! IsGrounded: {isGrounded}");
        }
        
        // Jump only when grounded and space is pressed
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            Debug.Log($"Jumping! Setting velocity.y to {jumpForce}");
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
        Enemy enemy = collision.gameObject.GetComponent<Enemy>();
        if (enemy != null)
        {
            HandleEnemyCollision(collision, enemy);
        }
    }
    
    private void HandleEnemyCollision(Collision2D collision, Enemy enemy)
    {
        // Check if player is landing on top of enemy
        if (IsLandingOnEnemy(collision))
        {
            // Apply slide effect
            ApplySlideEffect();
            
            // Temporarily disable collision to prevent sticking
            StartCoroutine(TemporarilyDisableCollision(collision.collider));
        }
        else
        {
            // Side collision - player takes damage
            Player player = GetComponent<Player>();
            if (player != null)
            {
                player.TakeDamage(enemy.DamageAmount, enemy.EnemyName);
            }
        }
    }
    
    private bool IsLandingOnEnemy(Collision2D collision)
    {
        // Check if player is above the enemy and moving downward
        if (rb.linearVelocity.y < 0)
        {
            ContactPoint2D[] contacts = collision.contacts;
            foreach (ContactPoint2D contact in contacts)
            {
                // If contact point is above the enemy's center, we're landing on it
                if (contact.point.y > collision.transform.position.y)
                {
                    return true;
                }
            }
        }
        return false;
    }
    
    private void ApplySlideEffect()
    {
        // Determine slide direction based on player position relative to enemy
        float slideDirection = transform.position.x > 0 ? 1f : -1f;
        
        // Apply slide and downward force
        Vector2 slideVelocity = new Vector2(slideDirection * slideForce, -downwardForce);
        rb.linearVelocity = slideVelocity;
    }
    
    private System.Collections.IEnumerator TemporarilyDisableCollision(Collider2D enemyCollider)
    {
        Collider2D playerCollider = GetComponent<Collider2D>();
        
        if (playerCollider != null && enemyCollider != null)
        {
            // Disable collision
            Physics2D.IgnoreCollision(playerCollider, enemyCollider, true);
            
            // Wait a short time
            yield return new WaitForSeconds(0.1f);
            
            // Re-enable collision
            Physics2D.IgnoreCollision(playerCollider, enemyCollider, false);
        }
    }
    
    // Visual debugging
    private void OnDrawGizmosSelected()
    {
        // Draw ground check
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(GroundCheckPosition, groundCheckDistance);
    }
} 