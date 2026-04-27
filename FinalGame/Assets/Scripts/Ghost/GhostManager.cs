using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

/// <summary>
/// Manages ghost recording, spawning, and material assignment.
/// </summary>
public class GhostManager : MonoBehaviour
{
    [Header("References")]
    public GameObject ghostPrefab;
    public GhostRecorder playerRecorder;
    public PlayerLife playerLife;
    public GhostDetection ghostDetection;

    [Header("Ghost Materials (8 — one per ghost)")]
    public Material[] ghostMaterials; // Assign 8 materials in Inspector

    //Event used to tell all dogs in the scene to select a new target
    public UnityEvent SelectNewDogTarget;

    // Store all completed run recordings
    private List<List<GhostRecorder.GhostFrame>> savedRuns = new List<List<GhostRecorder.GhostFrame>>();

    // Currently active ghost objects
    public List<GameObject> activeGhosts = new List<GameObject>();

    void Start()
    {
        //TODO Delete or Keep later
        //This was a duplicate subscription to the UnityEvents, same functionality in PlayerSpawner since the player is spawned at start anyways
        
        // If playerLife and playerRecorder are already assigned (e.g., manually in Inspector),
        // subscribe and start recording. Otherwise, PlayerSpawner handles this.
        /*if (playerLife != null && playerRecorder != null)
        {
            Debug.Log("Subscribing GhostManager from GhostManager.Start");
            playerLife.OnPlayerDied += OnRunEnded;
            playerLife.OnPlayerReset += OnNewRunStarting;
            playerRecorder.StartRecording();
        }*/
    }

    void OnRunEnded()
    {
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

        // Spawn a ghost for each saved run (max 8)
        for (int i = 0; i < savedRuns.Count && i < 8; i++)
        {
            SpawnGhost(savedRuns[i], i);
        }

        //Make dogs select new target
        SelectNewDogTarget.Invoke();

        // Start recording the new run
        if (playerRecorder != null)
        {
            playerRecorder.StartRecording();
        }
    }

    void SpawnGhost(List<GhostRecorder.GhostFrame> frames, int ghostIndex)
    {
        if (ghostPrefab == null || frames.Count == 0) return;

        Vector3 startPos = frames[0].position;
        Quaternion startRot = frames[0].rotation;

        GameObject ghost = Instantiate(ghostPrefab, startPos, startRot);
        ghost.name = "Ghost_Run" + (ghostIndex + 1);

        // Add the cat model to the ghost
        AddCatModelToGhost(ghost);

        // Apply unique ghost material
        ApplyGhostMaterial(ghost, ghostIndex);

        // Initialize replay
        GhostReplay replay = ghost.GetComponent<GhostReplay>();
        if (replay != null)
        {
            replay.ghostIndex = ghostIndex;
            replay.Initialize(frames);
        }

        activeGhosts.Add(ghost);
    }

    void AddCatModelToGhost(GameObject ghost)
    {
        // Get the same cat prefab the player is using
        GameObject catPrefab = null;
        RuntimeAnimatorController animController = null;

        if (GameManager.Instance != null)
        {
            catPrefab = GameManager.Instance.GetSelectedCatPrefab();
            animController = GameManager.Instance.GetSelectedAnimatorController();
        }

        if (catPrefab == null) return;

        // Spawn cat model as child of ghost
        GameObject model = Instantiate(catPrefab, ghost.transform);
        model.name = "GhostModel";
        model.transform.localPosition = Vector3.zero;
        model.transform.localRotation = Quaternion.identity;

        // Assign animator controller
        Animator anim = model.GetComponent<Animator>();
        if (anim == null) anim = model.GetComponentInChildren<Animator>();
        if (anim != null && animController != null)
        {
            anim.runtimeAnimatorController = animController;
            anim.applyRootMotion = false;
        }

        // Remove any colliders from the ghost model (ghosts are non-physical)
        Collider[] colliders = model.GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders)
        {
            Destroy(col);
        }
    }

    void ApplyGhostMaterial(GameObject ghost, int index)
    {
        if (ghostMaterials == null || ghostMaterials.Length == 0) return;

        // Clamp index to available materials
        int matIndex = index % ghostMaterials.Length;
        Material mat = ghostMaterials[matIndex];

        if (mat == null) return;

        // Apply to all renderers on the ghost
        Renderer[] renderers = ghost.GetComponentsInChildren<Renderer>();
        foreach (Renderer rend in renderers)
        {
            // Replace all material slots with the ghost material
            Material[] mats = new Material[rend.materials.Length];
            for (int i = 0; i < mats.Length; i++)
            {
                mats[i] = mat;
            }
            rend.materials = mats;
        }
    }

    void OnDestroy()
    {
        if (playerLife != null)
        {
            playerLife.OnPlayerDied -= OnRunEnded;
            playerLife.OnPlayerReset -= OnNewRunStarting;
        }
    }

    // --- Public wrappers for PlayerSpawner event subscription ---
    public void OnRunEndedPublic()
    {
        Debug.Log("OnRunEndedPublic called");
        OnRunEnded();
    }
    public void OnNewRunStartingPublic()
    {
        OnNewRunStarting();
    }
}