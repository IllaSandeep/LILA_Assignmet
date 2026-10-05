using UnityEngine;

public class UpgradePickup : MonoBehaviour
{
    [Header("Ability")]
    [SerializeField] private string abilityType;

    public void SetUpgradeType(string type)
    {
        abilityType = type;

        Debug.Log(
            "Ability pickup created: [" +
            abilityType +
            "]"
        );
    }

    public string GetUpgradeType()
    {
        return abilityType;
    }

    private void OnTriggerEnter2D(
        Collider2D other
    )
    {
        if (!other.CompareTag("Player"))
            return;

        if (string.IsNullOrWhiteSpace(abilityType))
        {
            Debug.LogError(
                "UPGRADE PICKUP ERROR: " +
                gameObject.name +
                " has NO ability type assigned!"
            );

            return;
        }

        PlayerExperience experience =
            other.GetComponent<PlayerExperience>();

        if (experience == null)
        {
            Debug.LogError(
                "UpgradePickup: PlayerExperience " +
                "missing from Player!"
            );

            return;
        }

        Debug.Log(
            "Ability selected: [" +
            abilityType +
            "]"
        );

        experience.ApplyUpgradeFromPickup(
            abilityType
        );

        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayPowerUp();

        UpgradeSpawner spawner =
            FindFirstObjectByType<UpgradeSpawner>();

        if (spawner != null)
        {
            spawner.UpgradeCollected(
                this
            );
        }

        Destroy(gameObject);
    }
}
