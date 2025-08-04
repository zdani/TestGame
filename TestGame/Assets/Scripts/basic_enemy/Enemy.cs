using System.Collections;
using UnityEngine;

public abstract class Enemy : MonoBehaviour, IHealthManager
{
    [Header("Enemy Settings")]
    [SerializeField] protected string enemyName = "Enemy";
    
    [Header("Health Settings")]
    [SerializeField] protected float maxHealth = 100f;
    [SerializeField] protected float currentHealth;

    [Header("Detection Settings")]
    [SerializeField] protected float detectionRadius = 10f;
    protected Transform playerTransform;
    protected bool playerDetected = false;
    
    [Header("Player Interaction Settings")]
    [SerializeField] protected float slideForce = 5f; // Horizontal force to make player slide off
    [SerializeField] protected float downwardForce = 3f; // Downward force to push player toward ground
    
    // Public properties
    public string EnemyName => enemyName;
    public abstract float DamageAmount { get; }
    public virtual bool CanDealContactDamage => true;
    
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

        playerTransform = FindFirstObjectByType<Player>()?.transform;
        
        // Ensure enemy sprites are drawn on top of the player so you can see their attacks
        spriteRenderer.sortingOrder = 1;
        
        // Base initialization - can be overridden by derived classes
    }

    protected virtual void Update()
    {
        CheckForPlayer();
    }

    protected virtual void CheckForPlayer()
    {

        if (playerTransform == null || !IsAlive) return;
        
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        bool wasDetected = playerDetected;
        playerDetected = distanceToPlayer <= detectionRadius;
        
        if (playerDetected && !wasDetected)
        {
            OnPlayerDetected();
        }
        else if (!playerDetected && wasDetected)
        {
            OnPlayerLost();
        }
    }

    protected virtual void OnPlayerDetected()
    {
        Debug.Log($"{enemyName} detected player!");
    }

    protected virtual void OnPlayerLost()
    {
        Debug.Log($"{enemyName} lost player!");
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
            Debug.LogWarning($"SpriteRenderer is null on {enemyName}! Cannot flash.");
        }
    }
    
    protected virtual void OnDeath()
    {
        // Ensure dead enemies appear behind the player
        spriteRenderer.sortingOrder = -1;
        Debug.Log($"{enemyName} has died!");
    }
    
    // Abstract method that derived classes must implement for custom collision behavior
    protected abstract void OnPlayerCollision(Player player);
    
    // Virtual method that can be overridden by derived classes for custom hit behavior
    protected virtual void OnPlayerHit(Player player)
    {
        // Default behavior - can be overridden by specific enemy types
        //Debug.Log($"{enemyName} hit the player! (Base Enemy.OnPlayerHit called)");
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
    protected virtual void OnCollisionStay2D(Collision2D collision)
    {
        // Check for Player collision and ensure the enemy is alive
        if (IsAlive && CanDealContactDamage && collision.gameObject.TryGetComponent<Player>(out var player))
        {
            // Let the concrete implementation handle specific collision effects
            OnPlayerCollision(player);
            
            // The enemy's primary job on collision is to deal damage
            // The player's TakeDamage method handles invincibility frames.
            DealDamageToPlayer(player);
        }
    }
    
    // Handle trigger collisions (projectiles and player proximity)
    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        // Check for Player proximity (if using a larger trigger collider)
        Player player = other.gameObject.GetComponent<Player>();
        if (player != null && IsAlive)
        {
            if (!CanDealContactDamage) return;
            
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
        
        // Check for Boulder collision
        Boulder boulder = other.gameObject.GetComponent<Boulder>();
        if (boulder != null)
        {
            Debug.Log($"{enemyName} hit by boulder! Taking 3 damage.");
            TakeDamage(3f); // Boulders do more damage
            
            // Destroy the boulder after impact
            //Destroy(other.gameObject);
            return;
        }
        
        //Debug.Log($"No FireballProjectile component found on {other.gameObject.name}");
    }
    
    // Visual feedback coroutine
    private IEnumerator FlashRed()
    {
        if(spriteRenderer == null) {
            Debug.LogWarning($"SpriteRenderer is null on {enemyName}! Cannot flash.");
            yield break;
        }
        Debug.Log($"FlashRed coroutine started for {enemyName}");
        Debug.Log($"Original color: {originalSpriteColor}, Setting to red");
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        Debug.Log($"Setting color back to: {originalSpriteColor}");
        spriteRenderer.color = originalSpriteColor;
        flashCoroutine = null; // Clear the reference when done
        Debug.Log($"FlashRed coroutine finished for {enemyName}");
    }

    protected void OnDrawGizmosSelected()
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
