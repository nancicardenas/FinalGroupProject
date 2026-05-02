using UnityEngine;
using System.Collections;

public class TrapZone : MonoBehaviour
{
    [Header("Trap Settings")]
    [Tooltip("If true, trap stays active and can trigger multiple times (water, lava, etc).")]
    public bool persistent = false;

    [Header("Fade Settings")]
    public float fadeDuration = 1.0f;

    [HideInInspector] public bool isTriggered = false;

    private Renderer trapRenderer;
    private Color originalColor;
    private Collider trapCollider;
    private bool isProcessing = false;

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
        if (!persistent && isTriggered) return;
        if (isProcessing) return;

        // --- Ghost triggers the trap ---
        if (other.CompareTag("Ghost"))
        {
            GhostReplay ghost = other.GetComponent<GhostReplay>();
            if (ghost == null) ghost = other.GetComponentInParent<GhostReplay>();

            if (ghost != null)
            {
                Debug.Log("Trap triggered by ghost!");
                ghost.DespawnAtTrap();

                if (!persistent)
                {
                    isTriggered = true;
                    StartCoroutine(FadeTrap());
                }
            }
            return;
        }

        // --- Player triggers the trap ---
        if (other.CompareTag("Player"))
        {
            PlayerLife life = other.GetComponent<PlayerLife>();
            if (life == null) return;
            if (life.IsDead) return; // don't kill during death sequence

            Debug.Log("Trap triggered by player!");
            if (AudioManager.Instance != null) AudioManager.Instance.PlayDeath();

            if (persistent)
            {
                isProcessing = true;
                life.Die();
            }
            else
            {
                isTriggered = true;
                isProcessing = true;
                StartCoroutine(FadeTrapThenKill(life));
            }
        }
    }

    IEnumerator FadeTrap()
    {
        if (trapCollider != null) trapCollider.enabled = false;
        yield return StartCoroutine(DoFade());
        gameObject.SetActive(false);
        isProcessing = false;
    }

    IEnumerator FadeTrapThenKill(PlayerLife life)
    {
        if (trapCollider != null) trapCollider.enabled = false;
        yield return StartCoroutine(DoFade());
        gameObject.SetActive(false);
        isProcessing = false;
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

    public void ResetTrap()
    {
        isProcessing = false;

        if (persistent)
        {
            return;
        }

        isTriggered = false;
        gameObject.SetActive(true);

        if (trapCollider != null) trapCollider.enabled = true;

        if (trapRenderer != null)
        {
            trapRenderer.material.color = originalColor;
        }
    }
}