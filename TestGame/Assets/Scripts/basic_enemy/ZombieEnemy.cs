using UnityEngine;

public enum ZombieState
{
    Falling,
    WalkingLeft,
    WalkingRight
}

public class ZombieEnemy : Enemy
{
    [Header("Zombie Settings")]
    [SerializeField] private float damageAmount = 30f;
    [SerializeField] private LayerMask groundLayerMask = 1; // Default layer
    
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 2f;
    [SerializeField] private float edgeDetectionDistance = 0.1f;
    [SerializeField] private float turnBufferDistance = 0.5f; // Buffer to prevent rapid turning
    
    // Implementation of abstract property from Enemy base class
    public override float DamageAmount => damageAmount;
    
    private Rigidbody2D rb;
    private BoxCollider2D boxCollider;
    private SpriteRenderer spriteRenderer;
    private bool isGrounded = false;
    private GameObject currentPlatform;
    private Collider2D platformCollider;
    private ZombieState currentState = ZombieState.Falling;
    private bool hasTurnedRecently = false; // Prevent rapid turning
    private float lastTurnTime = 0f;
    private const float TURN_COOLDOWN = 1f; // Minimum time between turns
    
    protected override void Start()
    {
        // Call base class Start method
        base.Start();
        
        // Set zombie-specific properties
        enemyName = "Zombie";
        
        // Get required components
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Configure Rigidbody2D for gravity
        SetupRigidbody();
    }
    
    void Update()
    {
        // Check if zombie has landed on ground
        CheckGrounded();
        
        // Handle walking behavior
        if (currentState == ZombieState.WalkingLeft || currentState == ZombieState.WalkingRight)
        {
            WalkOnPlatform();
        }
    }
    
    void FixedUpdate()
    {
        // Alternative: Move edge detection here for less frequent checks
        // This runs at a fixed time step (usually 50 times per second)
        if (currentState == ZombieState.WalkingLeft || currentState == ZombieState.WalkingRight)
        {
            CheckForEdgeTurn();
        }
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
        currentState = ZombieState.WalkingRight;
    }
    
    private void WalkOnPlatform()
    {
        if (currentPlatform == null || platformCollider == null) return;
        
        // Move in current direction
        float direction = (currentState == ZombieState.WalkingRight) ? 1f : -1f;
        Vector2 movement = new Vector2(direction * walkSpeed, 0f);
        rb.linearVelocity = movement;
        
        // Flip sprite based on direction
        FlipSprite();
    }
    
    private void CheckForEdgeTurn()
    {
        // Check if we're at the edge of the platform
        if (IsAtPlatformEdge())
        {
            // Turn around
            currentState = (currentState == ZombieState.WalkingRight) ? ZombieState.WalkingLeft : ZombieState.WalkingRight;
        }
    }
    
    private void FlipSprite()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = (currentState == ZombieState.WalkingRight);
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
} 