using UnityEngine;
using System.Collections;

public enum ZombieState
{
    Falling,
    Walking,
    Chasing,
    Dead
}

public class ZombieEnemy : Enemy
{
    [Header("Zombie Settings")]
    [SerializeField] private float damageAmount = 1f;
    [SerializeField] private LayerMask groundLayerMask = 1; // Default layer
    
    [Header("Zombie Health Settings")]
    [SerializeField] private float zombieMaxHealth = 50f;
    
    [Header("Detection Settings")]
    [SerializeField] private float detectionRadius = 5f;
    [SerializeField] private Transform playerTransform; // Drag player here in inspector
    
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 2f;
    [SerializeField] private float chaseSpeed = 4f; // Speed when chasing player
    [SerializeField] private float edgeDetectionDistance = 0.1f;
    [SerializeField] private float turnBufferDistance = 0.5f; // Buffer to prevent rapid turning
    
    // Animation state constants
    private const string ANIM_IS_WALKING = "IsWalking";
    private const string ANIM_IS_CHASING = "IsChasing";
    private const string ANIM_IS_DEAD = "IsDead";
    
    // Implementation of abstract property from Enemy base class
    public override float DamageAmount => damageAmount;
    
    private Rigidbody2D rb;
    private BoxCollider2D boxCollider;
    private SpriteRenderer spriteRenderer;
    private Animator animator; // For animation control
    private bool isGrounded = false;
    private GameObject currentPlatform;
    private Collider2D platformCollider;
    private ZombieState currentState = ZombieState.Falling;
    private bool isMovingRight = true; // Track movement direction separately
    private bool hasTurnedRecently = false; // Prevent rapid turning
    private float lastTurnTime = 0f;
    private const float TURN_COOLDOWN = 1f; // Minimum time between turns
    
    // Player detection
    private bool playerDetected = false;
    
    protected override void Start()
    {
        // Set zombie-specific health before calling base Start
        SetMaxHealth(zombieMaxHealth);
        
        // Call base class Start method
        base.Start();
        
        // Set zombie-specific properties
        enemyName = "Zombie";
        
        // Get required components
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        
        // Configure Rigidbody2D for gravity
        SetupRigidbody();
    }
    
    void Update()
    {
        // Don't update if dead
        if (currentState == ZombieState.Dead) return;
        
        // Check if zombie has landed on ground
        CheckGrounded();
        
        // Check for player detection
        CheckForPlayer();
        
        // Handle movement behavior based on state
        if (IsWalkingState())
        {
            WalkOnPlatform();
        }
        else if (IsChasingState())
        {
            ChasePlayer();
        }
    }
    
    void FixedUpdate()
    {
        // Don't update if dead
        if (currentState == ZombieState.Dead) return;
        
        // Alternative: Move edge detection here for less frequent checks
        // This runs at a fixed time step (usually 50 times per second)
        if (IsWalkingState())
        {
            CheckForEdgeTurn();
        }
    }
    
    // Override health management methods for zombie-specific behavior
    public override void TakeDamage(float damage)
    {
        if (currentState == ZombieState.Dead) return;
        
        // Call base implementation
        base.TakeDamage(damage);
        
        // Zombie-specific damage behavior
        if (IsAlive)
        {
            // Zombies might get more aggressive when damaged
            if (playerDetected && currentState == ZombieState.Walking)
            {
                OnPlayerDetected(); // Switch to chase mode
            }
        }
    }
    
    protected override void OnDamageTaken(float damage)
    {
        base.OnDamageTaken(damage);
        
        // Zombie-specific damage feedback
        Debug.Log($"Zombie grunts in pain! Took {damage} damage. Health: {CurrentHealth}/{MaxHealth}");
        
        // Visual feedback - could flash red, play sound, etc.
        if (spriteRenderer != null)
        {
            StartCoroutine(FlashRed());
        }
    }
    
    protected override void OnDeath()
    {
        base.OnDeath();
        
        // Set zombie to dead state
        currentState = ZombieState.Dead;
        
        // Stop all movement
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic; // Prevent further physics interactions
        }
        
        // Disable collider to prevent further collisions
        if (boxCollider != null)
        {
            boxCollider.enabled = false;
        }
        
        // Set death animation
        SetAnimationState(currentState);
        
