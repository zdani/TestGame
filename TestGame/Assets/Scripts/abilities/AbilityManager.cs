using UnityEngine;

public class AbilityManager : MonoBehaviour
{
    public GameObject fireballPrefab;
    public Transform fireballSpawnPoint;

    public void CastFireball()
    {
        // Get direction the player is facing
        Vector2 shootDirection = transform.localScale.x > 0 ? Vector2.right : Vector2.left;

        // Instantiate without rotation — we’ll handle that in code
        GameObject fireball = Instantiate(fireballPrefab, fireballSpawnPoint.position, Quaternion.identity);

        fireball.GetComponent<FireballProjectile>().Initialize(shootDirection);
    }
}
