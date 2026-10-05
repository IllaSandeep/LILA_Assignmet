using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemySpawner : MonoBehaviour
{
    [Serializable]
    private struct LevelSettings
    {
        [Min(1)] public int level;
        [Min(0.1f)] public float spawnInterval;
        [Min(1)] public int minimumEnemiesPerSpawn;
        [Min(1)] public int maximumEnemiesPerSpawn;
        [Min(1)] public int maximumActiveEnemies;
        [Min(0f)] public float chaserWeight;
        [Min(0f)] public float shooterWeight;
        [Min(0f)] public float flankerWeight;
        [Min(0f)] public float healthMultiplier;
        [Min(0f)] public float damageMultiplier;
        [Min(0f)] public float speedMultiplier;
    }

    [Header("Enemy Prefabs")]
    [SerializeField] private GameObject chaserPrefab;
    [SerializeField] private GameObject shooterPrefab;
    [SerializeField] private GameObject flankerPrefab;

    [Header("Spawn Location")]
    [Tooltip("Used when the spawn distance range is disabled.")]
    [SerializeField] private float spawnDistance = 17f;
    [SerializeField] private bool useSpawnDistanceRange;
    [SerializeField] private float minimumSpawnDistance = 8f;
    [SerializeField] private float maximumSpawnDistance = 8f;

    [Header("Spawn Modifiers")]
    [Tooltip("1 uses each level's configured spawn interval.")]
    [Min(0.1f)] [SerializeField] private float normalSpawnRateMultiplier = 1f;
    [Tooltip("1 uses each level's configured active enemy cap.")]
    [Min(0.1f)] [SerializeField] private float enemyDensityMultiplier = 1f;

    [Header("Level Progression")]
    [SerializeField] private LevelSettings[] levelSettings =
    {
        new LevelSettings { level = 1, spawnInterval = 3.5f, minimumEnemiesPerSpawn = 1, maximumEnemiesPerSpawn = 1, maximumActiveEnemies = 8, chaserWeight = 100f, shooterWeight = 0f, flankerWeight = 0f, healthMultiplier = 1f, damageMultiplier = 1f, speedMultiplier = 1f },
        new LevelSettings { level = 2, spawnInterval = 3.1f, minimumEnemiesPerSpawn = 1, maximumEnemiesPerSpawn = 1, maximumActiveEnemies = 10, chaserWeight = 85f, shooterWeight = 15f, flankerWeight = 0f, healthMultiplier = 1.05f, damageMultiplier = 1.03f, speedMultiplier = 1.02f },
        new LevelSettings { level = 3, spawnInterval = 2.8f, minimumEnemiesPerSpawn = 1, maximumEnemiesPerSpawn = 2, maximumActiveEnemies = 12, chaserWeight = 70f, shooterWeight = 25f, flankerWeight = 5f, healthMultiplier = 1.10f, damageMultiplier = 1.06f, speedMultiplier = 1.04f },
        new LevelSettings { level = 4, spawnInterval = 2.5f, minimumEnemiesPerSpawn = 2, maximumEnemiesPerSpawn = 2, maximumActiveEnemies = 15, chaserWeight = 60f, shooterWeight = 30f, flankerWeight = 10f, healthMultiplier = 1.15f, damageMultiplier = 1.10f, speedMultiplier = 1.06f },
        new LevelSettings { level = 5, spawnInterval = 2.2f, minimumEnemiesPerSpawn = 2, maximumEnemiesPerSpawn = 2, maximumActiveEnemies = 18, chaserWeight = 50f, shooterWeight = 30f, flankerWeight = 20f, healthMultiplier = 1.20f, damageMultiplier = 1.14f, speedMultiplier = 1.08f },
        new LevelSettings { level = 6, spawnInterval = 2.0f, minimumEnemiesPerSpawn = 2, maximumEnemiesPerSpawn = 2, maximumActiveEnemies = 21, chaserWeight = 45f, shooterWeight = 30f, flankerWeight = 25f, healthMultiplier = 1.25f, damageMultiplier = 1.18f, speedMultiplier = 1.10f },
        new LevelSettings { level = 7, spawnInterval = 1.8f, minimumEnemiesPerSpawn = 2, maximumEnemiesPerSpawn = 3, maximumActiveEnemies = 23, chaserWeight = 40f, shooterWeight = 30f, flankerWeight = 30f, healthMultiplier = 1.30f, damageMultiplier = 1.21f, speedMultiplier = 1.12f },
        new LevelSettings { level = 8, spawnInterval = 1.6f, minimumEnemiesPerSpawn = 3, maximumEnemiesPerSpawn = 3, maximumActiveEnemies = 25, chaserWeight = 38f, shooterWeight = 30f, flankerWeight = 32f, healthMultiplier = 1.35f, damageMultiplier = 1.24f, speedMultiplier = 1.14f }
    };

    [Header("Level 8+ Scaling")]
    [Min(0f)] [SerializeField] private float spawnIntervalDecreasePerLevel = 0.1f;
    [Min(0.1f)] [SerializeField] private float minimumHighLevelSpawnInterval = 1.5f;
    [Min(1)] [SerializeField] private int highLevelEnemiesPerSpawn = 3;
    [Min(1)] [SerializeField] private int highLevelMaximumActiveEnemies = 25;
    [Min(0f)] [SerializeField] private float healthIncreasePerLevel = 0.05f;
    [Min(0f)] [SerializeField] private float damageIncreasePerLevel = 0.03f;
    [Min(0f)] [SerializeField] private float speedIncreasePerLevel = 0.02f;
    [Min(1f)] [SerializeField] private float maximumHighLevelHealthMultiplier = 2f;
    [Min(1f)] [SerializeField] private float maximumHighLevelDamageMultiplier = 1.6f;
    [Min(1f)] [SerializeField] private float maximumHighLevelSpeedMultiplier = 1.35f;

    [Header("Nemesis Encounter")]
    [Min(1)] [SerializeField] private int nemesisCombatMaximumEnemies = 6;
    [Tooltip("0.5 means normal enemies spawn at half their usual rate while a Nemesis is active.")]
    [Range(0.1f, 1f)] [SerializeField] private float normalSpawnRateDuringNemesis = 0.5f;
    [Min(0f)] [SerializeField] private float nemesisDefeatedRecoveryTime = 8f;

    [Header("Debug")]
    [SerializeField] private bool enableSpawnDebug;

    private Transform player;
    private PlayerExperience playerExperience;
    private RunManager runManager;
    private NemesisManager nemesisManager;
    private float spawnTimer;
    private float recoveryTimer;
    private int previousLevel = -1;
    private bool nemesisWasActive;

    private void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
            playerExperience = playerObject.GetComponent<PlayerExperience>();
        }

        runManager = FindFirstObjectByType<RunManager>();
        nemesisManager = FindFirstObjectByType<NemesisManager>();
    }

    private void Update()
    {
        if (player == null)
        {
            FindPlayer();
            if (player == null)
                return;
        }

        if (runManager == null)
            runManager = FindFirstObjectByType<RunManager>();
        if (runManager == null || !runManager.RunActive || Time.timeScale <= 0f)
            return;

        if (playerExperience == null)
            playerExperience = player.GetComponent<PlayerExperience>();

        int playerLevel = playerExperience != null
            ? Mathf.Max(1, playerExperience.GetCurrentLevel())
            : 1;
        LevelSettings settings = GetSettingsForLevel(playerLevel);

        if (playerLevel != previousLevel)
        {
            previousLevel = playerLevel;
            if (enableSpawnDebug)
                Debug.Log($"ENEMY DIFFICULTY: Player Level {playerLevel} | Interval {settings.spawnInterval:0.0}s | Active cap {GetMaximumActiveEnemies(settings)}");
        }

        bool nemesisActive = IsNemesisActive();
        if (nemesisActive)
        {
            nemesisWasActive = true;
            recoveryTimer = 0f;
        }
        else if (nemesisWasActive)
        {
            nemesisWasActive = false;
            recoveryTimer = nemesisDefeatedRecoveryTime;
        }
        else if (recoveryTimer > 0f)
        {
            recoveryTimer = Mathf.Max(0f, recoveryTimer - Time.deltaTime);
        }

        int maxActiveEnemies = GetMaximumActiveEnemies(settings);
        if (nemesisActive)
            maxActiveEnemies = Mathf.Min(maxActiveEnemies, nemesisCombatMaximumEnemies);

        float interval = GetSpawnInterval(settings, nemesisActive);
        int activeEnemies = CountSmallEnemies();

        spawnTimer -= Time.deltaTime;
        if (spawnTimer > 0f || activeEnemies >= maxActiveEnemies)
            return;

        SpawnLevelWave(settings, maxActiveEnemies);
        spawnTimer = interval;
    }

    private void FindPlayer()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject == null)
            return;

        player = playerObject.transform;
        playerExperience = playerObject.GetComponent<PlayerExperience>();
    }

    private LevelSettings GetSettingsForLevel(int playerLevel)
    {
        if (levelSettings == null || levelSettings.Length == 0)
            return GetDefaultLevelOneSettings();

        LevelSettings last = levelSettings[0];
        foreach (LevelSettings entry in levelSettings)
        {
            if (entry.level == playerLevel)
                return entry;
            if (entry.level > last.level)
                last = entry;
        }

        if (playerLevel <= last.level)
        {
            last.level = playerLevel;
            return last;
        }

        int levelsAboveEight = Mathf.Max(0, playerLevel - last.level);
        last.spawnInterval = Mathf.Max(
            minimumHighLevelSpawnInterval,
            last.spawnInterval - spawnIntervalDecreasePerLevel * levelsAboveEight);
        last.minimumEnemiesPerSpawn = highLevelEnemiesPerSpawn;
        last.maximumEnemiesPerSpawn = highLevelEnemiesPerSpawn;
        last.maximumActiveEnemies = Mathf.Min(
            highLevelMaximumActiveEnemies,
            last.maximumActiveEnemies + levelsAboveEight);
        last.healthMultiplier = Mathf.Min(
            maximumHighLevelHealthMultiplier,
            last.healthMultiplier + healthIncreasePerLevel * levelsAboveEight);
        last.damageMultiplier = Mathf.Min(
            maximumHighLevelDamageMultiplier,
            last.damageMultiplier + damageIncreasePerLevel * levelsAboveEight);
        last.speedMultiplier = Mathf.Min(
            maximumHighLevelSpeedMultiplier,
            last.speedMultiplier + speedIncreasePerLevel * levelsAboveEight);
        last.level = playerLevel;
        return last;
    }

    private static LevelSettings GetDefaultLevelOneSettings()
    {
        return new LevelSettings
        {
            level = 1,
            spawnInterval = 3.5f,
            minimumEnemiesPerSpawn = 1,
            maximumEnemiesPerSpawn = 1,
            maximumActiveEnemies = 8,
            chaserWeight = 100f,
            healthMultiplier = 1f,
            damageMultiplier = 1f,
            speedMultiplier = 1f
        };
    }

    private int GetMaximumActiveEnemies(LevelSettings settings)
    {
        return Mathf.Max(1, Mathf.RoundToInt(settings.maximumActiveEnemies * enemyDensityMultiplier));
    }

    private float GetSpawnInterval(LevelSettings settings, bool nemesisActive)
    {
        float rateMultiplier = Mathf.Max(0.01f, normalSpawnRateMultiplier);
        if (nemesisActive)
            rateMultiplier *= normalSpawnRateDuringNemesis;
        else if (recoveryTimer > 0f)
        {
            float recovery = 1f - recoveryTimer / Mathf.Max(0.01f, nemesisDefeatedRecoveryTime);
            rateMultiplier *= Mathf.Lerp(normalSpawnRateDuringNemesis, 1f, recovery);
        }

        return Mathf.Max(0.1f, settings.spawnInterval / rateMultiplier);
    }

    private void SpawnLevelWave(LevelSettings settings, int maxActiveEnemies)
    {
        int count = Random.Range(
            Mathf.Max(1, settings.minimumEnemiesPerSpawn),
            Mathf.Max(settings.minimumEnemiesPerSpawn, settings.maximumEnemiesPerSpawn) + 1);

        int chasers = 0;
        int shooters = 0;
        int flankers = 0;
        for (int i = 0; i < count && CountSmallEnemies() < maxActiveEnemies; i++)
        {
            GameObject prefab = ChooseEnemyPrefab(settings, out int enemyType);
            if (!SpawnEnemy(prefab, settings))
                continue;

            if (enemyType == 0) chasers++;
            else if (enemyType == 1) shooters++;
            else flankers++;
        }

        if (enableSpawnDebug)
        {
            string composition = $"Chaser x{chasers}, Shooter x{shooters}, Flanker x{flankers}";
            Debug.Log($"LEVEL {settings.level} SPAWN: {composition} | Active {CountSmallEnemies()}/{maxActiveEnemies}");
        }
    }

    private GameObject ChooseEnemyPrefab(LevelSettings settings, out int enemyType)
    {
        float chaserWeight = chaserPrefab != null ? Mathf.Max(0f, settings.chaserWeight) : 0f;
        float shooterWeight = shooterPrefab != null ? Mathf.Max(0f, settings.shooterWeight) : 0f;
        float flankerWeight = flankerPrefab != null ? Mathf.Max(0f, settings.flankerWeight) : 0f;
        float totalWeight = chaserWeight + shooterWeight + flankerWeight;

        if (totalWeight <= 0f)
        {
            enemyType = 0;
            return chaserPrefab != null ? chaserPrefab : shooterPrefab != null ? shooterPrefab : flankerPrefab;
        }

        float roll = Random.value * totalWeight;
        if (roll < chaserWeight)
        {
            enemyType = 0;
            return chaserPrefab;
        }

        roll -= chaserWeight;
        if (roll < shooterWeight)
        {
            enemyType = 1;
            return shooterPrefab;
        }

        enemyType = 2;
        return flankerPrefab;
    }

    private bool SpawnEnemy(GameObject enemyPrefab, LevelSettings settings)
    {
        if (enemyPrefab == null || player == null)
            return false;

        Vector2 direction = Random.insideUnitCircle.normalized;
        if (direction.sqrMagnitude < 0.01f)
            direction = Vector2.right;

        float distance = useSpawnDistanceRange
            ? Random.Range(minimumSpawnDistance, maximumSpawnDistance)
            : spawnDistance;
        Vector2 position = (Vector2)player.position + direction * distance;
        GameObject spawned = Instantiate(enemyPrefab, position, Quaternion.identity);
        ApplyLevelMultipliers(spawned, settings);
        return true;
    }

    private static void ApplyLevelMultipliers(GameObject spawned, LevelSettings settings)
    {
        EnemyHealth health = spawned.GetComponent<EnemyHealth>();
        if (health != null)
            health.ApplyDifficultyMultiplier(settings.healthMultiplier);

        EnemyController chaser = spawned.GetComponent<EnemyController>();
        if (chaser != null)
            chaser.ApplyDifficultyMultipliers(settings.speedMultiplier, settings.damageMultiplier);

        ShooterEnemyController shooter = spawned.GetComponent<ShooterEnemyController>();
        if (shooter != null)
            shooter.ApplyDifficultyMultipliers(settings.speedMultiplier, settings.damageMultiplier);

        FlankerEnemyController flanker = spawned.GetComponent<FlankerEnemyController>();
        if (flanker != null)
            flanker.ApplyDifficultyMultipliers(settings.speedMultiplier, settings.damageMultiplier);
    }

    private bool IsNemesisActive()
    {
        if (nemesisManager == null)
            nemesisManager = FindFirstObjectByType<NemesisManager>();

        return nemesisManager != null && nemesisManager.GetCurrentNemesisObject() != null;
    }

    private int CountSmallEnemies()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        int count = 0;
        foreach (GameObject enemy in enemies)
        {
            if (enemy == null)
                continue;

            if (enemy.GetComponent<NemesisController>() == null)
                count++;
        }
        return count;
    }
}
