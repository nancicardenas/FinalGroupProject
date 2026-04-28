using UnityEngine;
using TMPro;

/// <summary>
/// Updates the HUD showing lives remaining and keys held.
/// Attach to TutorialCanvas. PlayerSpawner wires playerLife at runtime.
/// </summary>
public class LivesUI : MonoBehaviour
{
    [Header("Lives")]
    public TextMeshProUGUI livesText;

    [Header("Keys")]
    public TextMeshProUGUI keyText;

    [HideInInspector] public PlayerLife playerLife;

    private PlayerInteraction playerInteraction;

    void Update()
    {
        // Wire playerInteraction once playerLife is available
        if (playerInteraction == null && playerLife != null)
        {
            playerInteraction = playerLife.GetComponent<PlayerInteraction>();
        }

        if (playerLife != null && livesText != null)
        {
            livesText.text = playerLife.currentLives.ToString();
        }

        if (playerInteraction != null && keyText != null)
        {
            keyText.text = playerInteraction.hasKey ? "1" : "0";
        }
    }
}