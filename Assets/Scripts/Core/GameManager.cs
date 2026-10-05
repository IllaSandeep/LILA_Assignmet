using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public ScrapManager ScrapManager { get; private set; }

    public PermanentUpgradeManager PermanentUpgradeManager
    {
        get;
        private set;
    }

    private void Awake()
    {
        if (
            Instance != null &&
            Instance != this
        )
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        ScrapManager =
            GetComponent<ScrapManager>();

        PermanentUpgradeManager =
            GetComponent<PermanentUpgradeManager>();

        if (ScrapManager == null)
        {
            Debug.LogError(
                "GAME MANAGER: ScrapManager missing!"
            );
        }

        if (PermanentUpgradeManager == null)
        {
            Debug.LogError(
                "GAME MANAGER: PermanentUpgradeManager missing!"
            );
        }

        DontDestroyOnLoad(gameObject);

        Debug.Log(
            "===== GAME MANAGER INITIALIZED =====\n" +
            "ScrapManager: " +
            (
                ScrapManager != null
                    ? "FOUND"
                    : "MISSING"
            ) +
            "\nPermanentUpgradeManager: " +
            (
                PermanentUpgradeManager != null
                    ? "FOUND"
                    : "MISSING"
            )
        );
    }
}