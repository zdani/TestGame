using UnityEngine;

public class FireballAbility : IAbility
{
    private GameObject fireballPrefab;
    private Transform spawnPoint;
    
    public string AbilityName => "Fireball";
    
    public FireballAbility(GameObject fireballPrefab, Transform spawnPoint)
    {
        this.fireballPrefab = fireballPrefab;
        this.spawnPoint = spawnPoint;
    }
    
    public void Cast(Player caster)
    {
        if (fireballPrefab == null || spawnPoint == null)
        {
            Debug.LogError("FireballAbility: Missing prefab or spawn point!");
            return;
        }
        
        // Get direction the player is facing
        Vector2 shootDirection = caster.transform.localScale.x > 0 ? Vector2.right : Vector2.left;
        
        // Instantiate the fireball
        GameObject fireball = Object.Instantiate(fireballPrefab, spawnPoint.position, Quaternion.identity);
        
        // Initialize the fireball projectile
        FireballProjectile projectile = fireball.GetComponent<FireballProjectile>();
        if (projectile != null)
        {
            projectile.Initialize(shootDirection);
        }
        
        Debug.Log($"Player cast {AbilityName}!");
    }
} 