using UnityEngine;

public class ZombieEnemy : MonoBehaviour
{
    [Header("Physics Settings")]
    [SerializeField] private float gravityScale = 1f;
    [SerializeField] private LayerMask groundLayerMask = 1; // Default layer
    
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 2f;
    [SerializeField] private float edgeDetectionDistance = 0.1f;
    
    private Rigidbody2D rb;
    private BoxCollider2D boxCollider;
    private SpriteRenderer spriteRenderer;
    private bool isGrounded = false;
    private GameObject currentPlatform;
    private Collider2D platformCollider;
    private bool walkingRight = true;
    private bool isWalking = false;
    
    void Start()
    {
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
        if (isWalking)
        {
            WalkOnPlatform();
        }
    }
    
    private void SetupRigidbody()
    {
        rb.gravityScale = gravityScale;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation; // Prevent rotation
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous; // Better collision detection
        // Ensure the zombie starts falling
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = gravityScale;
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
        Debug.Log("Zombie has landed on ground!");
        
        // Start walking behavior
        isWalking = true;
        StartWalking();
    }
    
    private void StartWalking()
    {
        // Set initial walking direction
        walkingRight = true;
        
        Debug.Log($"Zombie started walking on platform: {currentPlatform.name}");
    }
    
    private void WalkOnPlatform()
    {
        if (currentPlatform == null || platformCollider == null) return;
        
        // Check if we're at the edge of the platform
        if (IsAtPlatformEdge())
        {
            // Turn around
            walkingRight = !walkingRight;
            Debug.Log($"Zombie turned around at platform edge. Now walking {(walkingRight ? "right" : "left")}");
        }
        
        // Move in current direction
        float direction = walkingRight ? 1f : -1f;
        Vector2 movement = new Vector2(direction * walkSpeed, 0f);
        rb.linearVelocity = movement;
        
        // Flip sprite based on direction
        FlipSprite();
    }
    
    private void FlipSprite()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = walkingRight;
        }
    }
    
    private bool IsAtPlatformEdge()
    {
        if (platformCollider == null) return false;
        
        // Get the platform bounds
        Bounds platformBounds = platformCollider.bounds;
        
        // Check if zombie is near the left or right edge
        float zombieX = transform.position.x;
        float leftEdge = platformBounds.min.x + edgeDetectionDistance;
        float rightEdge = platformBounds.max.x - edgeDetectionDistance;
        
        return zombieX <= leftEdge || zombieX >= rightEdge;
    }
} 