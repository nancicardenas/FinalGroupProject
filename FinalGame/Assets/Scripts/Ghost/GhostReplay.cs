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

    [Header("Interaction Range")]
    public float interactRange = 5f; // generous range since ghost positions may drift slightly

    // Ghost index (0-7), assigned by GhostManager for shader selection
    [HideInInspector] public int ghostIndex = 0;

    // Cached references — found once at start, works even when objects are deactivated
    private KeyPickup[] allKeys;
    private Gate[] allGates;

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

        // Cache all interactables including inactive ones
        allKeys = Object.FindObjectsByType<KeyPickup>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        allGates = Object.FindObjectsByType<Gate>(FindObjectsInactive.Include, FindObjectsSortMode.None);
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
                animator.SetBool("IsRunning", false);
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
            animator.SetBool("IsGrounded", true); // ghosts are always "grounded" for animation purposes
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
        float closestDist = float.MaxValue;
        KeyPickup closestKey = null;

        foreach (KeyPickup key in allKeys)
        {
            if (key == null) continue;
            if (key.isPickedUp) continue;
            if (!key.gameObject.activeInHierarchy) continue;

            float dist = Vector3.Distance(transform.position, key.transform.position);
            if (dist < interactRange && dist < closestDist)
            {
                closestDist = dist;
                closestKey = key;
            }
        }

        if (closestKey != null)
        {
            if (closestKey.GhostPickup())
            {
                ghostHasKey = true;
                Debug.Log("Ghost " + ghostIndex + " picked up key successfully.");

                // Play interact animation on ghost
                if (animator != null)
                {
                    animator.SetTrigger("Interact");
                }
            }
        }
        else
        {
            Debug.Log("Ghost " + ghostIndex + " tried to pick up key but it was already taken.");
        }
    }

    void TryGhostOpenGate()
    {
        float closestDist = float.MaxValue;
        Gate closestGate = null;

        foreach (Gate gate in allGates)
        {
            if (gate == null) continue;
            if (gate.isOpen) continue;
            if (!gate.gameObject.activeInHierarchy) continue;

            float dist = Vector3.Distance(transform.position, gate.transform.position);
            if (dist < interactRange && dist < closestDist)
            {
                closestDist = dist;
                closestGate = gate;
            }
        }

        if (closestGate != null)
        {
            if (ghostHasKey)
            {
                closestGate.GhostOpen(true);
                Debug.Log("Ghost " + ghostIndex + " opened the gate.");
            }
            else
            {
                Debug.Log("Ghost " + ghostIndex + " has no key — cannot open gate.");
            }
        }
        else
        {
            Debug.Log("Ghost " + ghostIndex + " tried to open gate but no gate found in range. Closest gate distance check failed.");
        }
    }

    /// <summary>
    /// Called when ghost touches a closed gate without a key.
    /// </summary>
    public void DespawnAtGate()
    {
        if (isDespawning) return;
        isDespawning = true;
        isPlaying = false;
        StartCoroutine(FadeAndDestroy());
    }

    /// <summary>
    /// Called when ghost triggers a trap.
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
        Dictionary<Renderer, Color[]> originalColors = new Dictionary<Renderer, Color[]>();
        foreach (Renderer rend in renderers)
        {
            if (rend == null) continue;
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
                if (rend == null) continue;
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