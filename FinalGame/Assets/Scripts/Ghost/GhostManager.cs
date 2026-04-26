using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

/// <summary>
/// Manages ghost recording and spawning.
/// Place on an empty GameObject in the Tutorial scene.
/// </summary>
public class GhostManager : MonoBehaviour
{
    [Header("References")]
    public GameObject ghostPrefab;
    public GhostRecorder playerRecorder;
    public PlayerLife playerLife;
    public GhostDetection ghostDetection;

    // Store all completed run recordings
    private List<List<GhostRecorder.GhostFrame>> savedRuns = new List<List<GhostRecorder.GhostFrame>>();
    // Currently active ghost objects
    public List<GameObject> activeGhosts = new List<GameObject>();

    //Event used to tell all dogs in the scene to select a new target
    public UnityEvent SelectNewDogTarget;

    void Start()
    {
        // Subscribe to player death/reset events
        if (playerLife != null)
        {
            playerLife.OnPlayerDied += OnRunEnded;
            playerLife.OnPlayerReset += OnNewRunStarting;
        }

        // Start recording the first run
        if (playerRecorder != null)
        {
            playerRecorder.StartRecording();
        }
    }

    void OnRunEnded()
    {
        // Save the recording from the run that just ended
        if (playerRecorder != null)
        {
            List<GhostRecorder.GhostFrame> recording = playerRecorder.GetRecording();
            if (recording.Count > 0)
            {
                savedRuns.Add(recording);
                Debug.Log("Ghost run saved. Total ghost runs: " + savedRuns.Count);
            }
            playerRecorder.StopRecording();
        }
    }

    void OnNewRunStarting()
    {
        // Destroy old ghost objects
        foreach (GameObject ghost in activeGhosts)
        {
            if (ghost != null) Destroy(ghost);
        }
        activeGhosts.Clear();

        // Spawn a ghost for each saved run
        for (int i = 0; i < savedRuns.Count && i < 8; i++)
        {
            SpawnGhost(savedRuns[i]);
        }
        
        //Make dogs select new target
        SelectNewDogTarget.Invoke();

        // Start recording the new run
        if (playerRecorder != null)
        {
            playerRecorder.StartRecording();
        }
    }

    void SpawnGhost(List<GhostRecorder.GhostFrame> frames)
    {
        if (ghostPrefab == null || frames.Count == 0) return;
        Vector3 startPos = frames[0].position;
        Quaternion startRot = frames[0].rotation;
        GameObject ghost = Instantiate(ghostPrefab, startPos, startRot);
        ghost.name = "Ghost_Run" + (savedRuns.IndexOf(frames) + 1);
        GhostReplay replay = ghost.GetComponent<GhostReplay>();

        if (replay != null)
        {
            replay.Initialize(frames);
        }
        activeGhosts.Add(ghost);
    }

    void OnDestroy()
    {
        // Unsubscribe from events
        if (playerLife != null)
        {
            playerLife.OnPlayerDied -= OnRunEnded;
            playerLife.OnPlayerReset -= OnNewRunStarting;
        }
    }
}
