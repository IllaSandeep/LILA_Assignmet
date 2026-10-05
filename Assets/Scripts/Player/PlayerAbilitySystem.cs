using UnityEngine;
using System.Collections.Generic;

public class PlayerAbilitySystem : MonoBehaviour
{
    [System.Serializable]
    private class ActiveAbility
    {
        public string abilityType;
        public float remainingTime;

        public ActiveAbility(
            string type,
            float duration
        )
        {
            abilityType =
                type;

            remainingTime =
                duration;
        }
    }

    [Header("Ability Durations")]
    [SerializeField] private float powerDuration = 15f;
    [SerializeField] private float rapidFireDuration = 10f;
    [SerializeField] private float speedDuration = 12f;
    [SerializeField] private float heavyShotDuration = 12f;
    [SerializeField] private float piercingDuration = 15f;
    // [SerializeField] private float dashDuration = 15f;
    [SerializeField] private float shieldDuration = 6f;

    private List<ActiveAbility> activeAbilities =
        new List<ActiveAbility>();

    private PlayerWeapon weapon;
    private PlayerController controller;
    private PlayerHealth playerHealth;
    private PlayerBehaviorTracker behaviorTracker;

    private void Awake()
    {
        weapon =
            GetComponentInChildren<PlayerWeapon>();

        controller =
            GetComponent<PlayerController>();

        playerHealth =
            GetComponent<PlayerHealth>();

        behaviorTracker =
            GetComponent<PlayerBehaviorTracker>();
    }

    private void Update()
    {
        UpdateAbilityTimers();
    }

    public void ActivateAbility(
        string abilityType
    )
    {
        float duration =
            GetAbilityDuration(
                abilityType
            );

        if (duration <= 0f)
        {
            Debug.LogWarning(
                "Unknown ability: " +
                abilityType
            );

            return;
        }

        ActiveAbility existingAbility =
            FindActiveAbility(
                abilityType
            );

        if (existingAbility != null)
        {
            existingAbility.remainingTime =
                duration;

            Debug.Log(
                abilityType +
                " refreshed: " +
                duration +
                " seconds"
            );

            return;
        }

        ActiveAbility newAbility =
            new ActiveAbility(
                abilityType,
                duration
            );

        activeAbilities.Add(
            newAbility
        );

        ApplyAbility(
            abilityType
        );

        if (behaviorTracker != null)
        {
            behaviorTracker.RegisterAbilityUse(
                abilityType
            );
        }

        Debug.Log(
            "ABILITY ACTIVATED: " +
            abilityType +
            " for " +
            duration +
            " seconds"
        );
    }

    private void UpdateAbilityTimers()
    {
        for (
            int i = activeAbilities.Count - 1;
            i >= 0;
            i--
        )
        {
            ActiveAbility ability =
                activeAbilities[i];

            ability.remainingTime -=
                Time.deltaTime;

            if (ability.remainingTime <= 0f)
            {
                ExpireAbility(
                    ability.abilityType
                );

                activeAbilities.RemoveAt(i);
            }
        }
    }

    private void ApplyAbility(
        string abilityType
    )
    {
        switch (abilityType)
        {
            case "Power":
                if (weapon != null)
                    weapon.ActivatePowerSurge();
                break;

            case "RapidFire":
                if (weapon != null)
                    weapon.ActivateRapidFire();
                break;

            case "Speed":
                if (controller != null)
                    controller.ActivateSpeedBoost();
                break;

            case "HeavyShot":
                if (weapon != null)
                    weapon.ActivateHeavyShot();
                break;

            case "Piercing":
                if (weapon != null)
                    weapon.ActivatePiercing();
                break;

            // case "Dash":
            //     if (controller != null)
            //         controller.ActivateDash();
            //     break;

            case "Shield":
                if (playerHealth != null)
                    playerHealth.ActivateShield();
                break;

            default:
                Debug.LogWarning(
                    "Unknown ability: " +
                    abilityType
                );
                break;
        }
    }

    private void ExpireAbility(
        string abilityType
    )
    {
        switch (abilityType)
        {
            case "Power":
                if (weapon != null)
                    weapon.DeactivatePowerSurge();
                break;

            case "RapidFire":
                if (weapon != null)
                    weapon.DeactivateRapidFire();
                break;

            case "Speed":
                if (controller != null)
                    controller.DeactivateSpeedBoost();
                break;

            case "HeavyShot":
                if (weapon != null)
                    weapon.DeactivateHeavyShot();
                break;

            case "Piercing":
                if (weapon != null)
                    weapon.DeactivatePiercing();
                break;

            // case "Dash":
            //     if (controller != null)
            //         controller.DeactivateDash();
            //     break;

            case "Shield":
                if (playerHealth != null)
                    playerHealth.DeactivateShield();
                break;
        }

        Debug.Log(
            "ABILITY EXPIRED: " +
            abilityType
        );
    }

    private float GetAbilityDuration(
        string abilityType
    )
    {
        switch (abilityType)
        {
            case "Power":
                return powerDuration;

            case "RapidFire":
                return rapidFireDuration;

            case "Speed":
                return speedDuration;

            case "HeavyShot":
                return heavyShotDuration;

            case "Piercing":
                return piercingDuration;

            // case "Dash":
            //     return dashDuration;

            case "Shield":
                return shieldDuration;
        }

        return 0f;
    }

    private ActiveAbility FindActiveAbility(
        string abilityType
    )
    {
        foreach (
            ActiveAbility ability
            in activeAbilities
        )
        {
            if (
                ability.abilityType ==
                abilityType
            )
            {
                return ability;
            }
        }

        return null;
    }

    public float GetRemainingTime(
        string abilityType
    )
    {
        ActiveAbility ability =
            FindActiveAbility(
                abilityType
            );

        if (ability == null)
            return 0f;

        return ability.remainingTime;
    }

    public bool IsAbilityActive(
        string abilityType
    )
    {
        return FindActiveAbility(
            abilityType
        ) != null;
    }
}