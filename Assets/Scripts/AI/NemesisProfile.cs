using UnityEngine;

public class NemesisProfile : MonoBehaviour
{
    public enum Playstyle
    {
        Balanced,
        Aggressive,
        Ranged,
        Mobile,
        AbilityHeavy
    }

    public enum NemesisType
    {
        Balanced,
        Trapper,
        Hunter,
        Interceptor,
        Disruptor
    }

    [Header("Profile")]
    [SerializeField] private Playstyle detectedPlaystyle;
    [SerializeField] private NemesisType selectedNemesis;

    [Header("Playstyle Detection Thresholds")]
    [Tooltip("A score must exceed its threshold to influence the selected Nemesis.")]
    [Range(0f, 1f)] [SerializeField] private float aggressionThreshold = 0.55f;
    [Range(0f, 1f)] [SerializeField] private float rangedThreshold = 0.55f;
    [Range(0f, 1f)] [SerializeField] private float mobilityThreshold = 0.55f;
    [Range(0f, 1f)] [SerializeField] private float abilityUsageThreshold = 0.55f;
    [Range(0f, 1f)] [SerializeField] private float minimumBehaviorConfidence = 0.35f;

    private PlayerBehaviorTracker behaviorTracker;

    private bool profileGenerated;

    public Playstyle DetectedPlaystyle =>
        detectedPlaystyle;

    public NemesisType SelectedNemesis =>
        selectedNemesis;

    public float Confidence => behaviorTracker != null ? behaviorTracker.BehaviorConfidence : 0f;

    private void Awake()
    {
        behaviorTracker =
            GetComponent<PlayerBehaviorTracker>();
    }

    public void GenerateProfile()
    {
        if (behaviorTracker == null)
        {
            behaviorTracker =
                GetComponent<PlayerBehaviorTracker>();
        }

        if (behaviorTracker == null)
        {
            Debug.LogError(
                "NemesisProfile: PlayerBehaviorTracker is missing!"
            );

            return;
        }

        float aggression =
            behaviorTracker.GetAggressionScore();

        float ranged =
            behaviorTracker.GetRangedScore();

        float mobility =
            behaviorTracker.GetMobilityScore();

        float abilityUsage =
            behaviorTracker.GetAbilityUsageScore();

        detectedPlaystyle =
            DeterminePlaystyle(
                aggression,
                ranged,
                mobility,
                abilityUsage
            );

        selectedNemesis =
            DetermineNemesis(
                detectedPlaystyle
            );

        if (behaviorTracker.BehaviorConfidence < minimumBehaviorConfidence)
        {
            detectedPlaystyle = Playstyle.Balanced;
            selectedNemesis = NemesisType.Balanced;
        }

        profileGenerated = true;

        PrintProfile(
            aggression,
            ranged,
            mobility,
            abilityUsage
        );
    }

    public void SetMinimumBehaviorConfidence(float value)
    {
        minimumBehaviorConfidence = Mathf.Clamp01(value);
    }

    private Playstyle DeterminePlaystyle(
        float aggression,
        float ranged,
        float mobility,
        float abilityUsage
    )
    {
        float highestScore = -1f;

        Playstyle strongestStyle =
            Playstyle.Balanced;

        if (aggression > aggressionThreshold && aggression > highestScore)
        {
            highestScore =
                aggression;

            strongestStyle =
                Playstyle.Aggressive;
        }

        if (ranged > rangedThreshold && ranged > highestScore)
        {
            highestScore =
                ranged;

            strongestStyle =
                Playstyle.Ranged;
        }

        if (mobility > mobilityThreshold && mobility > highestScore)
        {
            highestScore =
                mobility;

            strongestStyle =
                Playstyle.Mobile;
        }

        if (abilityUsage > abilityUsageThreshold && abilityUsage > highestScore)
        {
            strongestStyle =
                Playstyle.AbilityHeavy;
        }

        return strongestStyle;
    }

    private NemesisType DetermineNemesis(
        Playstyle playstyle
    )
    {
        switch (playstyle)
        {
            case Playstyle.Aggressive:
                return NemesisType.Trapper;

            case Playstyle.Ranged:
                return NemesisType.Hunter;

            case Playstyle.Mobile:
                return NemesisType.Interceptor;

            case Playstyle.AbilityHeavy:
                return NemesisType.Disruptor;

            default:
                return NemesisType.Balanced;
        }
    }

    private void PrintProfile(
        float aggression,
        float ranged,
        float mobility,
        float abilityUsage
    )
    {
        Debug.Log(
            "===== NEMESIS PROFILE =====\n" +

            "Aggression: " +
            aggression.ToString("0.00") +

            "\nRanged: " +
            ranged.ToString("0.00") +

            "\nMobility: " +
            mobility.ToString("0.00") +

            "\nAbility Usage: " +
            abilityUsage.ToString("0.00") +

            "\n\nPLAYSTYLE: " +
            detectedPlaystyle +

            "\nNEMESIS: " +
            selectedNemesis
        );
    }

    public bool IsProfileGenerated()
    {
        return profileGenerated;
    }

    public string GetPlaystyleName()
    {
        return detectedPlaystyle
            .ToString()
            .ToUpper();
    }

    public string GetNemesisName()
    {
        return selectedNemesis
            .ToString()
            .ToUpper();
    }
}
