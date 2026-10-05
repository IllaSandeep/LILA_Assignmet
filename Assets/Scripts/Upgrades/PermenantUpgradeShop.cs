using UnityEngine;

public class PermanentUpgradeShop : MonoBehaviour
{
    private PermanentUpgradeManager upgradeManager;

    private void Start()
    {
        upgradeManager =
            FindFirstObjectByType<PermanentUpgradeManager>();

        if (upgradeManager == null)
        {
            Debug.LogError(
                "PermanentUpgradeShop: PermanentUpgradeManager not found!"
            );
        }
    }

    public void BuyDamage()
    {
        if (upgradeManager == null)
            return;

        bool success =
            upgradeManager.UpgradeDamage();

        Debug.Log(
            success
                ? "DAMAGE UPGRADE PURCHASED"
                : "DAMAGE UPGRADE FAILED"
        );
    }

    public void BuyFireRate()
    {
        if (upgradeManager == null)
            return;

        bool success =
            upgradeManager.UpgradeFireRate();

        Debug.Log(
            success
                ? "FIRE RATE UPGRADE PURCHASED"
                : "FIRE RATE UPGRADE FAILED"
        );
    }

    public void BuyMoveSpeed()
    {
        if (upgradeManager == null)
            return;

        bool success =
            upgradeManager.UpgradeMoveSpeed();

        Debug.Log(
            success
                ? "MOVE SPEED UPGRADE PURCHASED"
                : "MOVE SPEED UPGRADE FAILED"
        );
    }

    public void BuyMaxHealth()
    {
        if (upgradeManager == null)
            return;

        bool success =
            upgradeManager.UpgradeMaxHealth();

        Debug.Log(
            success
                ? "MAX HEALTH UPGRADE PURCHASED"
                : "MAX HEALTH UPGRADE FAILED"
        );
    }
}