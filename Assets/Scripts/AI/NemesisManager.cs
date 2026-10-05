using UnityEngine;
using System.Collections;
using TMPro;

public class NemesisManager : MonoBehaviour
{
    [Header("Nemesis Prefab")]
    [SerializeField] private GameObject nemesisPrefab;

    [Header("Spawn")]
    [SerializeField] private float spawnDistance = 10f;
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

    [Header("Player")]
    [SerializeField] private Transform player;

    private PlayerBehaviorTracker behaviorTracker;
    private NemesisProfile nemesisProfile;

    private RunManager runManager;

    private NemesisProfile.NemesisType currentNemesis =
        NemesisProfile.NemesisType.Balanced;

    private GameObject currentNemesisObject;

    private float nextNemesisTime;
    private int nemesisCount;
    private bool spawnWarningInProgress;
    private float nextBehaviorEvaluationTime;
    private NemesisProfile.NemesisType lastSpawnedNemesis = NemesisProfile.NemesisType.Balanced;
    private int repeatedNemesisCount;

    private void Start()
    {
        FindPlayer();

        runManager =
            FindFirstObjectByType<RunManager>();

        if (runManager == null)
        {
            Debug.LogError(
                "NEMESIS MANAGER: RunManager NOT FOUND!"
            );

            return;
        }

        nextNemesisTime =
            runManager.FirstNemesisSpawnTime;
        nextBehaviorEvaluationTime = Mathf.Max(behaviorEvaluationInterval, nextNemesisTime);

        Debug.Log(
            "===== NEMESIS MANAGER STARTED =====\n" +
            "First Nemesis at: " +
            nextNemesisTime +
            " seconds"
        );
    }

    private void Update()
    {
        if (Time.timeScale <= 0f)
            return;

        if (runManager == null)
        {
            runManager =
                FindFirstObjectByType<RunManager>();

            return;
        }

        if (player == null)
        {
            FindPlayer();
            return;
        }

        EvaluatePlayerBehaviorIfDue();

        /*
         * Do not spawn another Nemesis while the
         * current Nemesis is still alive.
         */
        if (currentNemesisObject != null)
        {
            EnemyHealth currentHealth =
                currentNemesisObject
                    .GetComponent<EnemyHealth>();

            if (
                currentHealth != null &&
                !currentHealth.IsDead()
            )
            {
                return;
            }

            /*
             * The previous Nemesis has died.
             * The object will be destroyed by EnemyHealth,
             * so clear our reference.
             */
            currentNemesisObject = null;

            /*
             * Schedule the next Nemesis relative to
             * the current run time.
             */
            nextNemesisTime =
                runManager.ElapsedTime +
                runManager.NemesisInterval;
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

        if (spawnWarningDuration <= 0f)
        {
            GenerateAndSpawnNemesis();
            return;
        }

        StartCoroutine(SpawnAfterWarning());
    }

    private IEnumerator SpawnAfterWarning()
    {
        spawnWarningInProgress = true;
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayNemesisWarning();
        SetWarningMessage(warningStartMessage);
        yield return new WaitForSeconds(spawnWarningDuration * 0.5f);
        SetWarningMessage(warningApproachingMessage);
        yield return new WaitForSeconds(spawnWarningDuration * 0.45f);
        SetWarningMessage(warningIncomingMessage);
        yield return new WaitForSeconds(spawnWarningDuration * 0.05f);

        if (warningText != null)
            warningText.gameObject.SetActive(false);
        spawnWarningInProgress = false;
        GenerateAndSpawnNemesis();
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

        behaviorTracker =
            playerObject.GetComponent<PlayerBehaviorTracker>();

        nemesisProfile =
            playerObject.GetComponent<NemesisProfile>();
    }

    private void GenerateAndSpawnNemesis()
    {
        /*
         * Safety check.
         * Never create a second Nemesis if one
         * already exists.
         */
        if (currentNemesisObject != null)
        {
            EnemyHealth currentHealth =
                currentNemesisObject
                    .GetComponent<EnemyHealth>();

            if (
                currentHealth != null &&
                !currentHealth.IsDead()
            )
            {
                return;
            }

            currentNemesisObject = null;
        }

        if (player == null)
            FindPlayer();

        if (behaviorTracker == null)
        {
            Debug.LogError(
                "NEMESIS MANAGER: " +
                "PlayerBehaviorTracker missing!"
            );

            return;
        }

        if (nemesisProfile == null)
        {
            Debug.LogError(
                "NEMESIS MANAGER: " +
                "NemesisProfile missing!"
            );

            return;
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

        if (currentNemesis == lastSpawnedNemesis)
            repeatedNemesisCount++;
        else
            repeatedNemesisCount = 1;
        lastSpawnedNemesis = currentNemesis;

        nemesisCount++;

        Debug.Log(
            "NEW NEMESIS: " +
            currentNemesis
        );

        /*
         * Spawn the new Nemesis.
         */
        SpawnNemesis();

        /*
         * We don't immediately schedule another Nemesis.
         * The next one will be scheduled only after
         * this Nemesis is defeated.
         */

        Debug.Log(
            "================================"
        );
    }

    private void SpawnNemesis()
    {
        if (nemesisPrefab == null)
        {
            Debug.LogError(
                "NEMESIS MANAGER: " +
                "Nemesis Prefab is missing!"
            );

            return;
        }

        if (player == null)
        {
            Debug.LogError(
                "NEMESIS MANAGER: " +
                "Player is missing!"
            );

            return;
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

        Vector2 spawnPosition =
            (Vector2)player.position +
            spawnDirection *
            spawnDistance;

        currentNemesisObject =
            Instantiate(
                nemesisPrefab,
                spawnPosition,
                Quaternion.identity
            );

        if (currentNemesisObject != null && SoundManager.Instance != null)
            SoundManager.Instance.PlayNemesisSpawn();

        NemesisController controller =
            currentNemesisObject
                .GetComponent<NemesisController>();

        if (controller != null)
        {
            NemesisController.NemesisType controllerType =
                ConvertNemesisType(
                    currentNemesis
                );

            controller.SetNemesisType(
                controllerType
            );

            float escalation = Mathf.Min(1f + Mathf.Max(0, nemesisCount - 1) * healthMultiplierPerEncounter, maximumEscalationMultiplier);
            float speedEscalation = Mathf.Min(1f + Mathf.Max(0, nemesisCount - 1) * speedMultiplierPerEncounter, maximumEscalationMultiplier);
            float damageEscalation = Mathf.Min(1f + Mathf.Max(0, nemesisCount - 1) * damageMultiplierPerEncounter, maximumEscalationMultiplier);
            float predictionEscalation = Mathf.Min(1f + Mathf.Max(0, nemesisCount - 1) * predictionMultiplierPerEncounter, maximumEscalationMultiplier);
            controller.SetEncounterScaling(escalation, speedEscalation, damageEscalation, predictionEscalation);
            controller.ConfigureLearning(adaptationStrength, profileLockDuration, behaviorTracker.BehaviorConfidence);
        }

        Debug.Log(
            "===== NEMESIS SPAWNED =====\n" +
            "TYPE: " +
            currentNemesis +
            "\nGAME TIME: " +
            runManager.ElapsedTime.ToString("0.0") +
            "s"
        );
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
