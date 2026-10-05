using UnityEngine;

public class NemesisTrap : MonoBehaviour
{
    [Header("Trap")]
    [SerializeField] private float damage = 20f;
    [SerializeField] private float lifetime = 6f;
    [SerializeField] private float knockbackForce = 2f;

    [Header("Activation")]
    [SerializeField] private float activationDelay = 0.5f;

    private float lifetimeTimer;
    private float activationTimer;
    private float baseDamage;

    private bool activated;
    private bool triggered;

    private Vector3 originalScale;

    private void Awake()
    {
        baseDamage = damage;
        lifetimeTimer = lifetime;
        activationTimer = activationDelay;

        originalScale =
            transform.localScale;
    }

    public void SetDamageMultiplier(float multiplier)
    {
        damage = baseDamage * Mathf.Max(0f, multiplier);
    }

    private void Update()
    {
        lifetimeTimer -=
            Time.deltaTime;

        if (lifetimeTimer <= 0f)
        {
            Destroy(gameObject);
            return;
        }

        if (!activated)
        {
            activationTimer -=
                Time.deltaTime;

            float pulse =
                1f +
                Mathf.Sin(
                    Time.time * 8f
                ) * 0.08f;

            transform.localScale =
                originalScale * pulse;

            if (activationTimer <= 0f)
            {
                activated = true;
            }
        }
    }

    private void OnTriggerEnter2D(
        Collider2D other
    )
    {
        if (!activated)
            return;

        if (triggered)
            return;

        if (!other.CompareTag("Player"))
            return;

        PlayerHealth playerHealth =
            other.GetComponent<PlayerHealth>();

        if (playerHealth == null)
            return;

        triggered = true;

        playerHealth.TakeDamage(
            damage
        );

        Vector2 knockbackDirection = ((Vector2)other.transform.position - (Vector2)transform.position).normalized;
        playerHealth.ApplyKnockback(knockbackDirection, knockbackForce);

        Debug.Log(
            "NEMESIS TRAP TRIGGERED!"
        );

        Destroy(gameObject);
    }
}
