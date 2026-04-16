using UnityEngine;

/// <summary>
/// Shows a tutorial message when the player enters the trigger zone.
/// Place on an empty GameObject with a Box Collider (Is Trigger = true).
/// </summary>
public class TutorialTrigger : MonoBehaviour
{
    [TextArea(2, 5)]
    public string message;
    public float displayDuration = 5f;
    public bool triggerOnce = true;
    private bool hasTriggered = false;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (triggerOnce && hasTriggered) return;
        hasTriggered = true;

        if (TutorialManager.Instance != null)
        {
            TutorialManager.Instance.ShowPrompt(message, displayDuration);
        }
    }
}
