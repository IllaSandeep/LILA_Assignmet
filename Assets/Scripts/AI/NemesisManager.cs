using UnityEngine;
using System.Collections;
using TMPro;

public class NemesisManager : MonoBehaviour
{
    [Header("Nemesis Prefab")]
    [SerializeField] private GameObject nemesisPrefab;

    [Header("Spawn")]
    [Tooltip("Radial distance from the current player position. Kept between 7 and 10 world units.")]
    [Range(7f, 10f)] [SerializeField] private float spawnDistance = 8f;
    [Tooltip("The warning begins this many seconds before the scheduled Nemesis spawn.")]
    [Min(0f)] [SerializeField] private float spawnWarningDuration = 10f;
    [SerializeField] private string warningStartMessage = "NEMESIS SIGNAL DETECTED";
    [SerializeField] private string warningApproachingMessage = "ADAPTIVE THREAT APPROACHING";
    [SerializeField] private string warningIncomingMessage = "NEMESIS INCOMING";
    [SerializeField] private TMP_Text warningText;

    [Header("Learning and Adaptation")]
    [SerializeField] private float behaviorEvaluationInterval = 25f;
    [Range(0f, 1f)] [SerializeField] private float minimumBehaviorConfidence = 0.35f;
    [Range(0f, 1f)] [SerializeField] private float adaptationStrength = 0.35f;
    [SerializeField] private float profileLockDuration = 18f;
    [SerializeField] private int maxRecentNemesisRepeat = 2;
    [Range(0f, 1f)] [SerializeField] private float nemesisVariationStrength = 0.4f;

    [Header("Encounter Escalation")]
    [SerializeField] private float healthMultiplierPerEncounter = 0.08f;
    [SerializeField] private float speedMultiplierPerEncounter = 0.04f;
    [SerializeField] private float damageMultiplierPerEncounter = 0.05f;
    [SerializeField] private float predictionMultiplierPerEncounter = 0.08f;
    [SerializeField] private float maximumEscalationMultiplier = 1.5f;
    [SerializeField] private bool enableAIDebug;
    [SerializeField] private bool debugNemesis;

    [Header("Player")]
    [SerializeField] private Transform player;

    private PlayerBehaviorTracker behaviorTracker;
    private NemesisProfile nemesisProfile;

    private RunManager runManager;

    private NemesisProfile.NemesisType currentNemesis =
        NemesisProfile.NemesisType.Balanced;

    private GameObject currentNemesisObject;

    private float nextNemesisTime;
    private bool timerInitialized;
    private int nemesisCount;
    private bool spawnWarningInProgress;
    private float nextBehaviorEvaluationTime;
    private NemesisProfile.NemesisType lastSpawnedNemesis = NemesisProfile.NemesisType.Balanced;
    private int repeatedNemesisCount;

    private void Start()
    {
        FindRunManager();

        if (runManager == null)
        {
            Debug.LogError(
                "NEMESIS MANAGER: RunManager NOT FOUND!"
            );

            return;
        }

        FindPlayer();
    }

    private void FindRunManager()
    {
        RunManager[] candidates = FindObjectsByType<RunManager>(FindObjectsSortMode.None);
        runManager = null;
        foreach (RunManager candidate in candidates)
        {
            if (candidate.gameObject.scene == gameObject.scene)
            {
                runManager = candidate;
                return;
            }
        }
    }

    private void Update()
    {
        if (runManager == null)
            FindRunManager();

        if (runManager == null || !runManager.RunInitialized || !runManager.RunActive || Time.timeScale <= 0f)
            return;

        if (!timerInitialized)
            InitializeRunTimer();

        if (!IsCurrentPlayer(player))
        {
            FindPlayer();
            if (!IsCurrentPlayer(player))
                return;
        }

        EvaluatePlayerBehaviorIfDue();

        // Clear a defeated/destroyed Nemesis, but do not let it reset the
        // next scheduled spawn time.
        if (currentNemesisObject != null)
        {
            EnemyHealth currentHealth =
                currentNemesisObject
                    .GetComponent<EnemyHealth>();

            if (!currentNemesisObject.activeInHierarchy || (currentHealth != null && currentHealth.IsDead()))
                currentNemesisObject = null;
        }

        float elapsedTime =
            runManager.ElapsedTime;

        if (
            elapsedTime >=
            nextNemesisTime - spawnWarningDuration
        )
        {
            BeginNemesisSpawn();
        }
    }

