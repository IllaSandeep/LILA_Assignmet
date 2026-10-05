using UnityEngine;

public class FlankerEnemyController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2.2f;
    [SerializeField] private float acceleration = 6f;

    [Header("Flanking")]
    [UnityEngine.Serialization.FormerlySerializedAs("preferredDistance")]
    [SerializeField] private float flankDistance = 3f;
    [SerializeField] private float flankAngle = 60f;
    [SerializeField] private float predictionTime = 0.8f;
    [SerializeField] private float predictionJitter = 0.65f;
    [SerializeField] private float repositionCooldown = 2f;

    [Header("Attack")]
    [SerializeField] private float attackRange = 0.8f;
    [SerializeField] private float attackDamage = 6f;
    [SerializeField] private float attackCooldown = 0.7f;

    private Transform player;
    private PlayerHealth playerHealth;

    private float attackTimer;

    // Randomly chosen side for this flanker.
    private float flankDirection;
    private float repositionTimer;
    private float currentSpeed;
    private float currentPredictionNoise;
    private float baseMoveSpeed;
    private float baseAttackDamage;
    private Vector2 previousPlayerPosition;
    private Vector2 playerVelocity;

    private EnemyHealth enemyHealth;

    private void Awake()
    {
        enemyHealth =
            GetComponent<EnemyHealth>();

        // Randomly choose left or right
        // side for flanking.
        flankDirection =
            Random.value < 0.5f
                ? -1f
                : 1f;
        currentPredictionNoise = Random.Range(-predictionJitter, predictionJitter) * 30f;
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
            previousPlayerPosition = player.position;
        }
    }

    private void Update()
    {
        if (player == null)
            return;

        // Stop completely if this enemy is dead.
        if (
            enemyHealth != null &&
            enemyHealth.IsDead()
        )
        {
            return;
        }

        attackTimer -=
            Time.deltaTime;

        currentSpeed = Mathf.MoveTowards(currentSpeed, moveSpeed, acceleration * Time.deltaTime);
        Vector2 currentPlayerPosition = player.position;
        playerVelocity = (currentPlayerPosition - previousPlayerPosition) / Mathf.Max(Time.deltaTime, 0.001f);
        previousPlayerPosition = currentPlayerPosition;

        float distance =
            Vector2.Distance(
                transform.position,
                player.position
            );

        // ------------------------------------------
        // ATTACK
        // ------------------------------------------

        if (distance <= attackRange)
        {
            if (attackTimer <= 0f)
            {
                Attack();

                attackTimer =
                    attackCooldown;
            }

            return;
        }

        // ------------------------------------------
        // Predict a short, deliberately noisy intercept point.
        // ------------------------------------------

        repositionTimer -= Time.deltaTime;
        if (repositionTimer <= 0f)
        {
            flankDirection *= -1f;
            repositionTimer = repositionCooldown;
            currentPredictionNoise = Random.Range(-predictionJitter, predictionJitter) * 30f;
        }

        Vector2 predictedPosition = currentPlayerPosition + playerVelocity * predictionTime;
        Vector2 fromPlayer = ((Vector2)transform.position - currentPlayerPosition).normalized;
        if (fromPlayer.sqrMagnitude < 0.01f)
            fromPlayer = Vector2.right;
        float angle = flankAngle * flankDirection + currentPredictionNoise;
        float radians = angle * Mathf.Deg2Rad;
        Vector2 offset = new Vector2(
            fromPlayer.x * Mathf.Cos(radians) - fromPlayer.y * Mathf.Sin(radians),
            fromPlayer.x * Mathf.Sin(radians) + fromPlayer.y * Mathf.Cos(radians)
        ) * flankDistance;
        Vector2 intercept = predictedPosition + offset;
        Vector2 movementDirection = (intercept - (Vector2)transform.position).normalized;
        transform.position += (Vector3)(movementDirection * currentSpeed * Time.deltaTime);
    }

    // ----------------------------------------------
    // ATTACK
    // ----------------------------------------------

    private void Attack()
    {
        if (playerHealth == null)
            return;

        playerHealth.TakeDamage(    
            attackDamage
        );

        Debug.Log(
            "FLANKER ATTACKED PLAYER! Damage: " +
            attackDamage
        );
    }

    public void ApplyDifficultyMultipliers(float speedMultiplier, float damageMultiplier)
    {
        moveSpeed = baseMoveSpeed * Mathf.Max(0f, speedMultiplier);
        attackDamage = baseAttackDamage * Mathf.Max(0f, damageMultiplier);
    }
}
