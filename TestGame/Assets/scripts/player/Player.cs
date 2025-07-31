using System;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Combat Settings")]
    [SerializeField] private float invincibilityDuration = 3f;
    
    // Public properties
    public IHealthManager HealthManager { get; private set; }
    public List<Ability> Abilities => new List<Ability>();
    public bool IsInvincible { get; private set; } = false;

    const int INIT_MAX_HEALTH = 100;
    private float invincibilityTimer = 0f;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        HealthManager = new HealthManager(INIT_MAX_HEALTH);
        spriteRenderer = GetComponent<SpriteRenderer>();
        
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
        
        // Take damage
        HealthManager.TakeDamage(damageAmount);
        
        // Start invincibility period
        StartInvincibility();
        
        // Trigger hit event through GameEvents
        GameEvents.Instance.TriggerPlayerHit();
        
        Debug.Log($"Player took {damageAmount} damage from {enemyName}! Health: {HealthManager.CurrentHealth}/{HealthManager.MaxHealth}");
    }

    private void StartInvincibility()
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

    // Public methods for casting abilities
    public bool CastAbility(Ability ability)
    {
        if (!Abilities.Contains(ability))
        {
            Debug.LogWarning($"Ability '{ability.AbilityName}' is not in the available abilities list");
            return false;
        }

        ability.Cast(this);
        GameEvents.Instance.TriggerAbilityCast(ability);
        return true;
    }

    // Methods to add/remove abilities
    public void AddAbility(Ability ability)
    {
        if (!Abilities.Contains(ability))
        {
            Abilities.Add(ability);
        }
    }

    public void RemoveAbility(Ability ability)
    {
        Abilities.Remove(ability);
    }

    // Utility methods
    public float GetHealthPercentage()
    {
        return HealthManager.MaxHealth > 0 ? HealthManager.CurrentHealth / HealthManager.MaxHealth : 0f;
    }
}

