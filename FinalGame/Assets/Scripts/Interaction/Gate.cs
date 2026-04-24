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

            //if player is in tutorial change to level 1 scene 
            if(TutorialManager.Instance != null)
            {
                TutorialManager.Instance.CompleteTutorial("AncientGreekIsland");
            }
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
