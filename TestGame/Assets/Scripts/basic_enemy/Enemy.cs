using UnityEngine;

public abstract class Enemy : MonoBehaviour
{
    [Header("Enemy Settings")]
    [SerializeField] protected string enemyName = "Enemy";
    
    // Public properties
    public string EnemyName => enemyName;
    public abstract float DamageAmount { get; }
    
    protected virtual void Start()
    {
        // Base initialization - can be overridden by derived classes
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
        Player player = collision.gameObject.GetComponent<Player>();
        if (player != null)
        {
            Debug.Log($"Found Player component on {collision.gameObject.name}, calling OnPlayerCollision");
            OnPlayerCollision(player);
            DealDamageToPlayer(player);
        }
        else
        {
            Debug.Log($"No Player component found on {collision.gameObject.name}");
        }
    }
} 