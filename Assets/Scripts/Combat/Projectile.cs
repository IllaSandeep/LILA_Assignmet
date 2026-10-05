using UnityEngine;
using System.Collections.Generic;

public class Projectile : MonoBehaviour
{
    [Header("Projectile")]
    [SerializeField] private float speed = 8f;
    [SerializeField] private float damage = 10f;
    [SerializeField] private float lifetime = 3f;
    [Tooltip("Scale multiplier for the standard projectile; 1 keeps the prefab's current size.")]
    [SerializeField] private float projectileSizeMultiplier = 1f;

    [Header("Blast Animation")]
    [SerializeField] private float blastDuration = 0.25f;

    [Header("Heavy Shot")]
    [SerializeField] private float heavySpeedMultiplier = 1.25f;
    [SerializeField] private float heavySizeMultiplier = 1.8f;
    [SerializeField] private float heavyKnockbackForce = 6f;

    private Vector2 direction;

    private bool piercing;
    private bool heavyShot;

    private bool playingBlast;

    private float blastTimer;

    private Animator animator;

    private HashSet<GameObject> hitEnemies =
        new HashSet<GameObject>();

    // --------------------------------------------------
    // INITIALIZE
    // --------------------------------------------------

    public void Initialize(
        Vector2 targetDirection,
        float damageAmount,
        bool piercingShot
    )
    {
        direction =
            targetDirection.normalized;

        transform.localScale *= projectileSizeMultiplier;

        damage =
            damageAmount;

        piercing =
            piercingShot;

        // Find Animator on the projectile.
        animator =
            GetComponent<Animator>();

        // Rotate projectile so its visual
        // points in its travel direction.
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
    // HEAVY SHOT
    // --------------------------------------------------

    public void SetHeavyShot()
    {
        heavyShot = true;

        speed *=
            heavySpeedMultiplier;

        transform.localScale *=
            heavySizeMultiplier;
    }

    // --------------------------------------------------
    // UPDATE
    // --------------------------------------------------

    private void Update()
    {
        // ----------------------------------------------
        // BLAST IS PLAYING
        // ----------------------------------------------

        if (playingBlast)
        {
            blastTimer -=
                Time.deltaTime;

            // Do not move while blast animation
            // is playing.
            if (blastTimer <= 0f)
            {
                FinishBlast();
            }

            return;
        }

        // ----------------------------------------------
        // NORMAL PROJECTILE MOVEMENT
        // ----------------------------------------------

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
            Destroy(gameObject);
        }
    }

    // --------------------------------------------------
    // ENEMY HIT
    // --------------------------------------------------

    private void OnTriggerEnter2D(
        Collider2D other
    )
    {
        EnemyHealth enemy =
            other.GetComponent<EnemyHealth>();

        if (enemy == null)
            return;

        // Prevent the same projectile from hitting
        // the same enemy repeatedly.
        if (
            hitEnemies.Contains(
                other.gameObject
            )
        )
        {
            return;
        }

        hitEnemies.Add(
            other.gameObject
        );

        // ----------------------------------------------
        // DAMAGE
        // ----------------------------------------------

        enemy.TakeDamage(
            damage,
            heavyShot
        );

        // ----------------------------------------------
        // HEAVY SHOT KNOCKBACK
        // ----------------------------------------------

        if (heavyShot)
        {
            Vector2 knockbackDirection =
                (
                    (Vector2)other.transform.position -
                    (Vector2)transform.position
                ).normalized;

            NemesisController nemesisController =
                other.GetComponent<NemesisController>();

            if (nemesisController != null)
            {
                // Nemesis movement is kinematic and owned by its AI. Route
                // intentional Heavy Shot knockback through that controller.
                nemesisController.ApplyKnockback(knockbackDirection, heavyKnockbackForce);
            }
            else
            {
                Rigidbody2D enemyRb = other.GetComponent<Rigidbody2D>();
                if (enemyRb != null)
                    enemyRb.AddForce(knockbackDirection * heavyKnockbackForce, ForceMode2D.Impulse);
            }
        }

        // ----------------------------------------------
        // PLAY BLAST
        // ----------------------------------------------

        PlayBlast();

        // ----------------------------------------------
        // NORMAL SHOT
        // ----------------------------------------------

        if (!piercing)
        {
            // Normal projectile will be destroyed
            // after the blast animation.
            return;
        }

        // ----------------------------------------------
        // PIERCING SHOT
        // ----------------------------------------------

        // Piercing projectile continues after blast.
    }

    // --------------------------------------------------
    // PLAY BLAST
    // --------------------------------------------------

    private void PlayBlast()
    {
        if (animator == null)
        {
            animator =
                GetComponent<Animator>();
        }

        if (animator == null)
        {
            Debug.LogWarning(
                "Projectile: Animator is missing."
            );

            return;
        }

        playingBlast =
            true;

        blastTimer =
            blastDuration;

        animator.SetTrigger(
            "Hit"
        );
    }

    // --------------------------------------------------
    // FINISH BLAST
    // --------------------------------------------------

    private void FinishBlast()
    {
        playingBlast =
            false;

        // Normal projectile disappears
        // after the blast.
        if (!piercing)
        {
            Destroy(gameObject);
            return;
        }

        // Piercing projectile continues travelling.
        if (animator != null)
        {
            animator.Play(
                "Fire",
                0,
                0f
            );
        }
    }
}
