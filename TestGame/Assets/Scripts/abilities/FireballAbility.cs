using UnityEngine;

public class FireballAbility : MonoBehaviour
{
    public GameObject fireballPrefab;
    public Transform fireballSpawnPoint;

    public void Cast()
    {
        Vector2 shootDirection = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
        GameObject fireball = Instantiate(fireballPrefab, fireballSpawnPoint.position, Quaternion.identity);
        fireball.GetComponent<FireballProjectile>().Initialize(shootDirection);
    }
}
