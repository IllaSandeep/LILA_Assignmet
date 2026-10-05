using UnityEngine;

public class PermanentUpgradeManager : MonoBehaviour
{
    private const string DamageLevelKey =
        "PERM_DAMAGE_LEVEL";

    private const string FireRateLevelKey =
        "PERM_FIRE_RATE_LEVEL";

    private const string MoveSpeedLevelKey =
        "PERM_MOVE_SPEED_LEVEL";

    private const string MaxHealthLevelKey =
        "PERM_MAX_HEALTH_LEVEL";

    [Header("Upgrade Limits")]
    [SerializeField] private int maxUpgradeLevel = 5;

    [Header("Costs")]
    [SerializeField] private int damageBaseCost = 100;
    [SerializeField] private int fireRateBaseCost = 150;
    [SerializeField] private int moveSpeedBaseCost = 150;
    [SerializeField] private int maxHealthBaseCost = 200;

    [Header("Upgrade Per Level")]
    [SerializeField] private float damageIncreasePercent = 0.15f;
    [SerializeField] private float fireRateIncreasePercent = 0.10f;
    [SerializeField] private float moveSpeedIncreasePercent = 0.10f;
    [SerializeField] private float maxHealthIncreasePercent = 0.15f;

    private ScrapManager scrapManager;

    public int DamageLevel =>
        PlayerPrefs.GetInt(DamageLevelKey, 0);

    public int FireRateLevel =>
        PlayerPrefs.GetInt(FireRateLevelKey, 0);

    public int MoveSpeedLevel =>
        PlayerPrefs.GetInt(MoveSpeedLevelKey, 0);

    public int MaxHealthLevel =>
        PlayerPrefs.GetInt(MaxHealthLevelKey, 0);

    private void Awake()
    {
        scrapManager =
            FindFirstObjectByType<ScrapManager>();

        if (scrapManager == null)
        {
            Debug.LogError(
                "PermanentUpgradeManager: ScrapManager not found!"
            );
        }
    }

    public int GetDamageCost()
    {
        return CalculateCost(
            damageBaseCost,
            DamageLevel
        );
    }

    public int GetFireRateCost()
    {
        return CalculateCost(
            fireRateBaseCost,
            FireRateLevel
        );
    }

    public int GetMoveSpeedCost()
    {
        return CalculateCost(
            moveSpeedBaseCost,
            MoveSpeedLevel
        );
    }

    public int GetMaxHealthCost()
    {
        return CalculateCost(
            maxHealthBaseCost,
            MaxHealthLevel
        );
    }

    private int CalculateCost(
        int baseCost,
        int currentLevel
    )
    {
        return baseCost *
               (currentLevel + 1);
    }

    public bool UpgradeDamage()
    {
        if (DamageLevel >= maxUpgradeLevel)
            return false;

        int cost =
            GetDamageCost();

        if (!TrySpendScrap(cost))
            return false;

        PlayerPrefs.SetInt(
            DamageLevelKey,
            DamageLevel + 1
        );

        PlayerPrefs.Save();

        Debug.Log(
            "PERMANENT DAMAGE UPGRADE: Level " +
            DamageLevel
        );

        return true;
    }

    public bool UpgradeFireRate()
    {
        if (FireRateLevel >= maxUpgradeLevel)
            return false;

        int cost =
            GetFireRateCost();

        if (!TrySpendScrap(cost))
            return false;

        PlayerPrefs.SetInt(
            FireRateLevelKey,
            FireRateLevel + 1
        );

        PlayerPrefs.Save();

        Debug.Log(
            "PERMANENT FIRE RATE UPGRADE: Level " +
            FireRateLevel
        );

        return true;
    }

    public bool UpgradeMoveSpeed()
    {
        if (MoveSpeedLevel >= maxUpgradeLevel)
            return false;

        int cost =
            GetMoveSpeedCost();

        if (!TrySpendScrap(cost))
            return false;

        PlayerPrefs.SetInt(
            MoveSpeedLevelKey,
            MoveSpeedLevel + 1
        );

        PlayerPrefs.Save();

        Debug.Log(
            "PERMANENT MOVE SPEED UPGRADE: Level " +
            MoveSpeedLevel
        );

        return true;
    }

    public bool UpgradeMaxHealth()
    {
        if (MaxHealthLevel >= maxUpgradeLevel)
            return false;

        int cost =
            GetMaxHealthCost();

        if (!TrySpendScrap(cost))
            return false;

        PlayerPrefs.SetInt(
            MaxHealthLevelKey,
            MaxHealthLevel + 1
        );

        PlayerPrefs.Save();

        Debug.Log(
            "PERMANENT MAX HEALTH UPGRADE: Level " +
            MaxHealthLevel
        );

        return true;
    }

    private bool TrySpendScrap(int amount)
    {
        if (scrapManager == null)
            return false;

        return scrapManager.SpendScrap(amount);
    }

    public float GetDamageMultiplier()
    {
        return 1f +
               DamageLevel *
               damageIncreasePercent;
    }

    public float GetFireRateMultiplier()
    {
        return 1f +
               FireRateLevel *
               fireRateIncreasePercent;
    }

    public float GetMoveSpeedMultiplier()
    {
        return 1f +
               MoveSpeedLevel *
               moveSpeedIncreasePercent;
    }

    public float GetMaxHealthMultiplier()
    {
        return 1f +
               MaxHealthLevel *
               maxHealthIncreasePercent;
    }

    public bool IsDamageMaxed()
    {
        return DamageLevel >= maxUpgradeLevel;
    }

    public bool IsFireRateMaxed()
    {
        return FireRateLevel >= maxUpgradeLevel;
    }

    public bool IsMoveSpeedMaxed()
    {
        return MoveSpeedLevel >= maxUpgradeLevel;
    }

    public bool IsMaxHealthMaxed()
    {
        return MaxHealthLevel >= maxUpgradeLevel;
    }

    public void ResetAllUpgrades()
    {
        PlayerPrefs.DeleteKey(DamageLevelKey);
        PlayerPrefs.DeleteKey(FireRateLevelKey);
        PlayerPrefs.DeleteKey(MoveSpeedLevelKey);
        PlayerPrefs.DeleteKey(MaxHealthLevelKey);

        PlayerPrefs.Save();

        Debug.Log(
            "ALL PERMANENT UPGRADES RESET."
        );
    }
}