    private void InitializeRunTimer()
    {
        timerInitialized = true;
        nextNemesisTime = runManager.FirstNemesisSpawnTime;
        nextBehaviorEvaluationTime = Mathf.Max(behaviorEvaluationInterval, nextNemesisTime);

        if (debugNemesis)
        {
            Debug.Log($"[NEMESIS] Run started at time: {runManager.ElapsedTime:0.00}s");
            Debug.Log($"[NEMESIS] First spawn timer: {runManager.ElapsedTime:0.00} / {runManager.FirstNemesisSpawnTime:0.00}s");
        }
    }

    private void EvaluatePlayerBehaviorIfDue()
    {
        if (behaviorTracker == null || nemesisProfile == null)
            return;
        if (runManager.ElapsedTime < nextBehaviorEvaluationTime)
            return;

        nextBehaviorEvaluationTime = runManager.ElapsedTime + Mathf.Max(1f, behaviorEvaluationInterval);
        nemesisProfile.SetMinimumBehaviorConfidence(minimumBehaviorConfidence);
        nemesisProfile.GenerateProfile();

        if (enableAIDebug)
        {
            NemesisController activeController = currentNemesisObject != null ? currentNemesisObject.GetComponent<NemesisController>() : null;
            Debug.Log($"NEMESIS LEARNING | Confidence {behaviorTracker.BehaviorConfidence:0.00} | Aggression {behaviorTracker.AggressionScore:0.00} | Ranged {behaviorTracker.RangedScore:0.00} | Mobility {behaviorTracker.MobilityScore:0.00} | Ability {behaviorTracker.AbilityUsageScore:0.00} | Stationary {behaviorTracker.StationaryScore:0.00} | Dodge {behaviorTracker.DodgeScore:0.00} | Target {nemesisProfile.SelectedNemesis} | Active {(activeController != null ? activeController.GetNemesisType().ToString() : "None")} | Adaptation {(activeController != null ? activeController.AdaptationLevel : 0f):0.00}");
        }

        if (currentNemesisObject != null)
        {
            NemesisController activeController = currentNemesisObject.GetComponent<NemesisController>();
            if (activeController != null)
                activeController.SetAdaptiveTarget(nemesisProfile.SelectedNemesis, behaviorTracker.BehaviorConfidence, adaptationStrength, profileLockDuration);
        }
    }

    private void BeginNemesisSpawn()
    {
        if (spawnWarningInProgress)
            return;

        float warningDuration = Mathf.Min(
            spawnWarningDuration,
            Mathf.Max(0f, nextNemesisTime - runManager.ElapsedTime)
        );

        if (warningDuration <= 0f)
        {
            spawnWarningInProgress = true;
            CompleteScheduledSpawn();
            return;
        }

        spawnWarningInProgress = true;
        StartCoroutine(SpawnAfterWarning(warningDuration));
    }

    private IEnumerator SpawnAfterWarning(float warningDuration)
    {
        if (debugNemesis)
            Debug.Log($"[NEMESIS] Warning triggered at run time: {runManager.ElapsedTime:0.00}s");

        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayNemesisWarning();
        SetWarningMessage(warningStartMessage);
        yield return new WaitForSeconds(warningDuration * 0.5f);
        SetWarningMessage(warningApproachingMessage);
        yield return new WaitForSeconds(warningDuration * 0.45f);
        SetWarningMessage(warningIncomingMessage);
        yield return new WaitForSeconds(warningDuration * 0.05f);

        // A long encounter may outlive the next scheduled spawn time. Keep
        // the single pending timer and spawn as soon as the previous Nemesis dies.
        while (runManager != null && runManager.RunInitialized && runManager.RunActive &&
               (Time.timeScale <= 0f || HasLivingNemesis()))
        {
            yield return null;
        }

        if (runManager == null || !runManager.RunInitialized || !runManager.RunActive)
        {
            if (warningText != null)
                warningText.gameObject.SetActive(false);
            spawnWarningInProgress = false;
            yield break;
        }

        if (warningText != null)
            warningText.gameObject.SetActive(false);

        CompleteScheduledSpawn();
    }

    private void SetWarningMessage(string message)
    {
        if (warningText != null)
        {
            warningText.gameObject.SetActive(true);
            warningText.text = message;
        }
        else
        {
            Debug.Log(message);
        }
    }

    private bool FindPlayer()
    {
        // Search loaded active objects and scope the result to this scene.
        // FindGameObjectWithTag can return a tagged object from another
        // loaded scene, which is unsafe during scene transitions/restarts.
        Transform[] candidates = FindObjectsByType<Transform>(FindObjectsSortMode.None);
        player = null;
        foreach (Transform candidate in candidates)
        {
            if (IsCurrentPlayer(candidate))
            {
                player = candidate;
                break;
            }
        }

        if (!IsCurrentPlayer(player))
        {
            behaviorTracker = null;
            nemesisProfile = null;
            return false;
        }

        behaviorTracker =
            player.GetComponent<PlayerBehaviorTracker>();

        nemesisProfile =
            player.GetComponent<NemesisProfile>();

        return true;
    }

