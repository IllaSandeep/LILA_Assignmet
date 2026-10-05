using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 30f;

    [Header("XP")]
    [SerializeField] private GameObject xpOrbPrefab;

    [Header("Health Drop")]
    [SerializeField] private GameObject healthPickupPrefab;
    [SerializeField] private float healthDropChance = 0.10f;

    [Header("Death Animation")]
    [SerializeField] private float deathAnimationDuration = 0.5f;

    private float currentHealth;
    private float baseMaxHealth;

    private RunManager runManager;
    private Animator animator;

    private bool isDead;

    // --------------------------------------------------
    // AWAKE
    // --------------------------------------------------

    private void Awake()
    {
        baseMaxHealth = maxHealth;
        currentHealth =
            maxHealth;

        animator =
            GetComponentInChildren<Animator>();
    }

    // --------------------------------------------------
    // START
    // --------------------------------------------------

    private void Start()
    {
        runManager =
            FindFirstObjectByType<RunManager>();
    }

    // --------------------------------------------------
    // TAKE DAMAGE
    // --------------------------------------------------

    public void TakeDamage(float damage)
    {
        // Do nothing if already dead.
        if (isDead)
            return;

        // Ignore invalid damage.
        if (damage <= 0f)
            return;

        currentHealth -=
            damage;

        currentHealth =
            Mathf.Clamp(
                currentHealth,
                0f,
                maxHealth
            );

        Debug.Log(
            "Enemy HP: " +
            currentHealth +
            " / " +
            maxHealth
        );

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    // --------------------------------------------------
    // DEATH
    // --------------------------------------------------

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;

        Debug.Log(
            "ENEMY DIED"
        );

        // ----------------------------------------------
        // REGISTER KILL
        // ----------------------------------------------

        if (runManager != null)
        {
            runManager.RegisterKill();
        }

        PlayerBehaviorTracker behaviorTracker =
            PlayerBehaviorTracker.ActiveTracker;
        if (behaviorTracker != null)
            behaviorTracker.RegisterKill();

        // ----------------------------------------------
        // SPAWN XP
        // ----------------------------------------------

        SpawnXP();

        // ----------------------------------------------
        // RANDOM HEALTH DROP
        // ----------------------------------------------

        TrySpawnHealthPickup();

        // ----------------------------------------------
        // PLAY DEATH ANIMATION
        // ----------------------------------------------

        if (animator != null)
        {
            animator.SetTrigger(
                "Death"
            );
        }
        else
        {
            Debug.LogWarning(
                "EnemyHealth: Animator not found."
            );
        }

        // ----------------------------------------------
        // DISABLE COLLIDERS
        // ----------------------------------------------

        Collider2D[] colliders =
            GetComponentsInChildren<Collider2D>();

        foreach (
            Collider2D collider
            in colliders
        )
        {
            collider.enabled = false;
        }

        // ----------------------------------------------
        // STOP RIGIDBODY
        // ----------------------------------------------

        Rigidbody2D rb =
            GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.linearVelocity =
                Vector2.zero;

            rb.angularVelocity =
                0f;

            rb.simulated =
                false;
        }

        // ----------------------------------------------
        // DESTROY AFTER DEATH ANIMATION
        // ----------------------------------------------

        Destroy(
            gameObject,
            deathAnimationDuration
        );
    }

    // --------------------------------------------------
    // SPAWN XP
    // --------------------------------------------------

    private void SpawnXP()
    {
        if (xpOrbPrefab == null)
        {
            Debug.LogWarning(
                "EnemyHealth: XP Orb Prefab is missing."
            );

            return;
        }

        GameObject orb =
            Instantiate(
                xpOrbPrefab,
                transform.position,
                Quaternion.identity
            );

        XPOrb xp =
            orb.GetComponent<XPOrb>();

        if (
            xp != null &&
            runManager != null
        )
        {
            xp.SetBonusXP(
                runManager.GetStreakXPBonus()
            );
        }
    }

    // --------------------------------------------------
    // RANDOM HEALTH PICKUP
    // --------------------------------------------------

    private void TrySpawnHealthPickup()
    {
        if (healthPickupPrefab == null)
            return;

        float roll =
            Random.value;

        if (
            roll >
            healthDropChance
        )
        {
            return;
        }

        Instantiate(
            healthPickupPrefab,
            transform.position,
            Quaternion.identity
        );

        Debug.Log(
            "HEALTH PICKUP DROPPED!"
        );
    }

    // --------------------------------------------------
    // GETTERS
    // --------------------------------------------------

    public float GetCurrentHealth()
    {
        return currentHealth;
    }

    public void ApplyDifficultyMultiplier(float healthMultiplier)
    {
        float healthRatio = maxHealth > 0f ? currentHealth / maxHealth : 1f;
        maxHealth = baseMaxHealth * Mathf.Max(0f, healthMultiplier);
        currentHealth = maxHealth * healthRatio;
    }

    public float GetMaxHealth()
    {
        return maxHealth;
    }

    public bool IsDead()
    {
        return isDead;
    }
}
