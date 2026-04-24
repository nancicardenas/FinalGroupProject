using UnityEngine;

public class KeyPickup : MonoBehaviour
{

    public void Pickup(PlayerInteraction player)
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayPickup();
        player.hasKey = true;

        //update player inventory on key pick up 
        PlayerInventory playerInventory = player.GetComponent<PlayerInventory>();
        playerInventory.KeyCollected();


        Debug.Log("Key picked up!");
        gameObject.SetActive(false); // hide the key
    }
}
