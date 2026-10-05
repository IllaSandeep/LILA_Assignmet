using UnityEngine;

public class NemesisController : MonoBehaviour
{
    public enum NemesisType
    {
        Balanced,
        Trapper,
        Hunter,
        Interceptor,
        Disruptor
    }

    [Header("Nemesis")]
    [SerializeField] private NemesisType nemesisType =
        NemesisType.Balanced;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;

    [Header("Combat Distance")]
    [SerializeField] private float meleeRange = 1.5f;
    [SerializeField] private float shootingRange = 6f;

    [Header("Hunter")]
    [SerializeField] private float hunterPreferredDistance = 6f;
    [SerializeField] private float hunterDistanceTolerance = 1f;
    [SerializeField] private float hunterRetreatSpeed = 2.5f;

    [Header("Adaptive AI")]
    [SerializeField] private float predictionTime = 0.75f;
    [Range(0.1f, 1f)] [SerializeField] private float predictionAccuracy = 0.65f;
    [SerializeField] private float repositionCooldown = 3f;
    [SerializeField] private float interceptBurstMultiplier = 1.5f;
    [SerializeField] private float reactionDelay = 0.25f;
    [SerializeField] private float adaptationRampTime = 12f;

    [Header("Melee Attack")]
    [SerializeField] private float meleeDamage = 15f;
    [SerializeField] private float meleeCooldown = 1.5f;

