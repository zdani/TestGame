using System.Collections;
using UnityEngine;

public abstract class Enemy : MonoBehaviour, IHealthManager
{
    [Header("Enemy Settings")]
    [SerializeField] protected string enemyName = "Enemy";
    
    [Header("Health Settings")]
    [SerializeField] protected float maxHealth = 100f;
    [SerializeField] protected float currentHealth;
    
    [Header("Player Interaction Settings")]
    [SerializeField] protected float slideForce = 5f; // Horizontal force to make player slide off
    [SerializeField] protected float downwardForce = 3f; // Downward force to push player toward ground
    
    // Public properties
    public string EnemyName => enemyName;
    public abstract float DamageAmount { get; }
    
    // IHealthManager implementation
    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public bool IsAlive => currentHealth > 0f;
    
    protected virtual void Start()
    {
        // Initialize health
        currentHealth = maxHealth;
        
        // Get sprite renderer for flashing
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Store the original sprite color for flashing
        if (spriteRenderer != null)
        {
            originalSpriteColor = spriteRenderer.color;
        }
        
        // Base initialization - can be overridden by derived classes
    }
    
    // IHealthManager implementation
    public virtual void TakeDamage(float damage)
    {
        if (!IsAlive) return;
        
        // Check damage cooldown to prevent multiple rapid hits
        if (Time.time - lastDamageTime < DAMAGE_COOLDOWN)
        {
            Debug.Log($"{enemyName} damage blocked by cooldown. Time since last damage: {Time.time - lastDamageTime:F3}s");
            return;
        }
        
        lastDamageTime = Time.time;
        
        currentHealth = Mathf.Max(0f, currentHealth - damage);
        
        if (!IsAlive)
        {
            OnDeath();
        }
        else
        {
            OnDamageTaken(damage);
        }
    }
    
    public virtual void Heal(float amount)
    {
        if (!IsAlive) return;
        
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
    }
    
    public virtual void SetMaxHealth(float newMaxHealth)
    {
        maxHealth = newMaxHealth;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
    }
    
    // Visual feedback coroutine
    private Coroutine flashCoroutine;
    
    // Damage cooldown to prevent multiple hits from same source
    private float lastDamageTime = 0f;
    private const float DAMAGE_COOLDOWN = 0.1f; // 100ms cooldown between damage events
    
    // Store the original color to prevent issues with rapid damage
    private Color originalSpriteColor = Color.white;
    private SpriteRenderer spriteRenderer;
    
    // Virtual methods for derived classes to override
    protected virtual void OnDamageTaken(float damage)
    {
        Debug.Log($"{enemyName} took {damage} damage. Health: {currentHealth}/{maxHealth}");
        
        // Visual feedback - flash red when taking damage
        if (spriteRenderer != null)
        {
            Debug.Log($"Starting flash coroutine for {enemyName}. Current color: {spriteRenderer.color}");
            // Stop any existing flash coroutine before starting a new one
            if (flashCoroutine != null)
            {
                Debug.Log("Stopping existing flash coroutine");
                StopCoroutine(flashCoroutine);
            }
            flashCoroutine = StartCoroutine(FlashRed());
        }
        else
        {
            Debug.LogError($"SpriteRenderer is null on {enemyName}! Cannot flash.");
        }
    }
    
    protected virtual void OnDeath()
    {
        Debug.Log($"{enemyName} has died!");
        // Derived classes can override this to add death behavior
    }
    
    // Abstract method that derived classes must implement for custom collision behavior
    protected abstract void OnPlayerCollision(Player player);
    
    // Virtual method that can be overridden by derived classes for custom hit behavior
    protected virtual void OnPlayerHit(Player player)
    {
        // Default behavior - can be overridden by specific enemy types
        Debug.Log($"{enemyName} hit the player! (Base Enemy.OnPlayerHit called)");
    }
    
    // Method to deal damage to player - now handled by the enemy
    protected virtual void DealDamageToPlayer(Player player)
    {
        // Call the player's TakeDamage method directly
        player.TakeDamage(DamageAmount, enemyName);
        
        // Call the custom hit behavior
        OnPlayerHit(player);
    }
    
