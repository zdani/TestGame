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
    [SerializeField] private LayerMask groundLayerMask;
    
    [Header("Zombie Health Settings")]
    [SerializeField] private float zombieMaxHealth = 3f;
    
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 2f;
    [SerializeField] private float chaseSpeed = 4f; // Speed when chasing player
    [SerializeField] private float edgeDetectionDistance = 0.1f;
    [SerializeField] private float maxPatrolDistance = 10f; // Maximum distance from starting point
    [SerializeField] private float chaseTimeout = 4f; // How long to keep chasing after losing player

    [Header("Patrol Settings")] 
    [SerializeField] private bool startPatrolRight = true;
    
    // Animation state constants
    private const string ANIM_IS_WALKING = "IsWalking";
    private const string ANIM_IS_CHASING = "IsChasing";
    private const string ANIM_IS_DEAD = "IsDead";
    
    // Implementation of abstract property from Enemy base class
    public override float DamageAmount => damageAmount;
    
    private Rigidbody2D rb;
    private Collider2D zombieCollider; // Changed to generic Collider2D for capsule support
    private Animator animator; // For animation control
    private bool isGrounded = false;
    private GameObject currentPlatform;
    private Collider2D platformCollider;
    private ZombieState currentState = ZombieState.Falling;
    private bool isMovingRight; // Track movement direction separately
    
    // Movement tracking
    private Vector2 startingPosition;
    private float chaseTimeoutTimer = 0f;
    private bool isChaseTimeoutActive = false;
    private bool isReturningToPatrol = false; // Track if zombie is heading back to starting area
    
    
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
        zombieCollider = GetComponent<Collider2D>();
        animator = GetComponent<Animator>();

        // Configure Rigidbody2D for gravity
        SetupRigidbody();

        // Set up ground layer mask properly
        groundLayerMask = LayerMask.GetMask("Ground");

        // Store starting position for patrol limits
        startingPosition = transform.position;

        // Set initial patrol direction
        isMovingRight = startPatrolRight;
    }
    
    protected override void Update()
    {
        base.Update();
        
        // Don't update if dead
        if (currentState == ZombieState.Dead) return;
        
        // Check if zombie has landed on ground
        CheckGrounded();
        
        // Handle chase timeout
        HandleChaseTimeout();
        
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
        
        // FixedUpdate can be used for other physics-based checks if needed
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
            OnPlayerDetected(); // Switch to chase mode
        }
    }
    
    protected override void OnDamageTaken(float damage)
    {
        base.OnDamageTaken(damage);
        
        // Zombie-specific damage feedback
        Debug.Log($"Zombie grunts in pain! Took {damage} damage. Health: {CurrentHealth}/{MaxHealth}");
        
        // Additional zombie-specific damage behavior can be added here
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
        if (zombieCollider != null)
        {
            zombieCollider.enabled = false;
        }
        
        // Set death animation
        SetAnimationState(currentState);
        
        Debug.Log("Zombie has been defeated!");
    }
    

    

    
    private void SetupRigidbody()
    {
        rb.constraints = RigidbodyConstraints2D.FreezeRotation; // Prevent rotation
        
        // Enable continuous collision detection to prevent fast-moving objects from passing through
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        // Ensure the zombie starts falling
        rb.linearVelocity = Vector2.zero;
    }
    
    
    private void CheckGrounded()
    {
        if (isGrounded) return;
        
        // Cast a ray downward to detect ground
        Vector2 rayOrigin;
        if (zombieCollider != null)
        {
            // Use the bottom of the collider bounds
            rayOrigin = new Vector2(transform.position.x, zombieCollider.bounds.min.y);
        }
        else
        {
            // Fallback to transform position if no collider
            rayOrigin = (Vector2)transform.position + Vector2.down * 0.5f;
        }
        
        float rayDistance = 0.1f; // Short distance since we're at the exact bottom
        
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
    
    
    protected override void OnPlayerDetected()
    {
        Debug.Log("Zombie detected player! Switching to chase mode.");
        
        // Reset chase timeout
        isChaseTimeoutActive = false;
        chaseTimeoutTimer = 0f;
        
        // Determine chase direction based on player position
        isMovingRight = playerTransform.position.x > transform.position.x;
        
        // Set chase state
        currentState = ZombieState.Chasing;
        SetAnimationState(currentState);
    }
    
    private void HandleChaseTimeout()
    {
        if (isChaseTimeoutActive)
        {
            chaseTimeoutTimer -= Time.deltaTime;
            
            if (chaseTimeoutTimer <= 0f)
            {
                // Chase timeout expired, return to walking
                isChaseTimeoutActive = false;
                currentState = ZombieState.Walking;
                
                // Set direction toward starting position when returning to patrol
                isMovingRight = startingPosition.x > transform.position.x;
                isReturningToPatrol = true; // Mark that we're heading back to starting area
                
                SetAnimationState(currentState);
                Debug.Log("Zombie chase timeout expired! Returning to patrol mode and heading toward starting position.");
            }
        }
    }
    
    protected override void OnPlayerLost()
    {
        Debug.Log("Zombie lost player! Starting chase timeout.");
        
        // Start chase timeout instead of immediately returning to walking
        isChaseTimeoutActive = true;
        chaseTimeoutTimer = chaseTimeout;
    }
    
    private void ChasePlayer()
    {
        if (playerTransform == null) return;
        
        // Determine direction to player
        isMovingRight = playerTransform.position.x > transform.position.x;
        float direction = isMovingRight ? 1f : -1f;
        
        // Check if moving in this direction would go off the platform edge
        if (WouldGoOffEdge(direction))
        {
            // Don't move off the edge, but keep chase state active
            rb.linearVelocity = Vector2.zero;
            Debug.Log("Zombie stopped at platform edge while chasing!");
        }
        else
        {
            // Move towards player at chase speed
            Vector2 movement = new Vector2(direction * chaseSpeed, 0f);
            rb.linearVelocity = movement;
        }
        
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
        if (animator != null)
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
        }
    }
    
    private void WalkOnPlatform()
    {
        if (currentPlatform == null || platformCollider == null) return;
        
        // Move in current direction
        float direction = isMovingRight ? 1f : -1f;
        
        // Check if moving in this direction would go off the platform edge
        if (WouldGoOffEdge(direction))
        {
            // Turn around
            isMovingRight = !isMovingRight;
            direction = isMovingRight ? 1f : -1f;
        }
        // Only check patrol distance if we're not returning to patrol area
        else if (!isReturningToPatrol && WouldExceedPatrolDistance(direction))
        {
            // Turn around
            isMovingRight = !isMovingRight;
            direction = isMovingRight ? 1f : -1f;
        }
        
        Vector2 movement = new Vector2(direction * walkSpeed, 0f);
        rb.linearVelocity = movement;
        
        // Flip sprite based on direction
        FlipSprite();
        
        // Set walk animation
        SetAnimationState(currentState);
        
        // Check if we've reached the starting area and can stop returning to patrol
        if (isReturningToPatrol)
        {
            float distanceFromStart = Mathf.Abs(transform.position.x - startingPosition.x);
            if (distanceFromStart <= maxPatrolDistance)
            {
                isReturningToPatrol = false;
                Debug.Log("Zombie has returned to patrol area! Resuming normal patrol behavior.");
            }
        }
    }
    

    
    private void FlipSprite()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = isMovingRight;
        }
    }
    
    private bool WouldGoOffEdge(float direction)
    {
        if (platformCollider == null) return false;
        
        // Get the platform bounds
        Bounds platformBounds = platformCollider.bounds;
        
        // Calculate where the zombie would be after moving
        float zombieX = transform.position.x;
        float nextX = zombieX + (direction * walkSpeed * Time.deltaTime);
        
        // Check if the next position would be off the platform
        float leftEdge = platformBounds.min.x + edgeDetectionDistance;
        float rightEdge = platformBounds.max.x - edgeDetectionDistance;
        
        return nextX <= leftEdge || nextX >= rightEdge;
    }
    
    private bool WouldExceedPatrolDistance(float direction)
    {
        // Calculate where the zombie would be after moving
        float zombieX = transform.position.x;
        float nextX = zombieX + (direction * walkSpeed * Time.deltaTime);
        
        // Check if the next position would exceed max patrol distance from starting point
        float distanceFromStart = Mathf.Abs(nextX - startingPosition.x);
        
        return distanceFromStart > maxPatrolDistance;
    }



    // Implementation of abstract method from Enemy base class
    protected override void OnPlayerCollision(Player player)
    {
    }
} 