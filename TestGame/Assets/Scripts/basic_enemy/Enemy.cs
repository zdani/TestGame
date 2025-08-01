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
        IceShieldAbility iceShieldAbility = gameObject.GetComponent<IceShieldAbility>();
        if (iceShieldAbility != null && iceShieldAbility.isShieldActive){
            iceShieldAbility.BreakShield();
            return;
        }
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
        // Check for Player collision
        if (collision.gameObject.TryGetComponent<Player>(out var player))
        {
            // Let the concrete implementation handle specific collision effects
            OnPlayerCollision(player);
            
            // The enemy's primary job on collision is to deal damage
            DealDamageToPlayer(player);
        }
    }
    
    // Handle trigger collisions (projectiles and player proximity)
    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        //Debug.Log($"Enemy OnTriggerEnter2D with: {other.gameObject.name} (Tag: {other.gameObject.tag})");
        
        // Check for Player proximity (if using a larger trigger collider)
        Player player = other.gameObject.GetComponent<Player>();
        if (player != null)
        {
            // If player is moving fast and about to collide, force a collision check
            Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
            if (playerRb != null && Mathf.Abs(playerRb.linearVelocity.x) > 5f)
            {
                // Force collision detection for fast-moving players
                OnPlayerCollision(player);
                DealDamageToPlayer(player);
            }
            return;
        }
        
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
        
        //Debug.Log($"No FireballProjectile component found on {other.gameObject.name}");
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