    [Header("Ranged Attack")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float shootingCooldown = 1.2f;
    [SerializeField] private Transform projectileSpawnPoint;

    [Header("Trap")]
    [SerializeField] private GameObject trapPrefab;
    [SerializeField] private float trapCooldown = 5f;
    [SerializeField] private float trapSpawnDistance = 2.5f;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    [Header("Target")]
    [SerializeField] private Transform player;

    private float meleeTimer;
    private float shootingTimer;
    private float trapTimer;

    private PlayerHealth playerHealth;
    private EnemyHealth enemyHealth;
    private PlayerBehaviorTracker behaviorTracker;
    private PlayerAbilitySystem playerAbilitySystem;
    private NemesisType adaptiveTarget;
    private float adaptiveTargetConfidence;
    private float adaptationStrength = 0.35f;
    private float profileLockDuration = 18f;
    private float profileLockTimer;
    private float adaptationLevel;
    private float movementMultiplier = 1f;
    private float damageMultiplier = 1f;
    private float predictionMultiplier = 1f;
    private float repositionTimer;
    private float burstTimer;
    private float reactionTimer;

    private void Awake()
    {
        enemyHealth =
            GetComponent<EnemyHealth>();

        if (animator == null)
        {
            animator =
                GetComponent<Animator>();
        }
    }

    private void Start()
    {
        FindPlayer();

        meleeTimer =
            meleeCooldown;

        shootingTimer =
            shootingCooldown;

        trapTimer =
            trapCooldown;

        PlaySpawnAnimation();
    }

    private void Update()
    {
        if (Time.timeScale <= 0f)
            return;

        if (player == null)
        {
            FindPlayer();
            return;
        }

        if (
            enemyHealth != null &&
            enemyHealth.IsDead()
        )
        {
            return;
        }

        meleeTimer -=
            Time.deltaTime;

        shootingTimer -=
            Time.deltaTime;

        trapTimer -=
            Time.deltaTime;

        if (profileLockTimer > 0f)
            profileLockTimer -= Time.deltaTime;
        else if (adaptiveTarget != nemesisType)
            nemesisType = adaptiveTarget;

        float desiredAdaptation = adaptiveTargetConfidence * adaptationStrength;
        adaptationLevel = Mathf.MoveTowards(
            adaptationLevel,
            desiredAdaptation,
            Time.deltaTime * Mathf.Max(0.01f, adaptationStrength) / Mathf.Max(0.1f, adaptationRampTime)
        );
        repositionTimer -= Time.deltaTime;
        burstTimer = Mathf.Max(0f, burstTimer - Time.deltaTime);
        reactionTimer = Mathf.Max(0f, reactionTimer - Time.deltaTime);

        UpdateCombat();
    }

    private void FindPlayer()
    {
        GameObject playerObject =
            GameObject.FindGameObjectWithTag(
                "Player"
            );

        if (playerObject == null)
            return;

        player =
            playerObject.transform;

        playerHealth =
            playerObject.GetComponent<PlayerHealth>();
        behaviorTracker = playerObject.GetComponent<PlayerBehaviorTracker>();
        playerAbilitySystem = playerObject.GetComponent<PlayerAbilitySystem>();
    }

    private void UpdateCombat()
    {
        float distance =
            Vector2.Distance(
                transform.position,
                player.position
            );

        // =========================================
        // HUNTER
        // =========================================

        if (nemesisType == NemesisType.Hunter)
        {
            UpdateHunter(distance);
            return;
        }

        // =========================================
        // TRAPPER
        // =========================================

        if (nemesisType == NemesisType.Trapper)
        {
            UpdateTrapper(distance);
            return;
        }

        if (nemesisType == NemesisType.Interceptor)
        {
            UpdateInterceptor(distance);
            return;
        }

        if (nemesisType == NemesisType.Disruptor)
        {
            UpdateDisruptor(distance);
            return;
        }

        // =========================================
        // DEFAULT / BALANCED
        // =========================================

        UpdateBalanced(distance);
    }

    // =============================================
    // HUNTER
    // =============================================

    private void UpdateHunter(float distance)
    {
        // Player is too close.
        // Hunter immediately retreats.
        if (distance < hunterPreferredDistance)
        {
            StopFightAnimation();

            RetreatFromPlayer();

            ShootAtPlayer();

            return;
        }

        // Player is too far.
        // Hunter moves toward the player until
        // preferred distance is restored.
        if (
            distance >
            hunterPreferredDistance +
            hunterDistanceTolerance
        )
        {
            StopFightAnimation();

            MoveTowardPlayer();

            ShootAtPlayer();

            return;
        }

        // Perfect Hunter distance.
        // Stay here and fire.
        StopFightAnimation();

        ShootAtPlayer();
    }

    private void RetreatFromPlayer()
    {
        Vector2 direction =
            (
                (Vector2)transform.position -
                (Vector2)player.position
            ).normalized;

        transform.position +=
            (Vector3)(
                direction *
                hunterRetreatSpeed * movementMultiplier *
                Time.deltaTime
            );
    }

    // =============================================
    // TRAPPER
    // =============================================

    private void UpdateTrapper(float distance)
    {
        // Close range
        if (distance <= meleeRange + 0.5f)
        {
            if (trapTimer <= 0f)
            {
                SpawnTrapNearPlayer();

                trapTimer =
                    trapCooldown;
            }

            MeleeAttack();

            return;
        }

        StopFightAnimation();

        // Long range
        if (distance >= shootingRange)
        {
            ShootAtPlayer();

            return;
        }

        // Approach a predicted lane to punish direct rushing without perfect prediction.
        Vector2 predicted = GetPredictedPlayerPosition();
        Vector2 away = ((Vector2)transform.position - predicted).normalized;
        MoveToward(predicted + away * trapSpawnDistance, moveSpeed * (1f + adaptationLevel * 0.1f));
    }

    // =============================================
    // BALANCED
    // =============================================

    private void UpdateBalanced(float distance)
    {
        if (distance <= meleeRange)
        {
            MeleeAttack();

            return;
        }

        StopFightAnimation();

        if (distance >= shootingRange)
        {
            ShootAtPlayer();

            return;
        }

        MoveTowardPlayer();
    }

    private void UpdateInterceptor(float distance)
    {
        if (distance <= meleeRange)
        {
            MeleeAttack();
            return;
        }

        if (repositionTimer <= 0f && distance < hunterPreferredDistance)
        {
            burstTimer = 0.45f;
            repositionTimer = repositionCooldown;
        }

        Vector2 predicted = GetPredictedPlayerPosition();
        Vector2 direction = (predicted - (Vector2)transform.position).normalized;
        Vector2 perpendicular = new Vector2(-direction.y, direction.x) * (Random.value < 0.5f ? -1f : 1f);
        Vector2 intercept = predicted + perpendicular * Mathf.Min(hunterPreferredDistance * 0.5f, 3f);
        float speed = moveSpeed * (burstTimer > 0f ? interceptBurstMultiplier : 1f);
        MoveToward(intercept, speed * (1f + adaptationLevel * 0.12f));
        ShootAtPlayer(true);
    }

    private void UpdateDisruptor(float distance)
    {
        bool abilityActive = IsPlayerUsingAbility();
        if (abilityActive)
        {
            if (trapTimer <= 0f)
            {
                SpawnTrapNearPlayer();
                trapTimer = trapCooldown;
            }
            ShootAtPlayer(true);
        }

        if (distance <= meleeRange)
            MeleeAttack();
        else if (abilityActive || distance >= shootingRange)
            MoveToward(GetPredictedPlayerPosition(), moveSpeed * 0.8f);
        else
            MoveTowardPlayer();
    }

    // =============================================
    // MOVEMENT
    // =============================================

    private void MoveTowardPlayer()
    {
        Vector2 direction =
            (
                (Vector2)player.position -
                (Vector2)transform.position
            ).normalized;

        transform.position +=
            (Vector3)(
                    direction *
                    moveSpeed * movementMultiplier *
                    (1f + adaptationLevel * 0.12f) *
                    Time.deltaTime
            );
    }

    private void MoveToward(Vector2 target, float speed)
    {
        Vector2 direction = (target - (Vector2)transform.position).normalized;
        transform.position += (Vector3)(direction * speed * movementMultiplier * Time.deltaTime);
    }

    private Vector2 GetPredictedPlayerPosition()
    {
        Vector2 actual = player.position;
        if (behaviorTracker == null)
            return actual;

        Vector2 prediction = behaviorTracker.GetPredictedPosition(
            predictionTime * predictionMultiplier,
            predictionAccuracy * predictionMultiplier
        );
        float uncertainty = Mathf.Max(0f, 1f - predictionAccuracy) * 1.5f;
        return Vector2.Lerp(actual, prediction, Mathf.Clamp01(predictionAccuracy)) +
               Random.insideUnitCircle * uncertainty;
    }

    private bool IsPlayerUsingAbility()
    {
        if (playerAbilitySystem == null)
            return false;
        return playerAbilitySystem.IsAbilityActive("Power") ||
               playerAbilitySystem.IsAbilityActive("RapidFire") ||
               playerAbilitySystem.IsAbilityActive("Speed") ||
               playerAbilitySystem.IsAbilityActive("HeavyShot") ||
               playerAbilitySystem.IsAbilityActive("Piercing") ||
               playerAbilitySystem.IsAbilityActive("Shield");
    }

    // =============================================
    // MELEE
    // =============================================

    private void MeleeAttack()
    {
        if (meleeTimer > 0f)
            return;

        PlayFightAnimation();

        if (playerHealth != null)
        {
            playerHealth.TakeDamage(
                meleeDamage * damageMultiplier * (1f + adaptationLevel * 0.1f)
            );
        }

        meleeTimer =
            meleeCooldown;

        Debug.Log(
            "NEMESIS MELEE ATTACK!"
        );
    }

    // =============================================
    // SHOOTING
    // =============================================

    private void ShootAtPlayer(bool predictive = false)
    {
        if (shootingTimer > 0f)
            return;

        if (projectilePrefab == null)
        {
            Debug.LogWarning(
                "NemesisController: Projectile Prefab missing."
            );

            return;
        }

        Vector2 spawnPosition =
            transform.position;

        if (
            projectileSpawnPoint != null
        )
        {
            spawnPosition =
                projectileSpawnPoint.position;
        }

        if (reactionTimer > 0f)
            return;
        Vector2 targetPosition = predictive || nemesisType == NemesisType.Hunter
            ? GetPredictedPlayerPosition()
            : (Vector2)player.position;
        Vector2 direction = (targetPosition - spawnPosition).normalized;

        GameObject projectile =
            Instantiate(
                projectilePrefab,
                spawnPosition,
                Quaternion.identity
            );

        NemesisProjectile projectileScript =
            projectile.GetComponent<NemesisProjectile>();

        if (projectileScript != null)
        {
            projectileScript.Initialize(
                direction,
                movementMultiplier,
                damageMultiplier * (1f + adaptationLevel * 0.1f)
            );
        }

        shootingTimer = shootingCooldown / Mathf.Max(0.5f, 1f + adaptationLevel * 0.15f);
        reactionTimer = reactionDelay;

        Debug.Log(
            "NEMESIS FIRED PROJECTILE!"
        );
    }

    // =============================================
    // TRAPS
    // =============================================

    private void SpawnTrapNearPlayer()
    {
        if (trapPrefab == null)
        {
            Debug.LogWarning(
                "NemesisController: Trap Prefab missing."
            );

            return;
        }

        Vector2 predictedPlayer = GetPredictedPlayerPosition();
        Vector2 randomDirection = (predictedPlayer - (Vector2)player.position).normalized;
        if (randomDirection.sqrMagnitude < 0.01f)
            randomDirection = Random.insideUnitCircle.normalized;

        if (
            randomDirection.sqrMagnitude <
            0.01f
        )
        {
            randomDirection =
                Vector2.right;
        }

        Vector2 spawnPosition =
            predictedPlayer +
            randomDirection *
            trapSpawnDistance;

        GameObject trap = Instantiate(
            trapPrefab,
            spawnPosition,
            Quaternion.identity
        );
        NemesisTrap trapScript = trap.GetComponent<NemesisTrap>();
        if (trapScript != null)
            trapScript.SetDamageMultiplier(damageMultiplier * (1f + adaptationLevel * 0.1f));

        Debug.Log(
            "NEMESIS TRAP DEPLOYED!"
        );
    }

    // =============================================
    // ANIMATION
    // =============================================

    private void PlaySpawnAnimation()
    {
        if (animator == null)
            return;

        animator.ResetTrigger(
            "Spawn"
        );

        animator.SetTrigger(
            "Spawn"
        );
    }

    private void PlayFightAnimation()
    {
        if (animator == null)
            return;

        animator.ResetTrigger(
            "Fight"
        );

        animator.SetTrigger(
            "Fight"
        );
    }

    private void StopFightAnimation()
    {
        if (animator == null)
            return;

        AnimatorStateInfo state =
            animator.GetCurrentAnimatorStateInfo(0);

        if (state.IsName("Fight"))
        {
            animator.Play(
                "Idle",
                0,
                0f
            );
        }
    }

    public void PlayDeathAnimation()
    {
        if (animator == null)
            return;

        animator.ResetTrigger(
            "Death"
        );

        animator.SetTrigger(
            "Death"
        );
    }

    // =============================================
    // TYPE
    // =============================================

    public void SetNemesisType(
        NemesisType type
    )
    {
        nemesisType =
            type;
        adaptiveTarget = type;

        Debug.Log(
            "NEMESIS TYPE: " +
            nemesisType
        );
    }

    public NemesisType GetNemesisType()
    {
        return nemesisType;
    }

    public float AdaptationLevel => adaptationLevel;

    public void ConfigureLearning(float strength, float lockDuration, float confidence)
    {
        adaptationStrength = Mathf.Clamp01(strength);
        profileLockDuration = Mathf.Max(0f, lockDuration);
        profileLockTimer = profileLockDuration;
        adaptiveTarget = nemesisType;
        adaptiveTargetConfidence = Mathf.Clamp01(confidence);
    }

    public void SetAdaptiveTarget(NemesisProfile.NemesisType type, float confidence, float strength, float lockDuration)
    {
        NemesisType converted = (NemesisType)type;
        adaptiveTargetConfidence = Mathf.Clamp01(confidence);
        adaptationStrength = Mathf.Clamp01(strength);
        if (converted == adaptiveTarget)
            return;
        adaptiveTarget = converted;
        profileLockDuration = Mathf.Max(0f, lockDuration);
        profileLockTimer = profileLockDuration;
    }

    public void SetEncounterScaling(float health, float speed, float damage, float prediction)
    {
        if (enemyHealth != null)
            enemyHealth.ApplyDifficultyMultiplier(health);
        movementMultiplier = Mathf.Max(0.1f, speed);
        damageMultiplier = Mathf.Max(0.1f, damage);
        predictionMultiplier = Mathf.Clamp(prediction, 0.5f, 1.5f);
    }
}
