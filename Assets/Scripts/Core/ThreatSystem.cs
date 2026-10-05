using UnityEngine;
using TMPro;

public class ThreatSystem : MonoBehaviour
{
    [Header("Threat Settings")]
    [SerializeField] private float detectionRadius = 8f;

    [Header("Threat Weights")]
    [SerializeField] private float enemyCountWeight = 30f;
    [SerializeField] private float proximityWeight = 30f;
    [SerializeField] private float healthWeight = 25f;
    [SerializeField] private float projectileWeight = 15f;

    [Header("UI")]
    [SerializeField] private TMP_Text threatText;

    private Transform player;
    private PlayerHealth playerHealth;

    private float currentThreat;

    public float CurrentThreat => currentThreat;

    private void Start()
    {
        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;

            playerHealth =
                playerObject.GetComponent<PlayerHealth>();
        }

        UpdateThreatUI();
    }

    private void Update()
    {
        if (player == null)
            return;

        CalculateThreat();
        UpdateThreatUI();
    }

    private void CalculateThreat()
    {
        float enemyCountThreat =
            CalculateEnemyCountThreat();

        float proximityThreat =
            CalculateProximityThreat();

        float healthThreat =
            CalculateHealthThreat();

        float projectileThreat =
            CalculateProjectileThreat();

        currentThreat =
            enemyCountThreat +
            proximityThreat +
            healthThreat +
            projectileThreat;

        currentThreat =
            Mathf.Clamp(
                currentThreat,
                0f,
                100f
            );
    }

    private float CalculateEnemyCountThreat()
    {
        Collider2D[] enemies =
            Physics2D.OverlapCircleAll(
                player.position,
                detectionRadius
            );

        int enemyCount = 0;

        foreach (Collider2D collider in enemies)
        {
            if (collider.CompareTag("Enemy"))
            {
                enemyCount++;
            }
        }

        // Around 15 enemies = maximum count pressure.
        float normalized =
            Mathf.Clamp01(
                enemyCount / 15f
            );

        return normalized * enemyCountWeight;
    }

    private float CalculateProximityThreat()
    {
        Collider2D[] enemies =
            Physics2D.OverlapCircleAll(
                player.position,
                detectionRadius
            );

        float totalPressure = 0f;

        foreach (Collider2D collider in enemies)
        {
            if (!collider.CompareTag("Enemy"))
                continue;

            float distance =
                Vector2.Distance(
                    player.position,
                    collider.transform.position
                );

            float proximity =
                1f -
                Mathf.Clamp01(
                    distance / detectionRadius
                );

            totalPressure += proximity;
        }

        // Normalize approximately around 8 nearby
        // enemies contributing significant pressure.
        float normalized =
            Mathf.Clamp01(
                totalPressure / 8f
            );

        return normalized * proximityWeight;
    }

    private float CalculateHealthThreat()
    {
        if (playerHealth == null)
            return 0f;

        float healthPercent =
            playerHealth.CurrentHealth /
            playerHealth.MaxHealth;

        // High HP = low threat.
        // Low HP = high threat.
        float danger =
            1f - healthPercent;

        return danger * healthWeight;
    }

    private float CalculateProjectileThreat()
    {
        Collider2D[] projectiles =
            Physics2D.OverlapCircleAll(
                player.position,
                detectionRadius
            );

        int projectileCount = 0;

        foreach (Collider2D collider in projectiles)
        {
            if (collider.GetComponent<EnemyProjectile>() != null)
            {
                projectileCount++;
            }
        }

        // Around 5 nearby projectiles = maximum
        // projectile pressure.
        float normalized =
            Mathf.Clamp01(
                projectileCount / 5f
            );

        return normalized * projectileWeight;
    }

    private void UpdateThreatUI()
    {
        if (threatText == null)
            return;

        threatText.text =
            Mathf.RoundToInt(currentThreat) +
            "%";
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(
            transform.position,
            detectionRadius
        );
    }
}