        Debug.Log("Zombie has been defeated!");
    }
    
    // Visual feedback coroutine
    private IEnumerator FlashRed()
    {
        Color originalColor = spriteRenderer.color;
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = originalColor;
    }
    
    private void SetupRigidbody()
    {

        rb.constraints = RigidbodyConstraints2D.FreezeRotation; // Prevent rotation

        // Ensure the zombie starts falling
        rb.linearVelocity = Vector2.zero;
    }
    
    
    private void CheckGrounded()
    {
        if (isGrounded) return;
        
        // Cast a ray downward to detect ground
        Vector2 rayOrigin = transform.position;
        float rayDistance = boxCollider.bounds.extents.y + 0.1f; // Slightly more than half the collider height
        
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, rayDistance, groundLayerMask);
        
        if (hit.collider != null)
        {
            // Zombie has hit ground
            isGrounded = true;
            currentPlatform = hit.collider.gameObject;
            platformCollider = hit.collider;
            OnLanded();
        }
    }
    
    private void OnLanded()
    {         
        // Start walking behavior
        currentState = ZombieState.Walking;
        SetAnimationState(currentState);
    }
    
    private void CheckForPlayer()
    {
        if (playerTransform == null) return;
        
        // Calculate distance to player
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        
        // Check if player is within detection radius
        bool wasDetected = playerDetected;
        playerDetected = distanceToPlayer <= detectionRadius;
        
        // Handle state transitions
        if (playerDetected && !wasDetected)
        {
            // Player just entered detection radius
            OnPlayerDetected();
        }
        else if (!playerDetected && wasDetected)
        {
            // Player just left detection radius
            OnPlayerLost();
        }
    }
    
    private void OnPlayerDetected()
    {
        Debug.Log("Zombie detected player! Switching to chase mode.");
        
        // Determine chase direction based on player position
        isMovingRight = playerTransform.position.x > transform.position.x;
        
        // Set chase state
        currentState = ZombieState.Chasing;
        SetAnimationState(currentState);
    }
    
    private void OnPlayerLost()
    {
        Debug.Log("Zombie lost player! Returning to patrol mode.");
        
        // Return to walking state
        currentState = ZombieState.Walking;
        SetAnimationState(currentState);
    }
    
    private void ChasePlayer()
    {
        if (playerTransform == null) return;
        
        // Determine direction to player
        isMovingRight = playerTransform.position.x > transform.position.x;
        float direction = isMovingRight ? 1f : -1f;
        
        // Move towards player at chase speed
        Vector2 movement = new Vector2(direction * chaseSpeed, 0f);
        rb.linearVelocity = movement;
        
        // Flip sprite based on direction
        FlipSprite();
    }
    
    private bool IsWalkingState()
    {
        return currentState == ZombieState.Walking;
    }
    
    private bool IsChasingState()
    {
        return currentState == ZombieState.Chasing;
    }
    
    private void SetAnimationState(ZombieState state)
    {
        /*if (animator != null)
        {
            // Reset all animation parameters
            animator.SetBool(ANIM_IS_WALKING, false);
            animator.SetBool(ANIM_IS_CHASING, false);
            animator.SetBool(ANIM_IS_DEAD, false);
            
            // Set the appropriate animation state
            switch (state)
            {
                case ZombieState.Walking:
                    animator.SetBool(ANIM_IS_WALKING, true);
                    break;
                case ZombieState.Chasing:
                    animator.SetBool(ANIM_IS_CHASING, true);
                    break;
                case ZombieState.Dead:
                    animator.SetBool(ANIM_IS_DEAD, true);
                    break;
                case ZombieState.Falling:
                    // No animation for falling state
                    break;
            }
        }*/
    }
    
    private void WalkOnPlatform()
    {
        if (currentPlatform == null || platformCollider == null) return;
        
        // Move in current direction
        float direction = isMovingRight ? 1f : -1f;
        Vector2 movement = new Vector2(direction * walkSpeed, 0f);
        rb.linearVelocity = movement;
        
        // Flip sprite based on direction
        FlipSprite();
        
        // Set walk animation
        SetAnimationState(currentState);
    }
    
    private void CheckForEdgeTurn()
    {
        // Check if we're at the edge of the platform
        if (IsAtPlatformEdge())
        {
            // Turn around
            isMovingRight = !isMovingRight;
        }
    }
    
    private void FlipSprite()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = isMovingRight;
        }
    }
    
    private bool IsAtPlatformEdge()
    {
        if (platformCollider == null) return false;
        
        // Don't check for edges if we just turned
        if (hasTurnedRecently && Time.time - lastTurnTime < TURN_COOLDOWN)
        {
            return false;
        }
        
        // Get the platform bounds
        Bounds platformBounds = platformCollider.bounds;
        
        // Check if zombie is near the left or right edge with buffer
        float zombieX = transform.position.x;
        float leftEdge = platformBounds.min.x + edgeDetectionDistance + turnBufferDistance;
        float rightEdge = platformBounds.max.x - edgeDetectionDistance - turnBufferDistance;
        
        bool atEdge = zombieX <= leftEdge || zombieX >= rightEdge;
        
        // If we're at an edge, mark that we've turned recently
        if (atEdge)
        {
            hasTurnedRecently = true;
            lastTurnTime = Time.time;
        }
        else
        {
            // Reset the flag when we're away from edges
            hasTurnedRecently = false;
        }
        
        return atEdge;
    }
    
    // Implementation of abstract method from Enemy base class
    protected override void OnPlayerCollision(Player player)
    {
        Debug.Log($"ZombieEnemy.OnPlayerCollision called with player: {player.name}");
        
        // Zombie-specific collision behavior can be added here if needed
        Debug.Log($"Zombie collided with player! Will deal {damageAmount} damage.");
    }
    
    // Visual debug for detection radius (only visible in Scene view)
    private void OnDrawGizmosSelected()
    {
        // Draw detection radius
        Gizmos.color = playerDetected ? Color.red : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        
        // Draw line to player if detected
        if (playerDetected && playerTransform != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, playerTransform.position);
        }
    }
} 