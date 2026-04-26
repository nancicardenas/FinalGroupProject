using UnityEngine;

/// <summary>
/// A gate that requires a key to open.
/// Can be opened by the player or a ghost that holds the key.
/// Resets fully at the start of each new life.
/// </summary>
public class Gate : MonoBehaviour
{
    [HideInInspector] public bool isOpen = false;

    // --- Player interaction (called from PlayerInteraction) ---

    public void TryOpen(PlayerInteraction player)
    {
        if (isOpen) return;

        if (player.hasKey)
        {
            Open();

            // Record interaction so ghosts replay it
            GhostRecorder recorder = player.GetComponent<GhostRecorder>();
            if (recorder != null)
            {
                recorder.RecordInteraction(GhostRecorder.InteractionType.OpenGate);
            }
        }
        else
        {
            Debug.Log("You need a key to open this gate.");
        }
    }

    // --- Ghost interaction (called from GhostReplay) ---

    /// <summary>
    /// Returns true if the ghost successfully opened the gate.
    /// </summary>
    public bool GhostOpen(bool ghostHasKey)
    {
        if (isOpen) return false;

        if (ghostHasKey)
        {
            Open();
            return true;
        }

        return false;
    }

    void Open()
    {
        isOpen = true;
        Debug.Log("Gate opened!");
        if (AudioManager.Instance != null) AudioManager.Instance.PlayGateOpen();
        gameObject.SetActive(false);
    }

    // --- Reset (called by PlayerLife at start of each new life) ---

    public void ResetGate()
    {
        isOpen = false;
        gameObject.SetActive(true);
    }
}