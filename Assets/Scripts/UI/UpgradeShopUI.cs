using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeShopUI : MonoBehaviour
{
    [Header("Scrap")]
    [SerializeField] private TMP_Text scrapText;

    // =========================================================
    // SHARED BUTTON SPRITES
    // =========================================================

    [Header("Button Sprites")]
    [SerializeField] private Sprite upgradeSprite;
    [SerializeField] private Sprite notEnoughSprite;
    [SerializeField] private Sprite maxSprite;

    // =========================================================
    // BUTTON IMAGES
    // These are ONLY the upgrade button backgrounds.
    // Your Damage/Fire Rate/Speed/Health icons stay static.
    // =========================================================

    [Header("Upgrade Button Images")]
    [SerializeField] private Image damageButton;
    [SerializeField] private Image fireRateButton;
    [SerializeField] private Image moveSpeedButton;
    [SerializeField] private Image maxHealthButton;

    // =========================================================
    // DAMAGE
    // =========================================================

    [Header("Damage")]
    [SerializeField] private TMP_Text damageLevelText;
    [SerializeField] private TMP_Text damageCostText;

    // =========================================================
    // FIRE RATE
    // =========================================================

    [Header("Fire Rate")]
    [SerializeField] private TMP_Text fireRateLevelText;
    [SerializeField] private TMP_Text fireRateCostText;

    // =========================================================
    // MOVE SPEED
    // =========================================================

    [Header("Move Speed")]
    [SerializeField] private TMP_Text moveSpeedLevelText;
    [SerializeField] private TMP_Text moveSpeedCostText;

    // =========================================================
    // MAX HEALTH
    // =========================================================

    [Header("Max Health")]
    [SerializeField] private TMP_Text maxHealthLevelText;
    [SerializeField] private TMP_Text maxHealthCostText;

    // =========================================================
    // MANAGERS
    // =========================================================

    private ScrapManager scrapManager;
    private PermanentUpgradeManager upgradeManager;

    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        FindManagers();
    }

    private void Start()
    {
        FindManagers();
        UpdateUI();
    }

    private void OnEnable()
    {
        FindManagers();
        UpdateUI();
    }

    private void Update()
    {
        if (
            scrapManager == null ||
            upgradeManager == null
        )
        {
            FindManagers();

            if (
                scrapManager != null &&
                upgradeManager != null
            )
            {
                UpdateUI();
            }

            return;
        }

        // Keep button states updated in real time.
        UpdateButtonStates();
    }

    // =========================================================
    // FIND MANAGERS
    // =========================================================

    private void FindManagers()
    {
        if (scrapManager == null)
        {
            scrapManager =
                FindFirstObjectByType<ScrapManager>();
        }

        if (upgradeManager == null)
        {
            upgradeManager =
                FindFirstObjectByType<PermanentUpgradeManager>();
        }
    }

    // =========================================================
    // MAIN UI
    // =========================================================

    public void UpdateUI()
    {
        if (
            scrapManager == null ||
            upgradeManager == null
        )
        {
            return;
        }

        UpdateScrapUI();

        UpdateDamageUI();
        UpdateFireRateUI();
        UpdateMoveSpeedUI();
        UpdateMaxHealthUI();

        UpdateButtonStates();
    }

    // =========================================================
    // SCRAP
    // =========================================================

    private void UpdateScrapUI()
    {
        if (scrapText == null)
            return;

        scrapText.text =
            "SCRAP  " +
            scrapManager.CurrentScrap;
    }

    // =========================================================
    // DAMAGE
    // =========================================================

    private void UpdateDamageUI()
    {
        if (damageLevelText != null)
        {
            damageLevelText.text =
                "LEVEL " +
                upgradeManager.DamageLevel;
        }

        if (damageCostText != null)
        {
            if (upgradeManager.IsDamageMaxed())
            {
                damageCostText.text = "MAX";
            }
            else
            {
                damageCostText.text =
                    upgradeManager
                        .GetDamageCost()
                        .ToString();
            }
        }
    }

    // =========================================================
    // FIRE RATE
    // =========================================================

    private void UpdateFireRateUI()
    {
        if (fireRateLevelText != null)
        {
            fireRateLevelText.text =
                "LEVEL " +
                upgradeManager.FireRateLevel;
        }

        if (fireRateCostText != null)
        {
            if (upgradeManager.IsFireRateMaxed())
            {
                fireRateCostText.text = "MAX";
            }
            else
            {
                fireRateCostText.text =
                    upgradeManager
                        .GetFireRateCost()
                        .ToString();
            }
        }
    }

    // =========================================================
    // MOVE SPEED
    // =========================================================

    private void UpdateMoveSpeedUI()
    {
        if (moveSpeedLevelText != null)
        {
            moveSpeedLevelText.text =
                "LEVEL " +
                upgradeManager.MoveSpeedLevel;
        }

        if (moveSpeedCostText != null)
        {
            if (upgradeManager.IsMoveSpeedMaxed())
            {
                moveSpeedCostText.text = "MAX";
            }
            else
            {
                moveSpeedCostText.text =
                    upgradeManager
                        .GetMoveSpeedCost()
                        .ToString();
            }
        }
    }

    // =========================================================
    // MAX HEALTH
    // =========================================================

    private void UpdateMaxHealthUI()
    {
        if (maxHealthLevelText != null)
        {
            maxHealthLevelText.text =
                "LEVEL " +
                upgradeManager.MaxHealthLevel;
        }

        if (maxHealthCostText != null)
        {
            if (upgradeManager.IsMaxHealthMaxed())
            {
                maxHealthCostText.text = "MAX";
            }
            else
            {
                maxHealthCostText.text =
                    upgradeManager
                        .GetMaxHealthCost()
                        .ToString();
            }
        }
    }

    // =========================================================
    // BUTTON STATES
    // =========================================================

    private void UpdateButtonStates()
    {
        UpdateButton(
            damageButton,
            upgradeManager.IsDamageMaxed(),
            upgradeManager.GetDamageCost()
        );

        UpdateButton(
            fireRateButton,
            upgradeManager.IsFireRateMaxed(),
            upgradeManager.GetFireRateCost()
        );

        UpdateButton(
            moveSpeedButton,
            upgradeManager.IsMoveSpeedMaxed(),
            upgradeManager.GetMoveSpeedCost()
        );

        UpdateButton(
            maxHealthButton,
            upgradeManager.IsMaxHealthMaxed(),
            upgradeManager.GetMaxHealthCost()
        );
    }

    private void UpdateButton(
        Image buttonImage,
        bool isMaxed,
        int cost
    )
    {
        if (buttonImage == null)
            return;

        // MAX always wins.
        if (isMaxed)
        {
            buttonImage.sprite =
                maxSprite;

            return;
        }

        // Enough Scrap.
        if (scrapManager.CurrentScrap >= cost)
        {
            buttonImage.sprite =
                upgradeSprite;

            return;
        }

        // Not enough Scrap.
        buttonImage.sprite =
            notEnoughSprite;
    }

    // =========================================================
    // BUY DAMAGE
    // =========================================================

    public void BuyDamage()
    {
        if (upgradeManager == null)
            FindManagers();

        if (upgradeManager == null)
            return;

        upgradeManager.UpgradeDamage();

        UpdateUI();
    }

    // =========================================================
    // BUY FIRE RATE
    // =========================================================

    public void BuyFireRate()
    {
        if (upgradeManager == null)
            FindManagers();

        if (upgradeManager == null)
            return;

        upgradeManager.UpgradeFireRate();

        UpdateUI();
    }

    // =========================================================
    // BUY MOVE SPEED
    // =========================================================

    public void BuyMoveSpeed()
    {
        if (upgradeManager == null)
            FindManagers();

        if (upgradeManager == null)
            return;

        upgradeManager.UpgradeMoveSpeed();

        UpdateUI();
    }

    // =========================================================
    // BUY MAX HEALTH
    // =========================================================

    public void BuyMaxHealth()
    {
        if (upgradeManager == null)
            FindManagers();

        if (upgradeManager == null)
            return;

        upgradeManager.UpgradeMaxHealth();

        UpdateUI();
    }
}