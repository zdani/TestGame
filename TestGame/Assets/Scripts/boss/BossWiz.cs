using UnityEngine;
using System.Collections;

public class BossWiz : Enemy
{
    public override float DamageAmount => 0f;

    public enum BossState
    {
        Idle,
        Teleporting,
        Attacking,
        Dead
    }

    [Header("Boss Settings")]
    public GameObject projectilePrefab;
    public Transform projectileSpawnPoint;
    public float teleportRange = 10f;
    public float attackCooldown = 2f;
    public float idleTime = 1.5f;

    private BossState currentState;
    private float lastAttackTime;

    protected override void Start()
    {
        base.Start();
        currentState = BossState.Idle;
        lastAttackTime = -attackCooldown;
        playerTransform = FindObjectOfType<Player>()?.transform;
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
        // Stay idle for a bit
        yield return new WaitForSeconds(idleTime);

        // Decide next action
        if (playerTransform != null && Vector2.Distance(transform.position, playerTransform.position) > teleportRange / 2)
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
        if (playerTransform != null)
        {
            Vector2 teleportPosition = (Vector2)playerTransform.position + Random.insideUnitCircle * teleportRange;
            transform.position = teleportPosition;
        }
        yield return new WaitForSeconds(0.5f); // Teleport visual effect duration
        currentState = BossState.Attacking;
    }

    private IEnumerator AttackState()
    {
        if (Time.time > lastAttackTime + attackCooldown && playerTransform != null)
        {
            lastAttackTime = Time.time;
            Vector2 direction = (playerTransform.position - projectileSpawnPoint.position).normalized;
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
