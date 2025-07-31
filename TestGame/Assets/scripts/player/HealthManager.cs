using System;
using UnityEngine;

// Health manager implementation
public class HealthManager : IHealthManager
{
    public float CurrentHealth { get; private set; }
    public float MaxHealth { get; private set; }
    public bool IsAlive => CurrentHealth > 0f;

    public event Action<float> OnHealthChanged;
    public event Action OnPlayerDied;

    public HealthManager(float maxHealth)
    {
        MaxHealth = maxHealth;
        CurrentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        if (!IsAlive) return;

        float oldHealth = CurrentHealth;
        CurrentHealth = Mathf.Max(0f, CurrentHealth - damage);

        OnHealthChanged?.Invoke(CurrentHealth);

        if (CurrentHealth <= 0f && oldHealth > 0f)
        {
            OnPlayerDied?.Invoke();
        }
    }

    public void Heal(float amount)
    {
        if (!IsAlive) return;

        float oldHealth = CurrentHealth;
        CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + amount);

        if (CurrentHealth != oldHealth)
        {
            OnHealthChanged?.Invoke(CurrentHealth);
        }
    }

    public void SetMaxHealth(float maxHealth)
    {
        MaxHealth = maxHealth;
        CurrentHealth = Mathf.Min(CurrentHealth, MaxHealth);
    }
}
