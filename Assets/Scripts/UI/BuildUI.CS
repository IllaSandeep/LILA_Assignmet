using UnityEngine;
using TMPro;
using System.Text;

public class BuildUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text buildText;

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
            GameObject.FindGameObjectWithTag(
                "Player"
            );

        if (player != null)
        {
            abilitySystem =
                player.GetComponent<PlayerAbilitySystem>();
        }
    }

    private void UpdateUI()
    {
        if (buildText == null)
            return;

        if (abilitySystem == null)
        {
            // buildText.text =
            //     "ACTIVE ABILITIES";
            return;
        }

        StringBuilder text =
            new StringBuilder();

        // text.AppendLine(
        //     "ACTIVE ABILITIES"
        // );

        bool hasActiveAbility = false;

        foreach (
            string abilityType
            in abilityTypes
        )
        {
            if (
                abilitySystem.IsAbilityActive(
                    abilityType
                )
            )
            {
                float remaining =
                    abilitySystem.GetRemainingTime(
                        abilityType
                    );

                text.AppendLine(
                    GetDisplayName(
                        abilityType
                    ) +
                    "  " +
                    remaining.ToString("0.0") +
                    "s"
                );

                hasActiveAbility = true;
            }
        }

        if (!hasActiveAbility)
        {
            text.AppendLine(
                "None"
            );
        }

        buildText.text =
            text.ToString();
    }

    private string GetDisplayName(
        string abilityType
    )
    {
        switch (abilityType)
        {
            case "RapidFire":
                return "RAPID FIRE";

            case "HeavyShot":
                return "HEAVY SHOT";

            case "Power":
                return "POWER SURGE";

            case "Piercing":
                return "PIERCING";

            case "Speed":
                return "SPEED BOOST";

            case "Dash":
                return "DASH";

            default:
                return abilityType.ToUpper();
        }
    }
}