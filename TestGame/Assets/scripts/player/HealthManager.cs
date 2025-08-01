using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Health manager implementation
public class HealthManager : IHealthManager
{
    public float CurrentHealth { get; private set; }
    public float MaxHealth { get; private set; }
    public bool IsAlive => CurrentHealth > 0f;
    private Image _healthBarSprite;

    public HealthManager(float maxHealth, Image healthBarSprite)
    {
        MaxHealth = maxHealth;
        CurrentHealth = maxHealth;
        _healthBarSprite = healthBarSprite;
        GameEvents.Instance.OnHealthChanged += OnHealthChanged;
    }

    private void OnHealthChanged(float obj)
    {
        _healthBarSprite.fillAmount = CurrentHealth / MaxHealth;
    }

    public void TakeDamage(float damage)
    {
        if (!IsAlive) return;

        float oldHealth = CurrentHealth;
        CurrentHealth = Mathf.Max(0f, CurrentHealth - damage);

        // Trigger health changed event through GameEvents
        GameEvents.Instance.TriggerHealthChanged(CurrentHealth);

        if (CurrentHealth <= 0f && oldHealth > 0f)
        {
            // Trigger player died event through GameEvents
            GameEvents.Instance?.TriggerPlayerDied();
        }
    }

    public void Heal(float amount)
    {
        if (!IsAlive) return;

        float oldHealth = CurrentHealth;
        CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + amount);

        if (CurrentHealth != oldHealth)
        {
            // Trigger health changed event through GameEvents
            GameEvents.Instance.TriggerHealthChanged(CurrentHealth);
        }
    }

    public void SetMaxHealth(float maxHealth)
    {
        MaxHealth = maxHealth;
        CurrentHealth = Mathf.Min(CurrentHealth, MaxHealth);
    }
}
