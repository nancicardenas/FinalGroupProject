using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Records the player's position and rotation each fixed frame.
/// Attach to the Player object.
/// </summary>
public class GhostRecorder : MonoBehaviour
{
    // One frame of ghost data
    [System.Serializable]
    public struct GhostFrame
    {
        public Vector3 position;
        public Quaternion rotation;
        public float speed;      // for animations
        public bool isRunning;
    }

    // The complete recording of one run
    [HideInInspector]
    public List<GhostFrame> currentRecording = new List<GhostFrame>();
    private PlayerController playerController;
    private bool isRecording = false;

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

    void FixedUpdate()
    {
        if (!isRecording) return;
        GhostFrame frame = new GhostFrame
        {
            position = transform.position,
            rotation = transform.rotation,
            speed = playerController != null && playerController.isMoving
                ? (playerController.isRunning ? 1f : 0.5f) : 0f,
            isRunning = playerController != null && playerController.isRunning
        };
        currentRecording.Add(frame);
    }
}
