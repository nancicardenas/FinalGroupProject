using UnityEngine;
using System.Collections;

/// <summary>
/// A trap that triggers once per life. Can be triggered by player OR ghost.
/// When triggered:
///   - If player: player dies, trap fades away
///   - If ghost: ghost fades and despawns, trap fades away
/// The trap resets at the start of each new life.
/// </summary>
public class TrapZone : MonoBehaviour
{
    [Header("Fade Settings")]
    public float fadeDuration = 1.0f;

    [HideInInspector] public bool isTriggered = false;

    private Renderer trapRenderer;
    private Color originalColor;

    void Start()
    {
        trapRenderer = GetComponent<Renderer>();
        if (trapRenderer != null)
        {
            originalColor = trapRenderer.material.color;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (isTriggered) return;

        // Check for ghost first
        if (other.CompareTag("Ghost"))
        {
            GhostReplay ghost = other.GetComponent<GhostReplay>();
            if (ghost == null) ghost = other.GetComponentInParent<GhostReplay>();

            if (ghost != null)
            {
                isTriggered = true;
                Debug.Log("Trap triggered by ghost! Both fading away.");

                // Ghost fades and despawns
                ghost.DespawnAtTrap();

                // Trap fades away
                StartCoroutine(FadeTrap());
            }
            return;
        }

        // Check for player
        if (other.CompareTag("Player"))
        {
            PlayerLife life = other.GetComponent<PlayerLife>();
            if (life != null)
            {
                isTriggered = true;
                Debug.Log("Trap triggered by player!");

                if (AudioManager.Instance != null) AudioManager.Instance.PlayDeath();

                // Fade the trap, then kill the player
                StartCoroutine(FadeTrapThenKill(life));
            }
        }
    }

    IEnumerator FadeTrap()
    {
        // Disable the trigger so nothing else hits it
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        yield return StartCoroutine(DoFade());

        gameObject.SetActive(false);
    }

    IEnumerator FadeTrapThenKill(PlayerLife life)
    {
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        yield return StartCoroutine(DoFade());

        gameObject.SetActive(false);

        // Kill the player after fade
        life.Die();
    }

    IEnumerator DoFade()
    {
        if (trapRenderer == null) yield break;

        float elapsed = 0f;
        Color startColor = trapRenderer.material.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = 1f - (elapsed / fadeDuration);
            Color c = startColor;
            c.a = Mathf.Max(0f, startColor.a * alpha);
            trapRenderer.material.color = c;
            yield return null;
        }
    }

    /// <summary>
    /// Called at the start of each new life to reset this trap.
    /// </summary>
    public void ResetTrap()
    {
        isTriggered = false;
        gameObject.SetActive(true);

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = true;

        if (trapRenderer != null)
        {
            trapRenderer.material.color = originalColor;
        }
    }
}