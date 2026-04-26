using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Records the player's position, rotation, and interaction events each fixed frame.
/// Attach to the Player object.
/// </summary>
public class GhostRecorder : MonoBehaviour
{
    // Types of interactions a ghost can replay
    public enum InteractionType
    {
        None,
        PickupKey,
        OpenGate
    }

    // One frame of ghost data
    [System.Serializable]
    public struct GhostFrame
    {
        public Vector3 position;
        public Quaternion rotation;
        public float speed;
        public bool isRunning;
        public InteractionType interaction;
    }

    [HideInInspector]
    public List<GhostFrame> currentRecording = new List<GhostFrame>();

    private PlayerController playerController;
    private bool isRecording = false;

    // Queue interactions to be stamped on the next fixed frame
    private InteractionType pendingInteraction = InteractionType.None;

    void Start()
    {
        playerController = GetComponent<PlayerController>();
    }

    public void StartRecording()
    {
        currentRecording.Clear();
        isRecording = true;
    }

    public void StopRecording()
    {
        isRecording = false;
    }

    public List<GhostFrame> GetRecording()
    {
        return new List<GhostFrame>(currentRecording);
    }

    /// <summary>
    /// Call this from interaction scripts when the player performs an action.
    /// The interaction will be stamped on the next recorded frame.
    /// </summary>
    public void RecordInteraction(InteractionType type)
    {
        pendingInteraction = type;
    }

    void FixedUpdate()
    {
        if (!isRecording) return;

        GhostFrame frame = new GhostFrame
        {
            position = transform.position,
            rotation = transform.rotation,
            speed = playerController != null && playerController.isMoving
                ? (playerController.isRunning ? 1f : 0.5f) : 0f,
            isRunning = playerController != null && playerController.isRunning,
            interaction = pendingInteraction
        };

        currentRecording.Add(frame);

        // Clear pending interaction after recording it
        pendingInteraction = InteractionType.None;
    }
}