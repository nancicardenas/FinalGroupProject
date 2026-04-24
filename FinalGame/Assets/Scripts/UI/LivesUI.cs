using UnityEngine;
using TMPro;

public class LivesUI : MonoBehaviour
{
    public TextMeshProUGUI livesText;
    public PlayerLife playerLife;

    void Update()
    {
        if (playerLife != null && livesText != null)
        {
            livesText.text = playerLife.currentLives.ToString();
        }
    }
}
