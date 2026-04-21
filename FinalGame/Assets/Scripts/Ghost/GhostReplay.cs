using System.Collections;
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
    
    private float despawnDuration = 3f;
    private GhostManager ghostManager;
    private bool delayedDespawnRunning;

    public void Initialize(List<GhostRecorder.GhostFrame> recordedFrames)
    {
        frames = recordedFrames;
        currentFrame = 0;
        isPlaying = true;
    }

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        ghostManager = GameObject.FindGameObjectWithTag("GhostManager").GetComponent<GhostManager>();
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
            
            //Remove ghost from activeGhosts, Wait three seconds before disappearing, change target to different ghost or back to player
            //TODO change to have different distinctions, (ex. permanent lifespan when on a pressure plate, temporary everywhere else
            ghostManager.activeGhosts.Remove(gameObject);
            ghostManager.SelectNewDogTarget.Invoke();
            Destroy(gameObject, despawnDuration);
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

    /*IEnumerator DelayedDespawn()
    {
        delayedDespawnRunning = true;
        yield return new WaitForSeconds(despawnDuration);
        Destroy(gameObject);
        delayedDespawnRunning = false;
    }*/
}
