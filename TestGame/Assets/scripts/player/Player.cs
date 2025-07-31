using System;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{

    // Public properties
    public IHealthManager HealthManager { get; private set; }
    public List<Ability> Abilities => new List<Ability>();

    // Events
    public event Action<Ability> OnAbilityCast;

    const int INIT_MAX_HEALTH = 100;
    private void Awake()
    {
        HealthManager = new HealthManager(INIT_MAX_HEALTH);
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
        OnAbilityCast?.Invoke(ability);
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

