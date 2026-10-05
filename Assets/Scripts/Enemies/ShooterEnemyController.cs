using UnityEngine;

public class ShooterEnemyController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 1.5f;
    [SerializeField] private float preferredDistance = 5f;
    [SerializeField] private float distanceTolerance = 0.8f;
    [SerializeField] private float retreatDistance = 3.5f;
    [SerializeField] private float repositionCooldown = 3f;
    [SerializeField] private float repositionDuration = 0.55f;

    [Header("Attack")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private float projectileSpeedMultiplier = 1f;
    [SerializeField] private float projectileDamageMultiplier = 1f;

    private Transform player;

    private float attackTimer;

    private Rigidbody2D rb;
    private float baseMoveSpeed;
    private float baseAttackCooldown;
    private float repositionTimer;
    private float repositionRemaining;
    private float repositionSide = 1f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        baseMoveSpeed = moveSpeed;
        baseAttackCooldown = attackCooldown;
    }

    private void Start()
    {
        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
        }
    }

    private void Update()
    {
        attackTimer -= Time.deltaTime;

        if (player == null)
            return;

        if (attackTimer <= 0f)
        {
            Shoot();
            attackTimer = attackCooldown;
        }
    }

    private void FixedUpdate()
    {
        if (player == null)
            return;

        float distance =
            Vector2.Distance(
                transform.position,
                player.position
            );

        if (distance > preferredDistance + distanceTolerance)
        {
            MoveTowardPlayer();
        }
        else if (distance < retreatDistance)
        {
            MoveAwayFromPlayer();
        }
        else if (repositionRemaining > 0f)
        {
            Vector2 tangent = Vector2.Perpendicular((Vector2)player.position - rb.position).normalized * repositionSide;
            rb.MovePosition(rb.position + tangent * moveSpeed * Time.fixedDeltaTime);
        }
        else
        {
            StopMoving();
        }

        if (repositionTimer > 0f)
            repositionTimer -= Time.fixedDeltaTime;
        else
        {
            repositionSide *= -1f;
            repositionRemaining = repositionDuration;
            repositionTimer = repositionCooldown;
        }
        repositionRemaining = Mathf.Max(0f, repositionRemaining - Time.fixedDeltaTime);
    }

    private void MoveTowardPlayer()
    {
        Vector2 direction =
            ((Vector2)player.position - rb.position)
            .normalized;

        rb.MovePosition(
            rb.position +
            direction * moveSpeed *
            Time.fixedDeltaTime
        );
    }

    private void MoveAwayFromPlayer()
    {
        Vector2 direction =
            (rb.position - (Vector2)player.position)
            .normalized;

        rb.MovePosition(
            rb.position +
            direction * moveSpeed *
            Time.fixedDeltaTime
        );
    }

    private void StopMoving()
    {
        rb.linearVelocity = Vector2.zero;
    }

    private void Shoot()
    {
        if (projectilePrefab == null)
            return;

        Vector2 direction =
            ((Vector2)player.position -
             (Vector2)transform.position)
            .normalized;

        GameObject projectile =
            Instantiate(
                projectilePrefab,
                transform.position,
                Quaternion.identity
            );

        EnemyProjectile projectileScript =
            projectile.GetComponent<EnemyProjectile>();

        if (projectileScript != null)
        {
            projectileScript.Initialize(direction, projectileSpeedMultiplier, projectileDamageMultiplier);
        }
    }

    public void ApplyDifficultyMultipliers(float speedMultiplier, float damageMultiplier)
    {
        moveSpeed = baseMoveSpeed * Mathf.Max(0f, speedMultiplier);
        projectileDamageMultiplier *= Mathf.Max(0f, damageMultiplier);
    }
}
