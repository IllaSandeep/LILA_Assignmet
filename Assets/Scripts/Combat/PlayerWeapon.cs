using UnityEngine;

public class PlayerWeapon : MonoBehaviour
{
    [Header("Weapon Balance")]
    [SerializeField] private GameObject projectilePrefab;
    [Tooltip("Time between automatic shots. Higher value = slower firing. Lower value = faster firing.")]
    [Range(0.15f, 1.0f)]
    [SerializeField] private float attackCooldown = 0.35f;
    [SerializeField] private float projectileDamage = 10f;

    [Header("Temporary Ability Multipliers")]
    [SerializeField] private float powerDamageMultiplier = 2f;
    [Tooltip("Lower value = faster Rapid Fire.")]
    [SerializeField] private float rapidFireCooldownMultiplier = 0.55f;
    [SerializeField] private float heavyShotDamageMultiplier = 2.5f;
    [SerializeField] private float heavyShotCooldownMultiplier = 1.8f;

    [Header("Jet")]
    [SerializeField] private Transform playerVisual;

    [Header("Projectile Spawn")]
    [SerializeField] private float projectileSpawnOffset = 0.8f;

    private float cooldownTimer;
    private float scheduledCooldown;

    private PlayerBehaviorTracker behaviorTracker;
    private RunManager runManager;

    private float baseDamage;

    private bool powerSurgeActive;
    private bool rapidFireActive;
    private bool heavyShotActive;
    private bool piercingActive;
    private bool missingProjectileWarningLogged;
    private bool missingVisualWarningLogged;

    private PermanentUpgradeManager permanentUpgradeManager;

    private void Awake()
    {
        behaviorTracker =
            GetComponent<PlayerBehaviorTracker>();

        runManager =
            FindFirstObjectByType<RunManager>();

        permanentUpgradeManager =
            FindFirstObjectByType<PermanentUpgradeManager>();

        baseDamage =
            projectileDamage;

        if (playerVisual == null)
        {
            Transform visual =
                transform.Find("PlayerVisual");

            if (visual != null)
                playerVisual = visual;
        }
    }

    private void Start()
    {
        // The first projectile fires on the first active gameplay frame.
        cooldownTimer = 0f;
        scheduledCooldown = GetCurrentCooldown();
    }

    private void Update()
    {
        if (
            Time.timeScale <= 0f ||
            (runManager != null && !runManager.RunActive)
        )
            return;

        float currentCooldown = GetCurrentCooldown();
        if (cooldownTimer > 0f && scheduledCooldown > 0f &&
            !Mathf.Approximately(currentCooldown, scheduledCooldown))
        {
            // Preserve the elapsed fraction of the current interval when a
            // cooldown or firing-rate ability changes during play.
            cooldownTimer *= currentCooldown / scheduledCooldown;
        }

        scheduledCooldown = currentCooldown;
        cooldownTimer -= Time.deltaTime;

        if (cooldownTimer <= 0f)
        {
            Shoot();
            scheduledCooldown = GetCurrentCooldown();
            cooldownTimer = scheduledCooldown;
        }
    }

    private void Shoot()
    {
        if (projectilePrefab == null)
        {
            if (!missingProjectileWarningLogged)
            {
                Debug.LogWarning("PlayerWeapon: Projectile Prefab is missing.");
                missingProjectileWarningLogged = true;
            }

            return;
        }

        if (playerVisual == null)
        {
            if (!missingVisualWarningLogged)
            {
                Debug.LogWarning("PlayerWeapon: PlayerVisual is missing.");
                missingVisualWarningLogged = true;
            }

            return;
        }

        // The jet's UP direction is its nose.
        Vector2 direction =
            playerVisual.up.normalized;

        Vector2 spawnPosition =
            (Vector2)transform.position +
            direction *
            projectileSpawnOffset;

        GameObject projectile =
            Instantiate(
                projectilePrefab,
                spawnPosition,
                Quaternion.identity
            );

        Projectile projectileScript =
            projectile.GetComponent<Projectile>();

        if (projectileScript != null)
        {
            projectileScript.Initialize(
                direction,
                GetCurrentDamage(),
                piercingActive
            );

            if (heavyShotActive)
                projectileScript.SetHeavyShot();
        }

        if (behaviorTracker != null)
            behaviorTracker.RegisterShot();
    }

    private float GetCurrentDamage()
    {
        float damage =
            baseDamage;

        if (permanentUpgradeManager != null)
        {
            damage *=
                permanentUpgradeManager
                    .GetDamageMultiplier();
        }

        if (powerSurgeActive)
            damage *=
                powerDamageMultiplier;

        if (heavyShotActive)
            damage *=
                heavyShotDamageMultiplier;

        return damage;
    }

    private float GetCurrentCooldown()
    {
        float cooldown =
            attackCooldown;

        if (permanentUpgradeManager != null)
        {
            float fireRateMultiplier =
                permanentUpgradeManager
                    .GetFireRateMultiplier();

            cooldown /=
                fireRateMultiplier;
        }

        if (rapidFireActive)
            cooldown *=
                rapidFireCooldownMultiplier;

        if (heavyShotActive)
            cooldown *=
                heavyShotCooldownMultiplier;

        return Mathf.Max(
            0.08f,
            cooldown
        );
    }

    public void ActivatePowerSurge()
    {
        powerSurgeActive = true;

        Debug.Log(
            "POWER SURGE ACTIVATED"
        );
    }

    public void DeactivatePowerSurge()
    {
        powerSurgeActive = false;

        Debug.Log(
            "POWER SURGE EXPIRED"
        );
    }

    public void ActivateRapidFire()
    {
        rapidFireActive = true;

        Debug.Log(
            "RAPID FIRE ACTIVATED"
        );
    }

    public void DeactivateRapidFire()
    {
        rapidFireActive = false;

        Debug.Log(
            "RAPID FIRE EXPIRED"
        );
    }

    public void ActivateHeavyShot()
    {
        heavyShotActive = true;

        Debug.Log(
            "HEAVY SHOT ACTIVATED"
        );
    }

    public void DeactivateHeavyShot()
    {
        heavyShotActive = false;

        Debug.Log(
            "HEAVY SHOT EXPIRED"
        );
    }

    public void ActivatePiercing()
    {
        piercingActive = true;

        Debug.Log(
            "PIERCING ACTIVATED"
        );
    }

    public void DeactivatePiercing()
    {
        piercingActive = false;

        Debug.Log(
            "PIERCING EXPIRED"
        );
    }
}