    private bool IsCurrentPlayer(Transform candidate)
    {
        return candidate != null &&
               candidate.gameObject.activeInHierarchy &&
               candidate.CompareTag("Player") &&
               candidate.gameObject.scene == gameObject.scene;
    }

    private bool HasLivingNemesis()
    {
        if (currentNemesisObject == null)
            return false;

        if (!currentNemesisObject.activeInHierarchy)
            return false;

        EnemyHealth health = currentNemesisObject.GetComponent<EnemyHealth>();
        return health == null || !health.IsDead();
    }

    private void CompleteScheduledSpawn()
    {
        bool spawned = GenerateAndSpawnNemesis();
        spawnWarningInProgress = false;

        if (spawned)
        {
            // The interval is measured from this successful spawn event.
            // This keeps a delayed encounter from making the following
            // encounter arrive immediately after it.
            nextNemesisTime = runManager.ElapsedTime + runManager.NemesisInterval;
            if (debugNemesis)
                Debug.Log($"[NEMESIS] Next scheduled spawn: {nextNemesisTime:0.00}s");
        }
        else
        {
            // A missing prefab/target should not cause a warning coroutine to
            // restart every frame. Retry one interval from the failed attempt.
            nextNemesisTime = runManager.ElapsedTime + runManager.NemesisInterval;
            if (debugNemesis)
                Debug.LogWarning($"[NEMESIS] Spawn failed; retry scheduled for {nextNemesisTime:0.00}s");
        }
    }

    private bool GenerateAndSpawnNemesis()
    {
        if (HasLivingNemesis())
            return false;

        currentNemesisObject = null;

        // Resolve the active player again at the actual spawn moment so this
        // can never use a stale transform from an earlier run.
        if (!FindPlayer())
        {
            Debug.LogError("NEMESIS MANAGER: Active Player in the current scene was not found!");
            return false;
        }

        if (behaviorTracker == null)
        {
            Debug.LogError(
                "NEMESIS MANAGER: " +
                "PlayerBehaviorTracker missing!"
            );

            return false;
        }

        if (nemesisProfile == null)
        {
            Debug.LogError(
                "NEMESIS MANAGER: " +
                "NemesisProfile missing!"
            );

            return false;
        }

        Debug.Log(
            "================================"
        );

        Debug.Log(
            "NEMESIS EVENT #" +
            (nemesisCount + 1)
        );

        Debug.Log(
            "GAME TIME: " +
            runManager.ElapsedTime.ToString("0.0") +
            " seconds"
        );

        /*
         * Analyze how the player has been playing.
         */
        nemesisProfile.GenerateProfile();

        currentNemesis =
            nemesisProfile.SelectedNemesis;

        if (currentNemesis == lastSpawnedNemesis && repeatedNemesisCount >= maxRecentNemesisRepeat)
        {
            if (Random.value < nemesisVariationStrength)
                currentNemesis = GetVariationType(currentNemesis);
            else
                currentNemesis = currentNemesis == NemesisProfile.NemesisType.Balanced
                    ? NemesisProfile.NemesisType.Interceptor
                    : NemesisProfile.NemesisType.Balanced;
        }

        Debug.Log(
            "NEW NEMESIS: " +
            currentNemesis
        );

        int upcomingNemesisCount = nemesisCount + 1;
        GameObject spawnedNemesis = SpawnNemesis(upcomingNemesisCount);
        if (spawnedNemesis == null)
            return false;

        currentNemesisObject = spawnedNemesis;
        if (currentNemesis == lastSpawnedNemesis)
            repeatedNemesisCount++;
        else
            repeatedNemesisCount = 1;
        lastSpawnedNemesis = currentNemesis;
        nemesisCount = upcomingNemesisCount;

        Debug.Log(
            "================================"
        );

        return true;
    }

