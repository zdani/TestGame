using UnityEngine;
using System.Collections.Generic;

public class AbilityManager : MonoBehaviour
{
    [Header("Ability Prefabs")]
    public GameObject fireballPrefab;
    public Transform fireballSpawnPoint;
    
    [Header("Available Abilities")]
    [SerializeField] private List<IAbility> availableAbilities = new List<IAbility>();
    
    private void Awake()
    {
        // Initialize abilities
        InitializeAbilities();
    }
    
    private void InitializeAbilities()
    {
        // Create and add abilities to the list
        availableAbilities.Add(new FireballAbility(fireballPrefab, fireballSpawnPoint));
        // Add more abilities here as needed
    }
    
    public bool CastAbility(IAbility ability)
    {
        if (ability != null && availableAbilities.Contains(ability))
        {
            Player player = GetComponent<Player>();
            if (player != null)
            {
                ability.Cast(player);
                return true;
            }
        }
        
        Debug.LogWarning($"Cannot cast ability: {ability?.GetType().Name ?? "null"}");
        return false;
    }
    
    // Getter for available abilities
    public List<IAbility> GetAvailableAbilities()
    {
        return new List<IAbility>(availableAbilities);
    }
    
    // Legacy method for backward compatibility
    public void CastFireball()
    {
        IAbility fireballAbility = availableAbilities.Find(a => a is FireballAbility);
        if (fireballAbility != null)
        {
            CastAbility(fireballAbility);
        }
    }
}
