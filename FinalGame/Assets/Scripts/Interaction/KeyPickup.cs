using UnityEngine;

public class KeyPickup : MonoBehaviour
{
    [HideInInspector] public bool isPickedUp = false;

    /// <summary>
    /// Called when the player picks up this key.
    /// </summary>
    public void Pickup(PlayerInteraction player)
    {
        if (isPickedUp) return;

        isPickedUp = true;
        player.hasKey = true;
        Debug.Log("Key picked up by player!");

        // Record the interaction for ghost replay
        GhostRecorder recorder = player.GetComponent<GhostRecorder>();
        if (recorder != null)
        {
            recorder.RecordInteraction(GhostRecorder.InteractionType.PickupKey);
        }

        if (AudioManager.Instance != null) AudioManager.Instance.PlayPickup();

        gameObject.SetActive(false);
    }

    /// <summary>
    /// Called when a ghost replays a key pickup.
    /// Returns true if the ghost successfully got the key.
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

    /// <summary>
    /// Called at the start of each new life to reset this key.
    /// </summary>
    public void ResetKey()
    {
        isPickedUp = false;
        gameObject.SetActive(true);
    }
}