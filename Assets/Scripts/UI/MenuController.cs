using UnityEngine;

public class MenuUIController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject upgradesPanel;
    [SerializeField] private GameObject howToPlayPanel;
    [SerializeField] private GameObject settingsPanel;

    public void ShowMainMenu()
    {
        HideAllPanels();

        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);
    }

    public void ShowUpgrades()
    {
        HideAllPanels();

        if (upgradesPanel != null)
            upgradesPanel.SetActive(true);
    }

    public void ShowHowToPlay()
    {
        HideAllPanels();

        if (howToPlayPanel != null)
            howToPlayPanel.SetActive(true);
    }

    public void ShowSettings()
    {
        HideAllPanels();

        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }

    private void HideAllPanels()
    {
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);

        if (upgradesPanel != null)
            upgradesPanel.SetActive(false);

        if (howToPlayPanel != null)
            howToPlayPanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }
}