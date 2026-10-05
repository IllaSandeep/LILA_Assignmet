using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerExperience : MonoBehaviour
{
    [Header("Experience")]
    [SerializeField] private int level = 1;
    [SerializeField] private int currentXP = 0;

    [Header("XP Progression")]
    [SerializeField] private float baseXP = 50f;
    [SerializeField] private float growthPower = 1.45f;

    [Header("UI")]
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text xpText;
    [SerializeField] private Slider xpBar;

    private int xpToNextLevel;

    private UpgradeSpawner upgradeSpawner;
    private PlayerAbilitySystem abilitySystem;

    private void Awake()
    {
        abilitySystem =
            GetComponent<PlayerAbilitySystem>();

        xpToNextLevel =
            CalculateXPRequired(level);
    }

    private void Start()
    {
        upgradeSpawner =
            FindFirstObjectByType<UpgradeSpawner>();

        UpdateUI();
    }

    public void AddXP(int amount)
    {
        if (amount <= 0)
            return;

        currentXP += amount;

        while (currentXP >= xpToNextLevel)
        {
            currentXP -= xpToNextLevel;

            LevelUp();
        }

        UpdateUI();
    }

    private void LevelUp()
    {
        level++;

        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayLevelUp();

        xpToNextLevel =
            CalculateXPRequired(level);

        Debug.Log(
            "LEVEL UP! Level: " +
            level +
            " | XP Required: " +
            xpToNextLevel
        );

        ShowAbilityChoices();
    }

    private int CalculateXPRequired(int currentLevel)
    {
        if (currentLevel <= 1)
            return Mathf.RoundToInt(baseXP);

        float requiredXP =
            baseXP *
            Mathf.Pow(
                currentLevel,
                growthPower
            );

        return Mathf.Max(
            1,
            Mathf.RoundToInt(requiredXP)
        );
    }

    private void ShowAbilityChoices()
    {
        if (upgradeSpawner != null)
        {
            upgradeSpawner.SpawnUpgradeChoices();
        }
    }

    public void ApplyUpgradeFromPickup(
        string abilityType
    )
    {
        Debug.Log(
            "Ability selected: " +
            abilityType
        );

        ActivateAbility(
            abilityType
        );
    }

    private void ActivateAbility(
        string abilityType
    )
    {
        if (abilitySystem == null)
        {
            abilitySystem =
                GetComponent<PlayerAbilitySystem>();
        }

        if (abilitySystem == null)
        {
            Debug.LogError(
                "PlayerAbilitySystem is missing from Player!"
            );

            return;
        }

        abilitySystem.ActivateAbility(
            abilityType
        );
    }

    public int GetCurrentLevel()
    {
        return level;
    }

    public int GetCurrentXP()
    {
        return currentXP;
    }

    public int GetXPToNextLevel()
    {
        return xpToNextLevel;
    }

    private void UpdateUI()
    {
        if (levelText != null)
        {
            levelText.text =
                "LEVEL " +
                level;
        }

        if (xpText != null)
        {
            xpText.text =
                "XP: " +
                currentXP +
                " / " +
                xpToNextLevel;
        }

        if (xpBar != null)
        {
            xpBar.maxValue =
                xpToNextLevel;

            xpBar.value =
                currentXP;
        }
    }
}
