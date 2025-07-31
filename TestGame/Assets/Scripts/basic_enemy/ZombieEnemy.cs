using UnityEngine;

public class ZombieEnemy : MonoBehaviour
{
    [Header("Physics Settings")]
    [SerializeField] private float gravityScale = 1f;
    [SerializeField] private LayerMask groundLayerMask = 1; // Default layer
    
    private Rigidbody2D rb;
    private BoxCollider2D boxCollider;
    private bool isGrounded = false;
    
    void Start()
    {
        // Get required components
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        
        // Configure Rigidbody2D for gravity
        SetupRigidbody();
    }
    
    void Update()
    {
        // Check if zombie has landed on ground
        CheckGrounded();
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
            OnLanded();
        }
    }
    
    private void OnLanded()
    {
        // Disable gravity once landed
        rb.gravityScale = 0f;
        
        Debug.Log("Zombie has landed on ground!");
        
        // You can add additional behavior here when the zombie lands
        // For example, start AI behavior, play landing animation, etc.
    }
    
    // Optional: Method to set custom gravity scale
    public void SetGravityScale(float newGravityScale)
    {
        gravityScale = newGravityScale;
        if (!isGrounded)
        {
            rb.gravityScale = gravityScale;
        }
    }
} 