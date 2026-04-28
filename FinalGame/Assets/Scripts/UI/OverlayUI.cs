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
    public GhostManager ghostManager;

    [HideInInspector] public PlayerLife playerLife;

    private PlayerInteraction playerInteraction;

    void Update()
    {
        if (playerInteraction == null && playerLife != null)
        {
            playerInteraction = playerLife.GetComponent<PlayerInteraction>();
        }

        if (playerLife != null && livesText != null)
        {
            livesText.text = playerLife.currentLives.ToString();
        }

        if (keyText != null)
        {
            keyText.text = GetTotalKeys().ToString();
        }
    }

    int GetTotalKeys()
    {
        int keys = 0;

        // Check player
        if (playerInteraction != null && playerInteraction.hasKey)
        {
            keys++;
        }

        // Check all active ghosts
        if (ghostManager != null)
        {
            foreach (GameObject ghostObj in ghostManager.activeGhosts)
            {
                if (ghostObj == null) continue;
                GhostReplay replay = ghostObj.GetComponent<GhostReplay>();
                if (replay != null && replay.HasKey())
                {
                    keys++;
                }
            }
        }

        return keys;
    }
}