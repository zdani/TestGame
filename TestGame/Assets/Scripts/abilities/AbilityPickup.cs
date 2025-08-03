using UnityEngine;

public class AbilityPickup : MonoBehaviour
{
    public AbilityType abilityType;
    public GameObject chestUnlockPrefab;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out Player player))
        {
            player.LearnAbility(abilityType);
            player.Heal(2f);
            if (chestUnlockPrefab != null)
            {
                Instantiate(chestUnlockPrefab, transform.position, transform.rotation);
            }
            
            Destroy(gameObject);
        }
    }
} 