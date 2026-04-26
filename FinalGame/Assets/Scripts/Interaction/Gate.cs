using UnityEngine;

public class Gate : MonoBehaviour
{
    [HideInInspector] public bool isOpen = false;

    /// <summary>
    /// Called when the player tries to open the gate.
    /// </summary>
    public void TryOpen(PlayerInteraction player)
    {
        if (isOpen) return;

        if (player.hasKey)
        {
            Open();

            // Record the interaction for ghost replay
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

    /// <summary>
    /// Called by a ghost that has a key to open the gate.
    /// Returns true if the gate was opened.
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

    /// <summary>
    /// Called at the start of each new life to reset this gate.
    /// </summary>
    public void ResetGate()
    {
        isOpen = false;
        gameObject.SetActive(true);
    }
}