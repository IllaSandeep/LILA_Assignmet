using UnityEngine;

public class NemesisProjectile : MonoBehaviour
{
    [Header("Projectile")]
    [SerializeField] private float speed = 6f;
    [SerializeField] private float damage = 10f;
    [SerializeField] private float lifetime = 4f;

    [Header("Impact")]
    [SerializeField] private float impactDuration = 0.2f;

    private Vector2 direction;
    private float lifetimeTimer;

    private Animator animator;
    private bool hasHit;

    private void Awake()
    {
        animator =
            GetComponent<Animator>();

        lifetimeTimer =
            lifetime;
    }

    public void Initialize(
        Vector2 targetDirection,
        float speedMultiplier = 1f,
        float damageMultiplier = 1f
    )
    {
        direction =
            targetDirection.normalized;
        speed *= Mathf.Max(0f, speedMultiplier);
        damage *= Mathf.Max(0f, damageMultiplier);

        float angle =
            Mathf.Atan2(
                direction.y,
                direction.x
            ) *
            Mathf.Rad2Deg;

        transform.rotation =
            Quaternion.Euler(
                0f,
                0f,
                angle
            );
    }

    private void Update()
    {
        if (hasHit)
            return;

        transform.position +=
            (Vector3)(
                direction *
                speed *
                Time.deltaTime
            );

        lifetimeTimer -=
            Time.deltaTime;

        if (lifetimeTimer <= 0f)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(
        Collider2D other
    )
    {
        if (hasHit)
            return;

        if (!other.CompareTag("Player"))
            return;

        PlayerHealth playerHealth =
            other.GetComponent<PlayerHealth>();

        if (playerHealth == null)
            return;

        hasHit = true;

        playerHealth.TakeDamage(
            damage
        );

        PlayImpact();

        Debug.Log(
            "NEMESIS PROJECTILE HIT PLAYER!"
        );
    }

    private void PlayImpact()
    {
        if (animator == null)
        {
            Destroy(gameObject);
            return;
        }

        animator.SetTrigger(
            "Hit"
        );

        Destroy(
            gameObject,
            impactDuration
        );
    }
}
