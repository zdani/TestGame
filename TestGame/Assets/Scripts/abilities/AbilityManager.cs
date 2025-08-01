using UnityEngine;

public class AbilityManager : MonoBehaviour
{
    public GameObject fireballPrefab;
    public Transform fireballSpawnPoint;

    public void CastFireball()
    {
        Vector2 shootDirection = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
        GameObject fireball = Instantiate(fireballPrefab, fireballSpawnPoint.position, Quaternion.identity);
        fireball.GetComponent<FireballProjectile>().Initialize(shootDirection);
    }
}
