using UnityEngine;
using System.Collections;

/// <summary>
/// A trap that triggers once per life. First entity to touch it (player or ghost) triggers it.
/// When triggered: trap fades away and disables.
///   - If player triggered: player dies.
///   - If ghost triggered: ghost fades and despawns.
/// After triggering, the trap zone is safe for everyone else that life.
/// Resets fully at the start of each new life.
/// </summary>
public class TrapZone : MonoBehaviour
{
    [Header("Fade Settings")]
    public float fadeDuration = 1.0f;

    [HideInInspector] public bool isTriggered = false;

    private Renderer trapRenderer;
    private Color originalColor;
    private Collider trapCollider;

    void Start()
    {
        trapRenderer = GetComponent<Renderer>();
        trapCollider = GetComponent<Collider>();

        if (trapRenderer != null)
        {
            originalColor = trapRenderer.material.color;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (isTriggered) return;

        // --- Ghost triggers the trap ---
        if (other.CompareTag("Ghost"))
        {
            GhostReplay ghost = other.GetComponent<GhostReplay>();
            if (ghost == null) ghost = other.GetComponentInParent<GhostReplay>();

            if (ghost != null)
            {
                isTriggered = true;
                Debug.Log("Trap triggered by ghost! Both fading away.");
                ghost.DespawnAtTrap();
                StartCoroutine(FadeTrap());
            }
            return;
        }

        // --- Player triggers the trap ---
        if (other.CompareTag("Player"))
        {
            PlayerLife life = other.GetComponent<PlayerLife>();
            if (life != null)
            {
                isTriggered = true;
                Debug.Log("Trap triggered by player!");
                if (AudioManager.Instance != null) AudioManager.Instance.PlayDeath();
                StartCoroutine(FadeTrapThenKill(life));
            }
        }
    }

    IEnumerator FadeTrap()
    {
        if (trapCollider != null) trapCollider.enabled = false;
        yield return StartCoroutine(DoFade());
        gameObject.SetActive(false);
    }

    IEnumerator FadeTrapThenKill(PlayerLife life)
    {
        if (trapCollider != null) trapCollider.enabled = false;
        yield return StartCoroutine(DoFade());
        gameObject.SetActive(false);
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

    // --- Reset (called by PlayerLife at start of each new life) ---

    public void ResetTrap()
    {
        isTriggered = false;
        gameObject.SetActive(true);

        if (trapCollider != null) trapCollider.enabled = true;

        if (trapRenderer != null)
        {
            trapRenderer.material.color = originalColor;
        }
    }
}