    // Handle physical collisions (player bumping into enemy)
    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log($"Enemy OnCollisionEnter2D with: {collision.gameObject.name} (Tag: {collision.gameObject.tag})");
        
        // Check for Player collision
        Player player = collision.gameObject.GetComponent<Player>();
        if (player != null)
        {
            Debug.Log($"Found Player component on {collision.gameObject.name}, calling OnPlayerCollision");
            
            // Check if player is landing on top of the enemy
            if (IsPlayerLandingOnEnemy(collision, player))
            {
                ApplySlideAndDownwardEffect(player);
            }
            else
            {
                // Normal side collision - deal damage
                OnPlayerCollision(player);
                DealDamageToPlayer(player);
            }
            return;
        }
        
        Debug.Log($"No Player component found on {collision.gameObject.name}");
    }
    
    // Handle trigger collisions (projectiles)
    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"Enemy OnTriggerEnter2D with: {other.gameObject.name} (Tag: {other.gameObject.tag})");
        
        // Check for FireballProjectile collision
        FireballProjectile fireball = other.gameObject.GetComponent<FireballProjectile>();
        if (fireball != null)
        {
            Debug.Log($"{enemyName} hit by fireball! Taking 1 damage.");
            TakeDamage(1f);
            
            // Destroy the fireball
            Destroy(fireball.gameObject);
            return;
        }
        
        Debug.Log($"No FireballProjectile component found on {other.gameObject.name}");
    }
    
    // Check if player is landing on top of the enemy
    private bool IsPlayerLandingOnEnemy(Collision2D collision, Player player)
    {
        // Get the contact points
        ContactPoint2D[] contacts = collision.contacts;
        
        foreach (ContactPoint2D contact in contacts)
        {
            // Check if the contact point is above the enemy's center
            if (contact.point.y > transform.position.y)
            {
                // Check if player is moving downward (landing)
                Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
                if (playerRb != null && playerRb.linearVelocity.y < 0)
                {
                    return true;
                }
            }
        }
        
        return false;
    }
    
    // Apply slide and downward effect when player lands on enemy
    private void ApplySlideAndDownwardEffect(Player player)
    {
        Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
        if (playerRb == null) return;
        
        // Determine slide direction based on player's position relative to enemy
        float slideDirection = (player.transform.position.x > transform.position.x) ? 1f : -1f;
        
        // Immediately set velocity to push player away and down (more aggressive)
        Vector2 pushVelocity = new Vector2(slideDirection * slideForce, -downwardForce);
        playerRb.linearVelocity = pushVelocity;
        
        // Temporarily disable collision between player and enemy to prevent sticking
        StartCoroutine(TemporarilyDisableCollision(player));
        
        Debug.Log($"{enemyName} pushed player with velocity {pushVelocity}");
    }
    
    // Temporarily disable collision to prevent sticking
    private IEnumerator TemporarilyDisableCollision(Player player)
    {
        // Get colliders
        Collider2D enemyCollider = GetComponent<Collider2D>();
        Collider2D playerCollider = player.GetComponent<Collider2D>();
        
        if (enemyCollider != null && playerCollider != null)
        {
            // Disable collision
            Physics2D.IgnoreCollision(enemyCollider, playerCollider, true);
            
            // Wait a short time
            yield return new WaitForSeconds(0.2f);
            
            // Re-enable collision
            Physics2D.IgnoreCollision(enemyCollider, playerCollider, false);
        }
    }
    
    // Visual feedback coroutine
    private IEnumerator FlashRed()
    {
        Debug.Log($"FlashRed coroutine started for {enemyName}");
        Debug.Log($"Original color: {originalSpriteColor}, Setting to red");
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        Debug.Log($"Setting color back to: {originalSpriteColor}");
        spriteRenderer.color = originalSpriteColor;
        flashCoroutine = null; // Clear the reference when done
        Debug.Log($"FlashRed coroutine finished for {enemyName}");
    }
} 