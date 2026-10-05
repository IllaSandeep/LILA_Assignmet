using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    [Header("Healing")]
    [SerializeField] private float healAmount = 25f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        PlayerHealth playerHealth =
            other.GetComponent<PlayerHealth>();

        if (playerHealth == null)
            return;

        playerHealth.Heal(healAmount);

        Debug.Log(
            "HEALTH PICKUP COLLECTED! Healed: " +
            healAmount
        );

        Destroy(gameObject);
    }
}