using UnityEngine;
using TMPro;
using System;

public class RunManager : MonoBehaviour
{
    [Header("Run")]
    [Tooltip("Set to 0 for an unlimited run.")]
    [SerializeField] private float runDuration = 0f;

    [Header("Nemesis")]
    [Min(0f)] [SerializeField] private float firstNemesisSpawnTime = 50f;
    [Min(1f)] [SerializeField] private float nemesisInterval = 55f;
    [Min(1f)] [SerializeField] private float minimumNemesisInterval = 45f;

    [Header("Streak")]
    [SerializeField] private float streakTimeout = 4f;

    [Header("UI")]
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text killsText;
    [SerializeField] private TMP_Text streakText;

    private float elapsedTime;
    private int enemiesKilled;
    private int currentStreak;
    private float streakTimer;
    private int scrapAtRunStart;
    private bool runRewardAwarded;

    private bool runActive = true;
    private bool runInitialized;

    private ScrapManager scrapManager;
    private NemesisManager nemesisManager;

    public float ElapsedTime => elapsedTime;
    public int EnemiesKilled => enemiesKilled;
    public int CurrentStreak => currentStreak;
    public float FirstNemesisSpawnTime => Mathf.Max(0f, firstNemesisSpawnTime);
    public float NemesisInterval => Mathf.Max(minimumNemesisInterval, nemesisInterval);
    public bool RunActive => runActive;
    public bool RunInitialized => runInitialized;
    public int ScrapEarnedThisRun { get; private set; }
    public event Action RunEnded;

    private void Start()
    {
        runInitialized = false;
        Time.timeScale = 1f;

        elapsedTime = 0f;
        enemiesKilled = 0;
        currentStreak = 0;
        streakTimer = 0f;
        runActive = true;
        ScrapEarnedThisRun = 0;
        runRewardAwarded = false;

        FindManagers();
        scrapAtRunStart = scrapManager != null
            ? scrapManager.CurrentScrap
            : 0;

        UpdateTimerUI();
        UpdateKillsUI();
        UpdateStreakUI();
        runInitialized = true;

        Debug.Log(
            "===== RUN STARTED =====\n" +
            "SCRAP MANAGER: " +
            (
                scrapManager != null
                    ? "FOUND"
                    : "NOT FOUND"
            ) +
            "\nNEMESIS MANAGER: " +
            (
                nemesisManager != null
                    ? "FOUND"
                    : "NOT FOUND"
            )
        );
    }

    private void Update()
    {
        if (!runActive)
            return;

        elapsedTime += Time.deltaTime;

        UpdateTimerUI();
        UpdateStreak();

        if (
            runDuration > 0f &&
            elapsedTime >= runDuration
        )
        {
            EndRun();
        }
    }

    private void FindManagers()
    {
        if (GameManager.Instance != null)
        {
            scrapManager =
                GameManager.Instance.ScrapManager;
        }

        if (nemesisManager == null)
        {
            nemesisManager =
                FindFirstObjectByType<NemesisManager>();
        }

        if (scrapManager == null)
        {
            Debug.LogError(
                "RUN MANAGER: ScrapManager is missing from GameManager!"
            );
        }
    }

    public void RegisterKill()
    {
        if (!runActive)
            return;

        enemiesKilled++;

        currentStreak++;

        streakTimer =
            streakTimeout;

        UpdateKillsUI();
        UpdateStreakUI();

        Debug.Log(
            "Enemy killed! " +
            "Kills: " +
            enemiesKilled +
            " | Streak: " +
            currentStreak
        );
    }

    public int GetStreakXPBonus()
    {
        if (currentStreak < 2)
            return 0;

        return Mathf.Min(
            currentStreak,
            20
        );
    }

    private void UpdateStreak()
    {
        if (currentStreak <= 0)
            return;

        streakTimer -=
            Time.deltaTime;

        if (streakTimer <= 0f)
        {
            currentStreak = 0;

            UpdateStreakUI();

            Debug.Log(
                "STREAK LOST"
            );
        }
    }

    private void UpdateTimerUI()
    {
        if (timerText == null)
            return;

        int minutes =
            Mathf.FloorToInt(
                elapsedTime / 60f
            );

        int seconds =
            Mathf.FloorToInt(
                elapsedTime % 60f
            );

        timerText.text =
            minutes.ToString("00") +
            ":" +
            seconds.ToString("00");
    }

    private void UpdateKillsUI()
    {
        if (killsText == null)
            return;

        killsText.text =
            "KILLS  " +
            enemiesKilled;
    }

    private void UpdateStreakUI()
    {
        if (streakText == null)
            return;

        if (currentStreak <= 0)
        {
            streakText.text = "";
            return;
        }

        streakText.text =
            "STREAK  x" +
            currentStreak;
    }

    public void EndRun()
    {
        if (!runActive)
            return;

        runActive = false;

        int nemesisCount = 0;

        if (nemesisManager != null)
        {
            nemesisCount =
                nemesisManager.GetNemesisCount();
        }

        Debug.Log(
            "===== RUN COMPLETE =====\n" +
            "TOTAL TIME: " +
            elapsedTime.ToString("0.0") +
            "s\n" +
            "TOTAL KILLS: " +
            enemiesKilled +
            "\nNEMESIS ENCOUNTERS: " +
            nemesisCount
        );

        if (!runRewardAwarded)
        {
            AwardScrap(nemesisCount);
            runRewardAwarded = true;
        }

        ScrapEarnedThisRun = scrapManager != null
            ? Mathf.Max(0, scrapManager.CurrentScrap - scrapAtRunStart)
            : 0;

        Time.timeScale = 0f;
        RunEnded?.Invoke();
    }

    public void ResumeAfterMockAd()
    {
        if (runActive)
            return;

        runActive = true;
    }

    private void AwardScrap(int nemesisCount)
    {
        if (scrapManager == null)
        {
            Debug.LogError(
                "RUN MANAGER: ScrapManager is NULL. Cannot award Scrap."
            );
            return;
        }

        int reward =
            scrapManager.CalculateRunReward(
                enemiesKilled,
                elapsedTime,
                nemesisCount
            );

        Debug.Log(
            "===== SCRAP AWARD =====\n" +
            "Current Scrap BEFORE: " +
            scrapManager.CurrentScrap +
            "\nKills: " +
            enemiesKilled +
            "\nTime: " +
            elapsedTime.ToString("0.0") +
            "\nNemesis: " +
            nemesisCount +
            "\nReward: +" +
            reward
        );

        scrapManager.AddScrap(reward);

        Debug.Log(
            "Current Scrap AFTER: " +
            scrapManager.CurrentScrap
        );
    }
}
