using UnityEngine;
using UnityEngine.UI;

public class PlayerLivesUI : MonoBehaviour
{
    [Header("Heart Images")]
    [SerializeField] private Image[] heartImages;

    [Header("Heart Sprites")]
    [SerializeField] private Sprite filledHeart;
    [SerializeField] private Sprite emptyHeart;

    public void UpdateLives(int currentLives)
    {
        if (heartImages == null)
            return;

        for (int i = 0; i < heartImages.Length; i++)
        {
            if (heartImages[i] == null)
                continue;

            if (i < currentLives)
            {
                heartImages[i].sprite = filledHeart;
                heartImages[i].enabled = true;
            }
            else
            {
                heartImages[i].sprite = emptyHeart;
                heartImages[i].enabled = true;
            }
        }
    }
}