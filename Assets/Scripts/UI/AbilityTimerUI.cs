using UnityEngine;
using TMPro;
using System.Text;

public class AbilityTimerUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text abilityText;

    private PlayerAbilitySystem abilitySystem;

    private readonly string[] abilityTypes =
    {
        "Power",
        "RapidFire",
        "Speed",
        "HeavyShot",
        "Piercing",
        "Dash"
    };

    private void Start()
    {
        FindAbilitySystem();
        UpdateUI();
    }

    private void Update()
    {
        if (abilitySystem == null)
        {
            FindAbilitySystem();
            return;
        }

        UpdateUI();
    }

    private void FindAbilitySystem()
    {
        GameObject player =
            GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            abilitySystem =
                player.GetComponent<PlayerAbilitySystem>();
        }
    }

    private void UpdateUI()
    {
        if (abilityText == null)
            return;

        if (abilitySystem == null)
        {
            abilityText.text = "";
            return;
        }

        StringBuilder text =
            new StringBuilder();

        bool hasActiveAbility = false;

        foreach (string abilityType in abilityTypes)
        {
            if (!abilitySystem.IsAbilityActive(abilityType))
                continue;

            float remaining =
                abilitySystem.GetRemainingTime(
                    abilityType
                );

            text.AppendLine(
                GetDisplayName(abilityType) +
                "  " +
                remaining.ToString("0.0") +
                "s"
            );

            hasActiveAbility = true;
        }

        if (!hasActiveAbility)
        {
            abilityText.text = "";
            return;
        }

        abilityText.text =
            text.ToString();
    }

    private string GetDisplayName(
        string abilityType
    )
    {
        switch (abilityType)
        {
            case "Power":
                return "POWER SURGE";

            case "RapidFire":
                return "RAPID FIRE";

            case "Speed":
                return "SPEED BOOST";

            case "HeavyShot":
                return "HEAVY SHOT";

            case "Piercing":
                return "PIERCING";

            case "Dash":
                return "DASH";

            default:
                return abilityType.ToUpper();
        }
    }
}