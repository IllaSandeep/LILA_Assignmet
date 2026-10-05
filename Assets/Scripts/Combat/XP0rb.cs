using UnityEngine;

public class XPOrb : MonoBehaviour
{
    [Header("XP")]
    [SerializeField] private int xpAmount = 10;

    [Header("Scrap")]
    [SerializeField] private int scrapAmount = 10;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float pickupDistance = 0.25f;

    private Transform player;
    private PlayerExperience playerExperience;

    private int bonusXP;

    private void Start()
    {
        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player =
                playerObject.transform;

            playerExperience =
                playerObject.GetComponent<PlayerExperience>();
        }
    }

    public void SetBonusXP(int amount)
    {
        bonusXP = amount;
    }

    private void Update()
    {
        if (player == null)
            return;

        Vector2 direction =
            (
                (Vector2)player.position -
                (Vector2)transform.position
            ).normalized;

        transform.position +=
            (Vector3)(
                direction *
                moveSpeed *
                Time.deltaTime
            );

        float distance =
            Vector2.Distance(
                transform.position,
                player.position
            );

        if (distance <= pickupDistance)
        {
            Collect();
        }
    }

    private void Collect()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError(
                "XPOrb: GameManager.Instance is null. Cannot award Scrap."
            );
        }
        else if (GameManager.Instance.ScrapManager == null)
        {
            Debug.LogError(
                "XPOrb: GameManager has no ScrapManager. Cannot award Scrap."
            );
        }
        else
        {
            GameManager.Instance.ScrapManager.AddScrap(
                scrapAmount
            );
        }

        if (playerExperience != null)
        {
            int totalXP =
                xpAmount + bonusXP;

            playerExperience.AddXP(
                totalXP
            );
        }

        Destroy(gameObject);
    }
}
