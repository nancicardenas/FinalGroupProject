using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// A door the player clicks to leave the tutorial and enter the next scene.
/// Placed after the gate, before or after the trap area.
/// </summary>
public class ExitDoor : MonoBehaviour
{
    [Header("Settings")]
    public string nextSceneName = "EndingCutscene"; // or "Level1" when you have more levels

    [HideInInspector] public bool isLocked = false;

    /// <summary>
    /// Called when the player clicks this door via PlayerInteraction.
    /// </summary>
    public void Use()
    {
        if (isLocked)
        {
            Debug.Log("The door is locked.");
            return;
        }

        Debug.Log("Exiting tutorial! Loading: " + nextSceneName);
        SceneManager.LoadScene(nextSceneName);
    }

    /// <summary>
    /// Called at start of each new life.
    /// </summary>
    public void ResetDoor()
    {
        isLocked = false;
    }
}