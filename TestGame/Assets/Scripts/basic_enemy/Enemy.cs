using UnityEngine;

public abstract class Enemy : MonoBehaviour, IHealthManager
{
    [Header("Enemy Settings")]
    [SerializeField] protected string enemyName = "Enemy";
    
    [Header("Health Settings")]
    [SerializeField] protected float maxHealth = 100f;
    [SerializeField] protected float currentHealth;
    
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
        // Base initialization - can be overridden by derived classes
    }
    
    // IHealthManager implementation
    public virtual void TakeDamage(float damage)
    {
        if (!IsAlive) return;
        
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
    
    // Virtual methods for derived classes to override
    protected virtual void OnDamageTaken(float damage)
    {
        Debug.Log($"{enemyName} took {damage} damage. Health: {currentHealth}/{maxHealth}");
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
    
    // Collision detection methods - enemy is now responsible for detecting hits
    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log($"Enemy OnCollisionEnter2D with: {collision.gameObject.name} (Tag: {collision.gameObject.tag})");
        
        // Check for Player collision
        Player player = collision.gameObject.GetComponent<Player>();
        if (player != null)
        {
            Debug.Log($"Found Player component on {collision.gameObject.name}, calling OnPlayerCollision");
            OnPlayerCollision(player);
            DealDamageToPlayer(player);
            return;
        }
        
        // Check for FireballProjectile collision
        FireballProjectile fireball = collision.gameObject.GetComponent<FireballProjectile>();
        if (fireball != null)
        {
            Debug.Log($"{enemyName} hit by fireball! Taking 1 damage.");
            TakeDamage(1f);
            
            // Destroy the fireball
            Destroy(fireball.gameObject);
            return;
        }
        
        Debug.Log($"No Player or FireballProjectile component found on {collision.gameObject.name}");
    }
    
    // Trigger detection for projectiles (often more reliable than collision)
    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"Enemy OnTriggerEnter2D with: {other.gameObject.name} (Tag: {other.gameObject.tag})");
        
        // Check for FireballProjectile collision
        FireballProjectile fireball = other.gameObject.GetComponent<FireballProjectile>();
        if (fireball != null)
        {
            Debug.Log($"{enemyName} hit by fireball (trigger)! Taking 1 damage.");
            TakeDamage(1f);
            
            // Destroy the fireball
            Destroy(fireball.gameObject);
            return;
        }
        
        Debug.Log($"No FireballProjectile component found on {other.gameObject.name}");
    }
} 