    private GameObject SpawnNemesis(int upcomingNemesisCount)
    {
        if (nemesisPrefab == null)
        {
            Debug.LogError(
                "NEMESIS MANAGER: " +
                "Nemesis Prefab is missing!"
            );

            return null;
        }

        if (!FindPlayer())
        {
            Debug.LogError(
                "NEMESIS MANAGER: " +
                "Active Player in the current scene is missing!"
            );

            return null;
        }

        Vector2 spawnDirection =
            Random.insideUnitCircle.normalized;

        if (
            spawnDirection.sqrMagnitude <
            0.01f
        )
        {
            spawnDirection =
                Vector2.right;
        }

        Vector3 playerPosition = player.position;
        float actualSpawnDistance = Mathf.Clamp(spawnDistance, 7f, 10f);
        Vector3 spawnPosition = playerPosition +
                                new Vector3(spawnDirection.x, spawnDirection.y, 0f) * actualSpawnDistance;

        GameObject spawnedNemesis = Instantiate(
            nemesisPrefab,
            spawnPosition,
            Quaternion.identity
        );

        if (spawnedNemesis == null)
            return null;

        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayNemesisSpawn();

        NemesisController controller =
            spawnedNemesis
                .GetComponent<NemesisController>();

        if (controller != null)
        {
            controller.SetPlayerTarget(player);
            if (debugNemesis)
                Debug.Log("[NEMESIS] Target assigned: Player");

            NemesisController.NemesisType controllerType =
                ConvertNemesisType(
                    currentNemesis
                );

            controller.SetNemesisType(
                controllerType
            );

            float escalation = Mathf.Min(1f + Mathf.Max(0, upcomingNemesisCount - 1) * healthMultiplierPerEncounter, maximumEscalationMultiplier);
            float speedEscalation = Mathf.Min(1f + Mathf.Max(0, upcomingNemesisCount - 1) * speedMultiplierPerEncounter, maximumEscalationMultiplier);
            float damageEscalation = Mathf.Min(1f + Mathf.Max(0, upcomingNemesisCount - 1) * damageMultiplierPerEncounter, maximumEscalationMultiplier);
            float predictionEscalation = Mathf.Min(1f + Mathf.Max(0, upcomingNemesisCount - 1) * predictionMultiplierPerEncounter, maximumEscalationMultiplier);
            controller.SetEncounterScaling(escalation, speedEscalation, damageEscalation, predictionEscalation);
            controller.ConfigureLearning(adaptationStrength, profileLockDuration, behaviorTracker.BehaviorConfidence);
        }
        else
        {
            Debug.LogError("NEMESIS MANAGER: Spawned prefab is missing NemesisController!");
            Destroy(spawnedNemesis);
            return null;
        }

        if (debugNemesis)
        {
            Debug.Log($"[NEMESIS] Spawn triggered at run time: {runManager.ElapsedTime:0.00}s");
            Debug.Log($"[NEMESIS] Spawn position: {spawnPosition.x:0.00},{spawnPosition.y:0.00}");
            Debug.Log($"[NEMESIS] Player position: {playerPosition.x:0.00},{playerPosition.y:0.00}");
            Debug.Log($"[NEMESIS] Distance from player: {Vector2.Distance(spawnPosition, playerPosition):0.00}");
        }

        Debug.Log(
            "===== NEMESIS SPAWNED =====\n" +
            "TYPE: " +
            currentNemesis +
            "\nGAME TIME: " +
            runManager.ElapsedTime.ToString("0.0") +
            "s"
        );

        return spawnedNemesis;
    }

    private NemesisProfile.NemesisType GetVariationType(NemesisProfile.NemesisType type)
    {
        switch (type)
        {
            case NemesisProfile.NemesisType.Trapper: return NemesisProfile.NemesisType.Interceptor;
            case NemesisProfile.NemesisType.Hunter: return NemesisProfile.NemesisType.Balanced;
            case NemesisProfile.NemesisType.Interceptor: return NemesisProfile.NemesisType.Hunter;
            case NemesisProfile.NemesisType.Disruptor: return NemesisProfile.NemesisType.Trapper;
            default: return NemesisProfile.NemesisType.Interceptor;
        }
    }

    private NemesisController.NemesisType ConvertNemesisType(
        NemesisProfile.NemesisType profileType
    )
    {
        switch (profileType)
        {
            case NemesisProfile.NemesisType.Trapper:
                return
                    NemesisController.NemesisType.Trapper;

            case NemesisProfile.NemesisType.Hunter:
                return
                    NemesisController.NemesisType.Hunter;

            case NemesisProfile.NemesisType.Interceptor:
                return
                    NemesisController.NemesisType.Interceptor;

            case NemesisProfile.NemesisType.Disruptor:
                return
                    NemesisController.NemesisType.Disruptor;

            default:
                return
                    NemesisController.NemesisType.Balanced;
        }
    }

    public NemesisProfile.NemesisType GetCurrentNemesis()
    {
        return currentNemesis;
    }

    public int GetNemesisCount()
    {
        return nemesisCount;
    }

    public GameObject GetCurrentNemesisObject()
    {
        return currentNemesisObject;
    }
}
