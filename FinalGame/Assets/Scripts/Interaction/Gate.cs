using UnityEngine;

public class Gate : MonoBehaviour
{
    private bool isOpen = false;
    public void TryOpen(PlayerInteraction player)
    {
        if (isOpen) return;

        if (player.hasKey)
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlayGateOpen();
            isOpen = true;
            Debug.Log("Gate opened!");
            // Simple open: just deactivate the gate
            gameObject.SetActive(false);
        }
        else
        {
            Debug.Log("You need a key to open this gate.");
        }
    }
}
