using UnityEngine;

/// <summary>
/// A key that can be picked up by the player or a ghost.
/// First come, first served — whoever interacts first gets it.
/// Resets fully at the start of each new life.
/// </summary>
public class KeyPickup : MonoBehaviour
{
    [HideInInspector] public bool isPickedUp = false;

    // --- Player pickup (called from PlayerInteraction) ---

    public void Pickup(PlayerInteraction player)
    {
        if (isPickedUp) return;

        isPickedUp = true;
        player.hasKey = true;
        Debug.Log("Key picked up by player!");

        // Record interaction so ghosts replay it
        GhostRecorder recorder = player.GetComponent<GhostRecorder>();
        if (recorder != null)
        {
            recorder.RecordInteraction(GhostRecorder.InteractionType.PickupKey);
        }

        if (AudioManager.Instance != null) AudioManager.Instance.PlayPickup();

        gameObject.SetActive(false);
    }
    // --- Ghost pickup (called from GhostReplay) ---

    /// <summary>
    /// Returns true if the ghost successfully picked up the key.
    /// Returns false if someone already took it.
    /// </summary>
    public bool GhostPickup()
    {
        if (isPickedUp) return false;

        isPickedUp = true;
        Debug.Log("Key picked up by ghost!");

        if (AudioManager.Instance != null) AudioManager.Instance.PlayPickup();

        gameObject.SetActive(false);
        return true;
    }

    // --- Reset (called by PlayerLife at start of each new life) ---

    public void ResetKey()
    {
        isPickedUp = false;
        gameObject.SetActive(true);
    }
}