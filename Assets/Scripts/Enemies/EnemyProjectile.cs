using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [Header("Projectile")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private float damage = 10f;
    [SerializeField] private float lifetime = 4f;

    [Header("Blast")]
    [SerializeField] private float blastDuration = 0.25f;

    private Vector2 direction;

    private Animator animator;

    private bool hitPlayer;
    private float blastTimer;

    // --------------------------------------------------
    // AWAKE
    // --------------------------------------------------

    private void Awake()
    {
        animator =
            GetComponent<Animator>();
    }

    // --------------------------------------------------
    // INITIALIZE
    // --------------------------------------------------

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

        // Rotate projectile visual so that
        // its front points toward the player.
        float angle =
            Mathf.Atan2(
                direction.y,
                direction.x
            ) * Mathf.Rad2Deg;

        transform.rotation =
            Quaternion.Euler(
                0f,
                0f,
                angle
            );
    }

    // --------------------------------------------------
    // UPDATE
    // --------------------------------------------------

    private void Update()
    {
        // ----------------------------------------------
        // BLAST STATE
        // ----------------------------------------------

        if (hitPlayer)
        {
            blastTimer -=
                Time.deltaTime;

            if (blastTimer <= 0f)
            {
                Destroy(
                    gameObject
                );
            }

            return;
        }

        // ----------------------------------------------
        // PROJECTILE MOVEMENT
        // ----------------------------------------------

        // Move using WORLD direction.
        // This prevents projectile rotation from
        // changing its actual travel direction.
        transform.position +=
            (Vector3)(
                direction *
                speed *
                Time.deltaTime
            );

        lifetime -=
            Time.deltaTime;

        if (lifetime <= 0f)
        {
            Destroy(
                gameObject
            );
        }
    }

    // --------------------------------------------------
    // PLAYER HIT
    // --------------------------------------------------

    private void OnTriggerEnter2D(
        Collider2D other
    )
    {
        if (!other.CompareTag("Player"))
            return;

        if (hitPlayer)
            return;

        PlayerHealth playerHealth =
            other.GetComponent<PlayerHealth>();

        if (playerHealth != null)
        {
            playerHealth.TakeDamage(
                damage
            );
        }

        // Start blast animation.
        PlayBlast();
    }

    // --------------------------------------------------
    // PLAY BLAST
    // --------------------------------------------------

    private void PlayBlast()
    {
        hitPlayer = true;

        blastTimer =
            blastDuration;

        if (animator != null)
        {
            animator.SetTrigger(
                "Hit"
            );
        }
        else
        {
            Debug.LogWarning(
                "EnemyProjectile: Animator not found."
            );
        }
    }
}
