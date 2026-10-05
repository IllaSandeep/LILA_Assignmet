using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;

    [Header("Lives")]
    [SerializeField] private int maxLives = 3;

    [Header("Respawn")]
    [SerializeField] private float respawnDelay = 0f;
    [SerializeField] private float minEnemyDistance = 8f;
    [SerializeField] private float respawnSearchRadius = 14f;
    [SerializeField] private int respawnSearchAttempts = 30;
    [SerializeField] private float respawnInvulnerabilityDuration = 2.5f;

    [Header("Shield")]
    [SerializeField] private GameObject shieldVisual;

    [Header("UI")]
    [SerializeField] private Slider healthBar;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private PlayerLivesUI livesUI;

    private float currentHealth;
    private int currentLives;
    private float invulnerabilityTimer;
    private bool shieldActive;

    private Rigidbody2D rb;
    private PlayerController playerController;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public int CurrentLives => currentLives;
    public int MaxLives => maxLives;
    public bool IsInvulnerable =>
        invulnerabilityTimer > 0f;

    public bool IsShieldActive =>
        shieldActive;

    private void Awake()
    {
        currentLives = maxLives;

        rb =
            GetComponent<Rigidbody2D>();

        playerController =
            GetComponent<PlayerController>();

        // Apply permanent Max Health upgrade.
        PermanentUpgradeManager upgradeManager =
            FindFirstObjectByType<PermanentUpgradeManager>();

        if (upgradeManager != null)
        {
            maxHealth *=
                upgradeManager.GetMaxHealthMultiplier();

            Debug.Log(
                "PERMANENT MAX HEALTH APPLIED: " +
                maxHealth
            );
        }

        currentHealth =
            maxHealth;

        if (shieldVisual != null)
        {
            shieldVisual.SetActive(false);
        }
    }

    private void Start()
    {
        UpdateHealthUI();
        UpdateLivesUI();
    }

    private void Update()
    {
        if (invulnerabilityTimer > 0f)
        {
            invulnerabilityTimer -=
                Time.deltaTime;

            if (invulnerabilityTimer < 0f)
            {
                invulnerabilityTimer = 0f;
            }
        }
    }

    public void TakeDamage(float damage)
    {
        if (damage <= 0f)
            return;

        if (currentHealth <= 0f)
            return;

        // Shield completely blocks damage.
        if (shieldActive)
        {
            Debug.Log(
                "SHIELD BLOCKED DAMAGE: " +
                damage
            );

            return;
        }

        // Respawn protection.
        if (IsInvulnerable)
        {
            Debug.Log(
                "PLAYER INVULNERABLE - DAMAGE IGNORED"
            );

            return;
        }

        currentHealth -=
            damage;

        currentHealth =
            Mathf.Clamp(
                currentHealth,
                0f,
                maxHealth
            );

        Debug.Log(
            "Player HP: " +
            currentHealth +
            " / " +
            maxHealth
        );

        UpdateHealthUI();

        if (currentHealth <= 0f)
        {
            LoseLife();
        }
    }

    public void ActivateShield()
    {
        shieldActive = true;

        if (shieldVisual != null)
        {
            shieldVisual.SetActive(true);
        }

        Debug.Log(
            "===== SHIELD ACTIVATED ====="
        );
    }

    public void DeactivateShield()
    {
        shieldActive = false;

        if (shieldVisual != null)
        {
            shieldVisual.SetActive(false);
        }

        Debug.Log(
            "===== SHIELD EXPIRED ====="
        );
    }

    private void LoseLife()
    {
        currentLives--;

        Debug.Log(
            "PLAYER LOST A LIFE! " +
            "Lives remaining: " +
            currentLives
        );

        DeactivateShield();

        UpdateLivesUI();

        if (currentLives <= 0)
        {
            Die();
            return;
        }

        StartCoroutine(RespawnAfterLifeLoss());
    }

    private IEnumerator RespawnAfterLifeLoss()
    {
        if (respawnDelay > 0f)
            yield return new WaitForSeconds(respawnDelay);

        currentHealth =
            maxHealth;

        ResetPlayerVelocity();

        Vector2 respawnPosition =
            FindSafeRespawnPosition();

        MovePlayerTo(
            respawnPosition
        );

        invulnerabilityTimer =
            respawnInvulnerabilityDuration;

        UpdateHealthUI();

        Debug.Log(
            "PLAYER RESPAWNED\n" +
            "Position: " +
            respawnPosition +
            "\nInvulnerable for: " +
            respawnInvulnerabilityDuration +
            "s"
        );
    }

    private Vector2 FindSafeRespawnPosition()
    {
        GameObject[] enemies =
            GameObject.FindGameObjectsWithTag(
                "Enemy"
            );

        if (enemies.Length == 0)
        {
            return transform.position;
        }

        Vector2 originalPosition =
            transform.position;

        Vector2 bestPosition =
            originalPosition;

        float bestDistance = 0f;

        for (
            int i = 0;
            i < respawnSearchAttempts;
            i++
        )
        {
            Vector2 randomDirection =
                Random.insideUnitCircle.normalized;

            if (
                randomDirection.sqrMagnitude <
                0.01f
            )
            {
                randomDirection =
                    Vector2.right;
            }

            float randomDistance =
                Random.Range(
                    minEnemyDistance,
                    respawnSearchRadius
                );

            Vector2 candidate =
                originalPosition +
                randomDirection *
                randomDistance;

            float closestEnemyDistance =
                GetClosestEnemyDistance(
                    candidate,
                    enemies
                );

            if (
                closestEnemyDistance >=
                minEnemyDistance
            )
            {
                Debug.Log(
                    "SAFE RESPAWN POSITION FOUND: " +
                    candidate +
                    " | Closest Enemy: " +
                    closestEnemyDistance.ToString("0.0")
                );

                return candidate;
            }

            if (
                closestEnemyDistance >
                bestDistance
            )
            {
                bestDistance =
                    closestEnemyDistance;

                bestPosition =
                    candidate;
            }
        }

        Debug.LogWarning(
            "Could not find a fully safe respawn position. " +
            "Using best available position."
        );

        return bestPosition;
    }

    private float GetClosestEnemyDistance(
        Vector2 position,
        GameObject[] enemies
    )
    {
        float closestDistance =
            float.MaxValue;

        foreach (
            GameObject enemy
            in enemies
        )
        {
            if (enemy == null)
                continue;

            float distance =
                Vector2.Distance(
                    position,
                    enemy.transform.position
                );

            if (
                distance <
                closestDistance
            )
            {
                closestDistance =
                    distance;
            }
        }

        return closestDistance;
    }

    private void MovePlayerTo(
        Vector2 newPosition
    )
    {
        if (rb != null)
        {
            rb.position =
                newPosition;

            rb.linearVelocity =
                Vector2.zero;

            rb.angularVelocity =
                0f;
        }
        else
        {
            transform.position =
                newPosition;
        }
    }

    private void ResetPlayerVelocity()
    {
        if (rb == null)
            return;

        rb.linearVelocity =
            Vector2.zero;

        rb.angularVelocity =
            0f;
    }

    public void Heal(float amount)
    {
        if (amount <= 0f)
            return;

        if (currentHealth <= 0f)
            return;

        currentHealth +=
            amount;

        currentHealth =
            Mathf.Clamp(
                currentHealth,
                0f,
                maxHealth
            );

        UpdateHealthUI();

        Debug.Log(
            "PLAYER HEALED: " +
            amount
        );
    }

    public void ReviveFromMockAd()
    {
        currentLives = Mathf.Min(currentLives + 1, maxLives);
        if (currentLives <= 0)
            currentLives = 1;

        currentHealth = maxHealth;
        invulnerabilityTimer = respawnInvulnerabilityDuration;
        UpdateHealthUI();
        UpdateLivesUI();
    }

    public void ApplyKnockback(
        Vector2 direction,
        float force
    )
    {
        if (rb == null)
            return;

        rb.AddForce(
            direction * force,
            ForceMode2D.Impulse
        );
    }

    private void UpdateHealthUI()
    {
        if (healthBar != null)
        {
            healthBar.maxValue =
                maxHealth;

            healthBar.value =
                currentHealth;
        }

        if (healthText != null)
        {
            healthText.text =
                "HP: " +
                Mathf.CeilToInt(
                    currentHealth
                ) +
                " / " +
                Mathf.CeilToInt(
                    maxHealth
                );
        }
    }

    private void UpdateLivesUI()
    {
        if (livesUI != null)
        {
            livesUI.UpdateLives(
                currentLives
            );
        }
    }

    private void Die()
    {
        Debug.Log(
            "===== PLAYER DIED - GAME OVER ====="
        );

        RunManager runManager =
            FindFirstObjectByType<RunManager>();

        if (runManager != null)
        {
            runManager.EndRun();
        }
        else
        {
            Time.timeScale = 0f;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(
            transform.position,
            minEnemyDistance
        );

        Gizmos.DrawWireSphere(
            transform.position,
            respawnSearchRadius
        );
    }
}
