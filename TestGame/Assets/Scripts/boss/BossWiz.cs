using UnityEngine;
using System.Collections;

public class BossWiz : Enemy
{
    public override float DamageAmount => 1f;

    [Header("Boss Settings")]
    private Transform[] teleportPoints;
    public GameObject projectilePrefab;
    public Transform projectileSpawnPoint;

    private Rigidbody2D rb;
    private int lastTeleportIndex = -1;
    private Transform headTarget;
    private bool isDead = false;

    protected override void Start()
    {
        base.Start();
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = 0;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
        
        headTarget = playerTransform.Find("Head");

        // Find teleport points
        GameObject teleportParent = GameObject.Find("TeleportationPoints");
        if (teleportParent != null)
        {
            int pointCount = teleportParent.transform.childCount;
            teleportPoints = new Transform[pointCount];
            for (int i = 0; i < pointCount; i++)
            {
                teleportPoints[i] = teleportParent.transform.GetChild(i);
            }
            Debug.Log($"BossWiz loaded with {pointCount} teleport points.");
        }
        else
        {
            Debug.LogWarning("BossWiz could not find a 'TeleportationPoints' parent object. Teleportation will be disabled.");
            teleportPoints = new Transform[0]; // Initialize to prevent null reference errors
        }

        StartCoroutine(BehaviorCycle());
    }

    private IEnumerator BehaviorCycle()
    {
        // Initial delay before the first action
        yield return new WaitForSeconds(1f);

        while (!isDead)
        {
            PerformTeleport();
            
            yield return new WaitForSeconds(3f);
            PerformAttack();

            yield return new WaitForSeconds(3f);
            PerformAttack();

            yield return new WaitForSeconds(3f);
        }
    }

    private void PerformTeleport()
    {
        if (teleportPoints == null || teleportPoints.Length == 0)
        {
            return; // Can't teleport if there are no points
        }

        int nextTeleportIndex = lastTeleportIndex;
        if (teleportPoints.Length > 1)
        {
            while (nextTeleportIndex == lastTeleportIndex)
            {
                nextTeleportIndex = Random.Range(0, teleportPoints.Length);
            }
        }
        else
        {
            nextTeleportIndex = 0;
        }
            
        lastTeleportIndex = nextTeleportIndex;
        transform.position = teleportPoints[nextTeleportIndex].position;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    private void PerformAttack()
    {
        if (headTarget != null)
        {
            Vector2 direction = (headTarget.position - projectileSpawnPoint.position).normalized;
            GameObject projectile = Instantiate(projectilePrefab, projectileSpawnPoint.position, Quaternion.identity);
            projectile.GetComponent<BossProjectile>().Initialize(direction);
        }
    }

    protected override void OnDeath()
    {
        base.OnDeath();
        isDead = true;
        StopAllCoroutines();
        // Add any death effects or logic here
    }

    protected override void OnPlayerCollision(Player player)
    {
        // Boss might not damage on touch, relies on projectiles
    }
}
