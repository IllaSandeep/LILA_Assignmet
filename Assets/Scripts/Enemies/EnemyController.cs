using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float acceleration = 8f;
    [SerializeField] private float minimumSeparationDistance = 0.55f;

    [Header("Attack")]
    [SerializeField] private float attackRange = 0.8f;
    [SerializeField] private float attackDamage = 8f;
    [SerializeField] private float attackCooldown = 0.6f;

    private Transform player;
    private PlayerHealth playerHealth;

    private float attackTimer;
    private float currentSpeed;
    private float baseMoveSpeed;
    private float baseAttackDamage;

    private void Awake()
    {
        baseMoveSpeed = moveSpeed;
        baseAttackDamage = attackDamage;
    }

    private void Start()
    {
        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player =
                playerObject.transform;

            playerHealth =
                playerObject.GetComponent<PlayerHealth>();
        }
    }

    private void Update()
    {
        if (player == null)
            return;

        attackTimer -=
            Time.deltaTime;
        currentSpeed = Mathf.MoveTowards(currentSpeed, moveSpeed, acceleration * Time.deltaTime);

        float distance =
            Vector2.Distance(
                transform.position,
                player.position
            );

        // ------------------------------------------
        // MOVE TOWARD PLAYER
        // ------------------------------------------

        if (distance > attackRange || distance < minimumSeparationDistance)
        {
            Vector2 direction =
                distance < minimumSeparationDistance
                ? ((Vector2)transform.position - (Vector2)player.position).normalized
                : (
                    (Vector2)player.position -
                    (Vector2)transform.position
                ).normalized;

            transform.position +=
                (Vector3)(
                    direction *
                    currentSpeed *
                    Time.deltaTime
                );
        }

        // ------------------------------------------
        // ATTACK PLAYER
        // ------------------------------------------

        if (distance <= attackRange && attackTimer <= 0f)
        {
            Attack();

            attackTimer =
                attackCooldown;
        }
    }

    public void ApplyDifficultyMultipliers(float speedMultiplier, float damageMultiplier)
    {
        moveSpeed = baseMoveSpeed * Mathf.Max(0f, speedMultiplier);
        attackDamage = baseAttackDamage * Mathf.Max(0f, damageMultiplier);
    }

    private void Attack()
    {
        if (playerHealth == null)
            return;

        playerHealth.TakeDamage(
            attackDamage
        );

        Debug.Log(
            "CHASER ATTACKED PLAYER! Damage: " +
            attackDamage
        );
    }
}
