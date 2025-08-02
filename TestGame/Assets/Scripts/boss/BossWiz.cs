using UnityEngine;
using System.Collections;

public class BossWiz : Enemy
{
    public override float DamageAmount => 1f;

    public enum BossState
    {
        Idle,
        Teleporting,
        Attacking,
        Dead
    }

    [Header("Boss Settings")]
    private Transform[] teleportPoints;
    public GameObject projectilePrefab;
    public Transform projectileSpawnPoint;
    public float attackCooldown = 2f;
    public float idleTime = 1.5f;

    private BossState currentState;
    private float lastAttackTime;
    private Rigidbody2D rb;
    private int lastTeleportIndex = -1;
    private Transform headTarget;

    protected override void Start()
    {
        base.Start();
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = 0;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
        currentState = BossState.Idle;
        lastAttackTime = -attackCooldown;
        
        // Find player's head for targeting
        if (playerTransform != null)
        {
            headTarget = playerTransform.Find("Head");
        }
        // Fallback to player's main transform if head is not found
        if (headTarget == null)
        {
            Debug.LogWarning("BossWiz could not find 'Head' transform on player. Defaulting to player's main transform.");
            headTarget = playerTransform;
        }
        else
        {
            Debug.Log("BossWiz successfully targeted player's 'head' transform.");
        }

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

        StartCoroutine(StateMachine());
    }

    private IEnumerator StateMachine()
    {
        while (currentState != BossState.Dead)
        {
            switch (currentState)
            {
                case BossState.Idle:
                    yield return StartCoroutine(IdleState());
                    break;
                case BossState.Teleporting:
                    yield return StartCoroutine(TeleportState());
                    break;
                case BossState.Attacking:
                    yield return StartCoroutine(AttackState());
                    break;
            }
        }
    }

    private IEnumerator IdleState()
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        // Stay idle for a bit
        yield return new WaitForSeconds(idleTime);

        // Decide next action
        if (playerTransform != null && Vector2.Distance(transform.position, playerTransform.position) > 10f) // Example range
        {
            currentState = BossState.Teleporting;
        }
        else
        {
            currentState = BossState.Attacking;
        }
    }

    private IEnumerator TeleportState()
    {
        if (teleportPoints != null && teleportPoints.Length > 0)
        {
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
        else
        {
            Debug.LogError("BossWiz has no teleport points assigned or could not find 'TeleportationPoints' GameObject in the scene.");
        }
        yield return new WaitForSeconds(0.5f); // Teleport visual effect duration
        currentState = BossState.Attacking;
    }

    private IEnumerator AttackState()
    {
        if (Time.time > lastAttackTime + attackCooldown && headTarget != null)
        {
            lastAttackTime = Time.time;
            Vector2 direction = (headTarget.position - projectileSpawnPoint.position).normalized;
            GameObject projectile = Instantiate(projectilePrefab, projectileSpawnPoint.position, Quaternion.identity);
            projectile.GetComponent<BossProjectile>().Initialize(direction);
        }
        yield return new WaitForSeconds(0.5f); // Wait a bit after attacking
        currentState = BossState.Idle;
    }

    protected override void OnDeath()
    {
        base.OnDeath();
        currentState = BossState.Dead;
        StopAllCoroutines();
        // Add any death effects or logic here
    }

    protected override void OnPlayerCollision(Player player)
    {
        // Boss might not damage on touch, relies on projectiles
    }
}
