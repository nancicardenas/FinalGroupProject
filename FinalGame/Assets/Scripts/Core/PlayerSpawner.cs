using UnityEngine;

/// <summary>
/// Spawns the player with the correct cat model and animator at level start.
/// Attach to an empty GameObject in the Tutorial scene (or any gameplay scene).
/// </summary>
public class PlayerSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public Transform spawnPoint;
    public GameObject playerShellPrefab; // Player prefab without cat model

    [Header("Scene References to Wire After Spawn")]
    public CameraFollow cameraFollow;
    public GhostManager ghostManager;
    public LivesUI livesUI;

    [Header("Fallback (if GameManager not found — for testing)")]
    public GameObject fallbackCatPrefab;
    public RuntimeAnimatorController fallbackAnimController;

    private GameObject spawnedPlayer;

    void Start()
    {
        SpawnPlayer();
    }

    void SpawnPlayer()
    {
        // Instantiate the player shell at spawn point
        spawnedPlayer = Instantiate(playerShellPrefab, spawnPoint.position, Quaternion.identity);
        spawnedPlayer.name = "Player";
        spawnedPlayer.tag = "Player";

        // Read selected cat index from PlayerPrefs
        int catIndex = PlayerPrefs.GetInt("SelectedCatIndex", 0);

        // Get the cat prefab and animator controller
        GameObject catPrefab = null;
        RuntimeAnimatorController animController = null;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.selectedCatIndex = catIndex;
            catPrefab = GameManager.Instance.GetSelectedCatPrefab();
            animController = GameManager.Instance.GetSelectedAnimatorController();
        }

        // Fallback for testing (when launching Tutorial scene directly)
        if (catPrefab == null) catPrefab = fallbackCatPrefab;
        if (animController == null) animController = fallbackAnimController;

        if (catPrefab != null)
        {
            // Spawn cat model as child of player
            GameObject catModel = Instantiate(catPrefab, spawnedPlayer.transform);
            catModel.name = "CatModel";
            catModel.transform.localPosition = Vector3.zero;
            catModel.transform.localRotation = Quaternion.identity;

            // Assign the breed-specific animator controller
            Animator anim = catModel.GetComponent<Animator>();
            if (anim == null) anim = catModel.GetComponentInChildren<Animator>();

            if (anim != null && animController != null)
            {
                anim.runtimeAnimatorController = animController;
                anim.applyRootMotion = false;
            }

            // Add the PlayerAnimator script to drive animations
            if (catModel.GetComponent<PlayerAnimator>() == null)
            {
                catModel.AddComponent<PlayerAnimator>();
            }
        }

        // Wire scene references
        PlayerLife life = spawnedPlayer.GetComponent<PlayerLife>();
        if (life != null)
        {
            life.spawnPoint = spawnPoint;
        }

        if (cameraFollow != null)
        {
            cameraFollow.target = spawnedPlayer.transform;
        }

        // Wire GhostManager
        GhostRecorder recorder = spawnedPlayer.GetComponent<GhostRecorder>();
        if (ghostManager != null && life != null && recorder != null)
        {
            ghostManager.playerRecorder = recorder;
            ghostManager.playerLife = life;

            // GhostManager subscribes to events in its own Start(),
            // but player was just created, so we need to subscribe manually
            Debug.Log("Subscribing GhostManager from SpawnPlayer");
            life.OnPlayerDied += ghostManager.OnRunEndedPublic;
            life.OnPlayerReset += ghostManager.OnNewRunStartingPublic;

            // Start the first recording
            recorder.StartRecording();
        }

        if (livesUI != null)
        {
            livesUI.playerLife = life;
        }
        
        //Wire references for Dog
        //TODO change to foreach loop if multiple dogs
        DogAI dogAI = FindFirstObjectByType<DogAI>();
        if (dogAI != null)
        {
            dogAI.playerLife = spawnedPlayer.gameObject.GetComponent<PlayerLife>();
            dogAI.target = spawnedPlayer.transform;
            dogAI.player = spawnedPlayer.transform;
        }
        
        GhostDetection ghostDetection = FindFirstObjectByType<GhostDetection>();
        if (ghostDetection != null)
        {
            ghostDetection.player =  spawnedPlayer.transform;
        }
    }
}