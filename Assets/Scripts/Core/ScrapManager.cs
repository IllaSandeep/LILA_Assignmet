using UnityEngine;

public class ScrapManager : MonoBehaviour
{
    private const string ScrapKey = "PLAYER_SCRAP";

    [Header("Starting Scrap")]
    [SerializeField] private int startingScrap = 0;

    [Header("Run Rewards")]
    [SerializeField] private int scrapPerKill = 5;
    [SerializeField] private int scrapPerTenSeconds = 1;
    [SerializeField] private int scrapPerNemesis = 25;

    public int CurrentScrap { get; private set; }

    private void Awake()
    {
        LoadScrap();
    }

    private void LoadScrap()
    {
        CurrentScrap = PlayerPrefs.HasKey(ScrapKey)
            ? PlayerPrefs.GetInt(ScrapKey)
            : startingScrap;

        Debug.Log(
            "SCRAP LOADED: " +
            CurrentScrap
        );
    }

    public void AddScrap(int amount)
    {
        if (amount <= 0)
            return;

        CurrentScrap += amount;

        PlayerPrefs.SetInt(
            "PLAYER_SCRAP",
            CurrentScrap
        );

        PlayerPrefs.Save();

        Debug.Log("SCRAP COLLECTED: +" + amount);
        Debug.Log("CURRENT SCRAP: " + CurrentScrap);
        Debug.Log("SAVED SCRAP: " + PlayerPrefs.GetInt(ScrapKey));
    }

    public bool SpendScrap(int amount)
    {
        if (amount <= 0)
            return false;

        if (CurrentScrap < amount)
        {
            Debug.Log(
                "NOT ENOUGH SCRAP! " +
                "Have: " +
                CurrentScrap +
                " | Required: " +
                amount
            );

            return false;
        }

        CurrentScrap -= amount;

        SaveScrap();

        Debug.Log(
            "SCRAP SPENT: -" +
            amount +
            " | TOTAL: " +
            CurrentScrap
        );

        return true;
    }

    public int CalculateRunReward(
        int kills,
        float runTime,
        int nemesisCount
    )
    {
        int killReward =
            kills * scrapPerKill;

        int timeReward =
            Mathf.FloorToInt(
                runTime / 10f
            ) * scrapPerTenSeconds;

        int nemesisReward =
            nemesisCount * scrapPerNemesis;

        return
            killReward +
            timeReward +
            nemesisReward;
    }

    public void RewardRun(
        int kills,
        float runTime,
        int nemesisCount
    )
    {
        int reward =
            CalculateRunReward(
                kills,
                runTime,
                nemesisCount
            );

        Debug.Log(
            "===== RUN SCRAP REWARD =====\n" +
            "Kills: " +
            kills +
            "\nTime: " +
            runTime.ToString("0.0") +
            "\nNemesis: " +
            nemesisCount +
            "\nReward: +" +
            reward
        );

        AddScrap(reward);
    }

    private void SaveScrap()
    {
        PlayerPrefs.SetInt(
            ScrapKey,
            CurrentScrap
        );

        PlayerPrefs.Save();

        Debug.Log(
            "SCRAP SAVED: " +
            CurrentScrap
        );
    }

    public void ResetScrap()
    {
        CurrentScrap =
            startingScrap;

        SaveScrap();

        Debug.Log(
            "SCRAP RESET: " +
            CurrentScrap
        );
    }
}
