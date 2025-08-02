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
    [SerializeField] private float maxPatrolDistance = 10f; // Maximum distance from starting point
    [SerializeField] private float chaseTimeout = 4f; // How long to keep chasing after losing player

    [Header("Patrol Settings")] 
    [SerializeField] private bool startPatrolRight = true;
    
    [Header("Chase Settings")]
    [SerializeField] private float directionChangeCooldown = 1.5f;
    [SerializeField] private float stuckTurnAroundTime = 2f;
    
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
    private ZombieState currentState = ZombieState.Falling;
    private bool isMovingRight; // Track movement direction separately
    
    // Movement tracking
    private Vector2 startingPosition;
    private float chaseTimeoutTimer = 0f;
    private bool isChaseTimeoutActive = false;
    private bool isReturningToPatrol = false; // Track if zombie is heading back to starting area
    private float lastDirectionChangeTime = 0f;
    
    // Stuck detection
    private Vector2 lastPosition;
    private float timeStuck;
    
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

        // Store starting for patrol limits
        startingPosition = transform.position;

        // Set initial patrol direction
        isMovingRight = startPatrolRight;

        // Initialize stuck detection variables
        lastPosition = transform.position;
        timeStuck = 0f;
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
        Vector2 rayOrigin = new Vector2(transform.position.x, zombieCollider.bounds.min.y + 0.1f);
        float rayDistance = 0.2f; 
        
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, rayDistance, groundLayerMask);
        
        if (hit.collider != null)
        {
            // Zombie has hit ground
            isGrounded = true;
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
        if (currentState == ZombieState.Dead) return;
        
        Debug.Log("Zombie detected player! Switching to chase mode.");
        
        // Reset chase timeout
        isChaseTimeoutActive = false;
        chaseTimeoutTimer = 0f;
        
        // Determine chase direction based on player position
        isMovingRight = playerTransform.position.x > transform.position.x;
        
        // Set chase state
        currentState = ZombieState.Chasing;
        SetAnimationState(currentState);
        
        // Reset the direction change cooldown so the zombie can turn immediately.
        lastDirectionChangeTime = 0f;
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
        
        // Determine the direction to the player
        bool newDirectionIsRight = playerTransform.position.x > transform.position.x;
        
        // If the direction needs to change, check the cooldown
        if (newDirectionIsRight != isMovingRight)
        {
            if (Time.time - lastDirectionChangeTime > directionChangeCooldown)
            {
                isMovingRight = newDirectionIsRight;
                lastDirectionChangeTime = Time.time;
            }
        }

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
        // Stuck detection logic
        if (Vector2.Distance(transform.position, lastPosition) < 0.1f)
        {
            timeStuck += Time.deltaTime;
        }
        else
        {
            lastPosition = transform.position;
            timeStuck = 0f;
        }

        if (timeStuck >= stuckTurnAroundTime)
        {
            isMovingRight = !isMovingRight;
            timeStuck = 0f; // Reset timer after turning
        }
        
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
        // The point to start our raycast is at the leading edge of the collider.
        Vector2 raycastOrigin = new Vector2(
            zombieCollider.bounds.center.x + (direction * (zombieCollider.bounds.extents.x)),
            zombieCollider.bounds.min.y + 0.1f // Start the ray slightly above the bottom
        );

        // Cast a short ray downwards from just in front of the zombie.
        // If it doesn't hit anything on the ground layer, we're at a ledge.
        float raycastDistance = 0.2f;
        RaycastHit2D hit = Physics2D.Raycast(raycastOrigin, Vector2.down, raycastDistance, groundLayerMask);
        
        // You can enable this for debugging to see the raycasts in the Scene view
        // Debug.DrawRay(raycastOrigin, Vector2.down * raycastDistance, hit.collider != null ? Color.green : Color.red);

        return hit.collider == null;
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
