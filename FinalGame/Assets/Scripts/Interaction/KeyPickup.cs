using UnityEngine;

public class KeyPickup : MonoBehaviour
{
    public void Pickup(PlayerInteraction player)
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayPickup();
        player.hasKey = true;
        Debug.Log("Key picked up!");
        gameObject.SetActive(false); // hide the key
    }
}
