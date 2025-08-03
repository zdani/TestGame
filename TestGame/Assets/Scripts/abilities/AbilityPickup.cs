using UnityEngine;

public class AbilityPickup : MonoBehaviour
{
    public AbilityType abilityType;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out Player player))
        {
            player.LearnAbility(abilityType);
            Destroy(gameObject);
        }
    }
} 