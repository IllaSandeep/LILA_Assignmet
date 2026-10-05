using UnityEngine;
using System.Collections.Generic;

public class UpgradeSpawner : MonoBehaviour
{
    [Header("Ability Prefabs")]
    [SerializeField] private GameObject powerPrefab;
    [SerializeField] private GameObject rapidFirePrefab;
    [SerializeField] private GameObject speedPrefab;
    [SerializeField] private GameObject heavyShotPrefab;
    [SerializeField] private GameObject piercingPrefab;
    // [SerializeField] private GameObject dashPrefab;
    [SerializeField] private GameObject shieldPrefab;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnDistance = 3f;
    [Tooltip("Number of distinct ability pickups offered when the player levels up. Defaults to the existing three choices.")]
    [SerializeField] private int choiceCount = 3;

    private Transform player;

    private readonly string[] abilityPool =
    {
        "Power",
        "RapidFire",
        "Speed",
        "HeavyShot",
        "Piercing",
        "Dash",
        "Shield"
    };

    private void Start()
    {
        GameObject playerObject =
            GameObject.FindGameObjectWithTag(
                "Player"
            );

        if (playerObject == null)
        {
            Debug.LogError(
                "UPGRADE SPAWNER: Player not found!"
            );

            return;
        }

        player =
            playerObject.transform;
    }

    public void SpawnUpgradeChoices()
    {
        if (player == null)
        {
            Debug.LogError(
                "UPGRADE SPAWNER: Player is missing!"
            );

            return;
        }

        ClearExistingUpgrades();

        List<string> availableAbilities =
            new List<string>(
                abilityPool
            );

        for (int i = 0; i < Mathf.Max(0, choiceCount); i++)
        {
            if (availableAbilities.Count == 0)
                break;

            int randomIndex =
                Random.Range(
                    0,
                    availableAbilities.Count
                );

            string selectedAbility =
                availableAbilities[
                    randomIndex
                ];

            availableAbilities.RemoveAt(
                randomIndex
            );

            Debug.Log(
                "SPAWNING ABILITY CHOICE: [" +
                selectedAbility +
                "]"
            );

            SpawnAbility(
                selectedAbility,
                i
            );
        }
    }

    private void SpawnAbility(
        string abilityType,
        int index
    )
    {
        GameObject prefab =
            GetAbilityPrefab(
                abilityType
            );

        if (prefab == null)
        {
            Debug.LogError(
                "UPGRADE SPAWNER: No prefab assigned for [" +
                abilityType +
                "]"
            );

            return;
        }

        float angle =
            index * 120f;

        float radians =
            angle * Mathf.Deg2Rad;

        Vector2 offset =
            new Vector2(
                Mathf.Cos(radians),
                Mathf.Sin(radians)
            ) *
            spawnDistance;

        Vector2 spawnPosition =
            (Vector2)player.position +
            offset;

        GameObject ability =
            Instantiate(
                prefab,
                spawnPosition,
                Quaternion.identity
            );

        UpgradePickup pickup =
            ability.GetComponent<UpgradePickup>();

        if (pickup == null)
        {
            Debug.LogError(
                "UPGRADE SPAWNER: Prefab [" +
                abilityType +
                "] does not contain UpgradePickup " +
                "on its ROOT GameObject."
            );

            return;
        }

        pickup.SetUpgradeType(
            abilityType
        );

        Debug.Log(
            "ABILITY PICKUP READY: [" +
            abilityType +
            "]"
        );
    }

    private GameObject GetAbilityPrefab(
        string abilityType
    )
    {
        switch (abilityType)
        {
            case "Power":
                return powerPrefab;

            case "RapidFire":
                return rapidFirePrefab;

            case "Speed":
                return speedPrefab;

            case "HeavyShot":
                return heavyShotPrefab;

            case "Piercing":
                return piercingPrefab;

            // case "Dash":
            //     return dashPrefab;

            case "Shield":
                return shieldPrefab;

            default:
                Debug.LogError(
                    "UPGRADE SPAWNER: Unknown ability [" +
                    abilityType +
                    "]"
                );

                return null;
        }
    }

    public void UpgradeCollected(
        UpgradePickup collected
    )
    {
        if (collected == null)
            return;

        ClearExistingUpgrades(
            collected.gameObject
        );
    }

    private void ClearExistingUpgrades(
        GameObject except = null
    )
    {
        UpgradePickup[] upgrades =
            FindObjectsByType<UpgradePickup>(
                FindObjectsSortMode.None
            );

        foreach (
            UpgradePickup upgrade
            in upgrades
        )
        {
            if (upgrade == null)
                continue;

            if (
                upgrade.gameObject ==
                except
            )
            {
                continue;
            }

            Destroy(
                upgrade.gameObject
            );
        }
    }
}
