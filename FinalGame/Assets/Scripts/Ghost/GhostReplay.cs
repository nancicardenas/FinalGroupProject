using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Replays a recorded ghost path and interactions.
/// Handles despawning when touching a closed gate without a key.
/// </summary>
public class GhostReplay : MonoBehaviour
{
    private List<GhostRecorder.GhostFrame> frames;
    private int currentFrame = 0;
    private bool isPlaying = false;
    private bool ghostHasKey = false;
    private bool isDespawning = false;

    private Animator animator;
    private Renderer[] renderers;

    [Header("Fade Settings")]
    public float fadeDuration = 1.0f;

    // Ghost index (0-7), assigned by GhostManager for shader selection
    [HideInInspector] public int ghostIndex = 0;

    public void Initialize(List<GhostRecorder.GhostFrame> recordedFrames)
    {
        frames = recordedFrames;
        currentFrame = 0;
        isPlaying = true;
        ghostHasKey = false;
    }

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        renderers = GetComponentsInChildren<Renderer>();
    }

    void FixedUpdate()
    {
        if (!isPlaying || frames == null || isDespawning) return;

        if (currentFrame >= frames.Count)
        {
            isPlaying = false;
            if (animator != null)
            {
                animator.SetFloat("Speed", 0f);
            }
            return;
        }

        GhostRecorder.GhostFrame frame = frames[currentFrame];
        transform.position = frame.position;
        transform.rotation = frame.rotation;

        // Drive ghost animations
        if (animator != null)
        {
            animator.SetFloat("Speed", frame.speed);
            animator.SetBool("IsRunning", frame.isRunning);
        }

        // Replay interactions
        if (frame.interaction != GhostRecorder.InteractionType.None)
        {
            ReplayInteraction(frame.interaction);
        }

        currentFrame++;
    }

    void ReplayInteraction(GhostRecorder.InteractionType interaction)
    {
        switch (interaction)
        {
            case GhostRecorder.InteractionType.PickupKey:
                TryGhostPickupKey();
                break;
            case GhostRecorder.InteractionType.OpenGate:
                TryGhostOpenGate();
                break;
        }
    }

    void TryGhostPickupKey()
    {
        // Find the nearest key within range
        KeyPickup[] keys = FindObjectsByType<KeyPickup>(FindObjectsSortMode.None);
        foreach (KeyPickup key in keys)
        {
            if (!key.isPickedUp && key.gameObject.activeInHierarchy)
            {
                float dist = Vector3.Distance(transform.position, key.transform.position);
                if (dist < 3f) // generous range for ghost
                {
                    if (key.GhostPickup())
                    {
                        ghostHasKey = true;
                        Debug.Log("Ghost picked up key successfully.");
                    }
                    return;
                }
            }
        }
        // Key was already picked up by player or another ghost
        Debug.Log("Ghost tried to pick up key but it was already taken.");
    }

    void TryGhostOpenGate()
    {
        Gate[] gates = FindObjectsByType<Gate>(FindObjectsSortMode.None);
        foreach (Gate gate in gates)
        {
            if (!gate.isOpen && gate.gameObject.activeInHierarchy)
            {
                float dist = Vector3.Distance(transform.position, gate.transform.position);
                if (dist < 3f)
                {
                    if (ghostHasKey)
                    {
                        gate.GhostOpen(true);
                        Debug.Log("Ghost opened the gate.");
                    }
                    else
                    {
                        Debug.Log("Ghost has no key — cannot open gate.");
                        // Ghost didn't have the key. It will despawn on gate contact.
                        // (handled by GhostGateDespawn trigger on the gate)
                    }
                    return;
                }
            }
        }
    }

    /// <summary>
    /// Called when ghost touches a closed gate without a key.
    /// Fades out and destroys itself.
    /// </summary>
    public void DespawnAtGate()
    {
        if (isDespawning) return;
        isDespawning = true;
        isPlaying = false;
        StartCoroutine(FadeAndDestroy());
    }

    /// <summary>
    /// Called when ghost triggers a trap. Fades out and destroys itself.
    /// </summary>
    public void DespawnAtTrap()
    {
        if (isDespawning) return;
        isDespawning = true;
        isPlaying = false;
        StartCoroutine(FadeAndDestroy());
    }

    IEnumerator FadeAndDestroy()
    {
        if (animator != null)
        {
            animator.SetFloat("Speed", 0f);
        }

        float elapsed = 0f;
        // Store original colors
        Dictionary<Renderer, Color[]> originalColors = new Dictionary<Renderer, Color[]>();
        foreach (Renderer rend in renderers)
        {
            Color[] colors = new Color[rend.materials.Length];
            for (int i = 0; i < rend.materials.Length; i++)
            {
                colors[i] = rend.materials[i].color;
            }
            originalColors[rend] = colors;
        }

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = 1f - (elapsed / fadeDuration);

            foreach (Renderer rend in renderers)
            {
                if (originalColors.ContainsKey(rend))
                {
                    Color[] origColors = originalColors[rend];
                    for (int i = 0; i < rend.materials.Length; i++)
                    {
                        Color c = origColors[i];
                        c.a = Mathf.Max(0f, c.a * alpha);
                        rend.materials[i].color = c;
                    }
                }
            }

            yield return null;
        }

        Destroy(gameObject);
    }

    public bool HasKey()
    {
        return ghostHasKey;
    }
}