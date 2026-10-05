using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScrapHUD : MonoBehaviour
{
    [Header("UI")]
    [Tooltip("Text that displays the persistent Scrap total.")]
    [SerializeField] private TMP_Text scrapText;
    [Tooltip("Replace this empty Image with the final Scrap icon.")]
    [SerializeField] private Image scrapIcon;
    [Tooltip("Text placed before the current Scrap amount.")]
    [SerializeField] private string scrapPrefix = "SCRAP ";

    private const float RefreshInterval = 0.15f;

    private ScrapManager scrapManager;
    private int lastDisplayedScrap = int.MinValue;

    private void Awake()
    {
        if (scrapIcon != null)
            scrapIcon.raycastTarget = false;
    }

    private IEnumerator Start()
    {
        while (true)
        {
            if (scrapManager == null && GameManager.Instance != null)
                scrapManager = GameManager.Instance.ScrapManager;

            if (scrapManager != null)
                RefreshScrapText();

            yield return new WaitForSecondsRealtime(RefreshInterval);
        }
    }

    private void RefreshScrapText()
    {
        if (scrapText == null)
            return;

        int currentScrap = scrapManager.CurrentScrap;
        if (currentScrap == lastDisplayedScrap)
            return;

        scrapText.text = scrapPrefix + currentScrap;
        lastDisplayedScrap = currentScrap;
    }
}
