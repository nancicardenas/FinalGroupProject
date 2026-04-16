using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Replays a recorded ghost path. Attach to a Ghost prefab.
/// </summary>
public class GhostReplay : MonoBehaviour
{
    private List<GhostRecorder.GhostFrame> frames;
    private int currentFrame = 0;
    private bool isPlaying = false;
    private Animator animator;

    public void Initialize(List<GhostRecorder.GhostFrame> recordedFrames)
    {
        frames = recordedFrames;
        currentFrame = 0;
        isPlaying = true;
    }

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
    }

    void FixedUpdate()
    {
        if (!isPlaying || frames == null) return;

        if (currentFrame >= frames.Count)
        {
            // Recording ended — ghost just stops
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
        currentFrame++;
    }
}
