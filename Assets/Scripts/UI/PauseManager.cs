using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject gameOverPanel;
    [Tooltip("Optional existing Settings panel. The Settings button stays disabled when this is unassigned.")]
    [SerializeField] private GameObject settingsPanel;

    [Header("Pause Buttons")]
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button pauseSettingsButton;
    [SerializeField] private Button pauseMainMenuButton;

    [Header("Game Over Buttons")]
    [SerializeField] private Button restartButton;
    [SerializeField] private Button gameOverMainMenuButton;
    [Tooltip("Prototype mock: restores one life and resumes this run. Replace with rewarded ads later.")]
    [SerializeField] private Button watchAdButton;

    [Header("Game Over Stats")]
    [SerializeField] private TMP_Text runTimeText;
    [SerializeField] private TMP_Text killsText;
    [SerializeField] private TMP_Text scrapEarnedText;
    [SerializeField] private TMP_Text finalLevelText;

    private RunManager runManager;
    private PlayerExperience playerExperience;
    private PlayerHealth playerHealth;
    private bool isPaused;
    private bool isGameOver;

    public bool IsPaused => isPaused;

    private void Awake()
    {
        Time.timeScale = 1f;

        runManager = FindFirstObjectByType<RunManager>();
        playerExperience = FindFirstObjectByType<PlayerExperience>();
        playerHealth = FindFirstObjectByType<PlayerHealth>();

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (pausePanel != null && pauseSettingsButton == null)
            pauseSettingsButton = CreateButton(pausePanel.transform, "SettingsButton", "SETTINGS", 0f, -80f);

        BindButtons();
    }

    private void OnEnable()
    {
        if (runManager != null)
            runManager.RunEnded += HandleRunEnded;
    }

    private void OnDisable()
    {
        if (runManager != null)
            runManager.RunEnded -= HandleRunEnded;
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;
    }

    private void Update()
    {
        if (isGameOver || Keyboard.current == null ||
            !Keyboard.current.escapeKey.wasPressedThisFrame)
            return;

        if (settingsPanel != null && settingsPanel.activeSelf)
        {
            CloseSettings();
            return;
        }

        TogglePause();
    }

    private void BindButtons()
    {
        if (pauseButton != null)
            pauseButton.onClick.AddListener(PauseGame);

        if (resumeButton != null)
            resumeButton.onClick.AddListener(ResumeGame);

        if (pauseSettingsButton != null)
        {
            pauseSettingsButton.interactable = settingsPanel != null;
            pauseSettingsButton.onClick.AddListener(OpenSettings);
        }

        if (pauseMainMenuButton != null)
            pauseMainMenuButton.onClick.AddListener(LoadMainMenu);

        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);

        if (gameOverMainMenuButton != null)
            gameOverMainMenuButton.onClick.AddListener(LoadMainMenu);

        if (watchAdButton != null)
            watchAdButton.onClick.AddListener(ResumeAfterMockAd);
    }

    private TMP_Text CreateLabel(Transform parent, string objectName, string value, float y, float fontSize)
    {
        GameObject labelObject = new GameObject(objectName, typeof(RectTransform), typeof(TextMeshProUGUI));
        RectTransform rect = (RectTransform)labelObject.transform;
        rect.SetParent(parent, false);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0f, y);
        rect.sizeDelta = new Vector2(700f, 70f);

        TextMeshProUGUI label = labelObject.GetComponent<TextMeshProUGUI>();
        label.text = value;
        label.fontSize = fontSize;
        label.alignment = TextAlignmentOptions.Center;
        label.raycastTarget = false;
        return label;
    }

    private Button CreateButton(Transform parent, string objectName, string label, float x, float y)
    {
        GameObject buttonObject = new GameObject(objectName, typeof(RectTransform), typeof(Image), typeof(Button));
        RectTransform rect = (RectTransform)buttonObject.transform;
        rect.SetParent(parent, false);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(x, y);
        rect.sizeDelta = new Vector2(300f, 78f);

        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(0.08f, 0.18f, 0.3f, 1f);
        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;
        CreateLabel(buttonObject.transform, "Label", label, 0f, 26f);
        return button;
    }

    public void TogglePause()
    {
        if (isGameOver)
            return;

        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }

    public void PauseGame()
    {
        if (isGameOver || isPaused ||
            (runManager != null && !runManager.RunActive))
            return;

        isPaused = true;
        Time.timeScale = 0f;

        if (pausePanel != null)
            pausePanel.SetActive(true);

        Debug.Log("GAME PAUSED");
    }

    public void ResumeGame()
    {
        if (isGameOver || !isPaused)
            return;

        isPaused = false;
        Time.timeScale = 1f;

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (pausePanel != null)
            pausePanel.SetActive(false);

        Debug.Log("GAME RESUMED");
    }

    private void OpenSettings()
    {
        if (settingsPanel == null || isGameOver)
            return;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (isPaused && !isGameOver && pausePanel != null)
            pausePanel.SetActive(true);
    }

    private void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneLoader.LoadMainMenuScene();
    }

    private void RestartGame()
    {
        Time.timeScale = 1f;
        SceneLoader.LoadGameScene();
    }

    private void ResumeAfterMockAd()
    {
        if (!isGameOver || runManager == null || playerHealth == null)
            return;

        playerHealth.ReviveFromMockAd();
        runManager.ResumeAfterMockAd();
        isGameOver = false;
        Time.timeScale = 1f;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        Debug.Log("MOCK WATCH AD: restored one life and resumed the run.");
    }

    private void HandleRunEnded()
    {
        isGameOver = true;
        isPaused = false;
        Time.timeScale = 0f;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        UpdateGameOverStats();
    }

    private void UpdateGameOverStats()
    {
        if (runManager != null)
        {
            int totalSeconds = Mathf.FloorToInt(runManager.ElapsedTime);
            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;

            if (runTimeText != null)
                runTimeText.text = "" + minutes.ToString("00") + ":" + seconds.ToString("00");

            if (killsText != null)
                killsText.text = "" + runManager.EnemiesKilled;

            if (scrapEarnedText != null)
                scrapEarnedText.text = "+" + runManager.ScrapEarnedThisRun;
        }

        if (finalLevelText != null)
        {
            int level = playerExperience != null
                ? playerExperience.GetCurrentLevel()
                : 1;
            finalLevelText.text = "" + level;
        }
    }
}
