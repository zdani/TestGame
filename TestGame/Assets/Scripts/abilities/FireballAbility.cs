using UnityEngine;

public class FireballAbility : Ability
{
    [Header("Fireball Settings")]
    [SerializeField] private GameObject fireballPrefab;
    [SerializeField] private float fireballSpeed = 10f;
    [SerializeField] private float fireballDamage = 25f;
    [SerializeField] private float fireballLifetime = 3f;
    [SerializeField] private Transform firePoint; // Point where fireball spawns

    [Header("Cooldown Settings")]
    [SerializeField] private float cooldownTime = 2f;
    private float lastCastTime = 0f;

    public FireballAbility() : base("Fireball")
    {
        // Constructor calls base constructor with ability name
    }

    protected override void OnCast(Player caster)
    {
        // Check cooldown
        if (Time.time - lastCastTime < cooldownTime)
        {
            Debug.Log($"Fireball is on cooldown! {cooldownTime - (Time.time - lastCastTime):F1}s remaining");
            return;
        }

        // Check if we have a fireball prefab
        if (fireballPrefab == null)
        {
            Debug.LogError("FireballAbility: No fireball prefab assigned!");
            return;
        }

        // Get spawn position
        Vector3 spawnPosition = firePoint != null ? firePoint.position : caster.transform.position;

        // Create fireball
        GameObject fireball = Object.Instantiate(fireballPrefab, spawnPosition, Quaternion.identity);

        // Set up fireball behavior
        SetupFireball(fireball, caster);

        // Update cooldown
        lastCastTime = Time.time;

        Debug.Log($"{caster.name} cast Fireball!");
    }

    private void SetupFireball(GameObject fireball, Player caster)
    {
        // Add Rigidbody2D if it doesn't exist
        if (!fireball.TryGetComponent<Rigidbody2D>(out var rb))
        {
            rb = fireball.AddComponent<Rigidbody2D>();
        }

        // Set up physics
        rb.gravityScale = 0f; // Fireballs don't fall
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        // Determine direction (assume player faces right by default, or use input)
        Vector2 direction = Vector2.right; // Default direction
        if (caster.transform.localScale.x < 0) // Player facing left
        {
            direction = Vector2.left;
        }

        // Set velocity
        rb.linearVelocity = direction * fireballSpeed;

        // Add FireballProjectile component for damage and lifetime
        if (!fireball.TryGetComponent<FireballProjectile>(out var projectile))
        {
            projectile = fireball.AddComponent<FireballProjectile>();
        }

        projectile.Initialize(fireballDamage, fireballLifetime, caster);

        // Destroy fireball after lifetime
        Object.Destroy(fireball, fireballLifetime);
    }

    // Public method to check if ability is on cooldown
    public bool IsOnCooldown()
    {
        return Time.time - lastCastTime < cooldownTime;
    }

    // Public method to get remaining cooldown time
    public float GetRemainingCooldown()
    {
        float remaining = cooldownTime - (Time.time - lastCastTime);
        return remaining > 0 ? remaining : 0f;
    }
}

// Component that handles fireball collision and damage
public class FireballProjectile : MonoBehaviour
{
    private float damage;
    private float lifetime;
    private Player owner;
    private bool hasHit = false;
    
    public void Initialize(float damageAmount, float projectileLifetime, Player ownerPlayer)
    {
        damage = damageAmount;
        lifetime = projectileLifetime;
        owner = ownerPlayer;
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Don't hit the owner
        if (other.gameObject == owner.gameObject) return;
        
        // Don't hit multiple times
        if (hasHit) return;
        
        /* Check if we hit an enemy (not implemented yet)
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null)
        {
            // Deal damage to enemy (you'll need to implement TakeDamage on Enemy)
            Debug.Log($"Fireball hit {enemy.EnemyName} for {damage} damage!");
            
            // Mark as hit to prevent multiple hits
            hasHit = true;
            
            // Destroy the fireball
            Destroy(gameObject);
        }
        */
        // Check if we hit a wall/obstacle
        if (other.CompareTag("Ground") || other.CompareTag("Wall"))
        {
            Debug.Log("Fireball hit a wall!");
            hasHit = true;
            Destroy(gameObject);
        }
    }
} 
