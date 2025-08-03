using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    [Header("Combat Settings")]
    [SerializeField] private float _invincibilityDuration = 3f;
    
    // Public properties
    public IHealthManager HealthManager { get; private set; }
    public HashSet<AbilityType> Abilities { get; private set; } = new HashSet<AbilityType>();
    public bool IsInvincible { get; private set; } = false;
    [SerializeField] Image _healthBarSprite;

    const int INIT_MAX_HEALTH = 5;
    private float invincibilityTimer = 0f;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        HealthManager = new HealthManager(INIT_MAX_HEALTH, _healthBarSprite, this);
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Start player with Fireball ability unlocked
        Abilities.Add(AbilityType.Fireball);
        
        // Subscribe to health events through GameEvents
        GameEvents.Instance.OnPlayerDied += OnPlayerDied;
    }

    private void Update()
    {
        // Handle invincibility timer
        if (IsInvincible)
        {
            invincibilityTimer -= Time.deltaTime;
            
            // Flash the sprite during invincibility
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = Mathf.Sin(Time.time * 10f) > 0f;
            }
            
            if (invincibilityTimer <= 0f)
            {
                EndInvincibility();
            }
        }
    }

    // Public method for enemies to call when dealing damage
    public void TakeDamage(float damageAmount, string enemyName = "Enemy")
    {
        // Don't take damage if invincible
        if (IsInvincible) return;
        
        // Find the IceShieldAbility component on this player's GameObject
        IceShieldAbility iceShieldAbility = gameObject.GetComponent<IceShieldAbility>();
        if (iceShieldAbility != null && iceShieldAbility.isShieldActive){
            StartInvincibility(1f);
            iceShieldAbility.BreakShield();
            return;
        }
        
        // Take damage
        HealthManager.TakeDamage(damageAmount);
        
        // Start invincibility period
        StartInvincibility(_invincibilityDuration);
        
        // Trigger hit event through GameEvents
        GameEvents.Instance.TriggerPlayerHit();
        
        Debug.Log($"Player took {damageAmount} damage from {enemyName}! Health: {HealthManager.CurrentHealth}/{HealthManager.MaxHealth}");
    }

    public void Heal(float healAmount) =>  HealthManager.Heal(healAmount);

    public void LearnAbility(AbilityType abilityType)
    {
        if (Abilities.Add(abilityType))
        {
            Debug.Log($"Player has learned a new ability: {abilityType}");
            GameEvents.Instance.TriggerAbilityLearned(abilityType);
        }
    }

    private void StartInvincibility(float invincibilityDuration)
    {
        IsInvincible = true;
        invincibilityTimer = invincibilityDuration;
        GameEvents.Instance.TriggerInvincibilityStarted();
        Debug.Log($"Player is now invincible for {invincibilityDuration} seconds");
    }

    private void EndInvincibility()
    {
        IsInvincible = false;
        invincibilityTimer = 0f;
        
        // Restore sprite visibility
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }
        
        GameEvents.Instance.TriggerInvincibilityEnded();
        Debug.Log("Player invincibility ended");
    }

    private void OnPlayerDied()
    {
        // Add end game logic here later
        Debug.Log("Player has died!");
    }

    // Utility methods
    public float GetHealthPercentage()
    {
        return HealthManager.MaxHealth > 0 ? HealthManager.CurrentHealth / HealthManager.MaxHealth : 0f;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check for BossProjectile collision
        BossProjectile bossProjectile = other.gameObject.GetComponent<BossProjectile>();
        if (bossProjectile != null)
        {
            TakeDamage(bossProjectile.damage, "BossWiz");
            Destroy(bossProjectile.gameObject);
        }
    }
}

