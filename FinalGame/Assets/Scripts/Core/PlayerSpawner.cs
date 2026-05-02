using UnityEngine;

/// <summary>
/// Spawns the player with the correct cat model and animator at level start.
/// Attach to an empty GameObject in the Tutorial scene (or any gameplay scene).
/// </summary>
public class PlayerSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public Transform spawnPoint;
    public GameObject playerShellPrefab;

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
        spawnedPlayer = Instantiate(playerShellPrefab, spawnPoint.position, Quaternion.identity);
        spawnedPlayer.name = "Player";
        spawnedPlayer.tag = "Player";

        int catIndex = PlayerPrefs.GetInt("SelectedCatIndex", 0);

        GameObject catPrefab = null;
        RuntimeAnimatorController animController = null;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.selectedCatIndex = catIndex;
            catPrefab = GameManager.Instance.GetSelectedCatPrefab();
            animController = GameManager.Instance.GetSelectedAnimatorController();
        }

        if (catPrefab == null) catPrefab = fallbackCatPrefab;
        if (animController == null) animController = fallbackAnimController;

        if (catPrefab != null)
        {
            GameObject catModel = Instantiate(catPrefab, spawnedPlayer.transform);
            catModel.name = "CatModel";
            catModel.transform.localPosition = Vector3.zero;
            catModel.transform.localRotation = Quaternion.identity;

            Animator anim = catModel.GetComponent<Animator>();
            if (anim == null) anim = catModel.GetComponentInChildren<Animator>();

            if (anim != null && animController != null)
            {
                anim.runtimeAnimatorController = animController;
                anim.applyRootMotion = false;
            }

            if (catModel.GetComponent<PlayerAnimator>() == null)
            {
                catModel.AddComponent<PlayerAnimator>();
            }
        }

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

            life.OnPlayerDied += ghostManager.OnRunEndedPublic;
            life.OnPlayerReset += ghostManager.OnNewRunStartingPublic;

            recorder.StartRecording();
        }

        if (livesUI != null)
        {
            livesUI.playerLife = life;
        }

        // Wire all dogs in the scene
        DogAI[] allDogs = FindObjectsByType<DogAI>(FindObjectsSortMode.None);
        foreach (DogAI dogAI in allDogs)
        {
            dogAI.playerLife = life;
            dogAI.target = spawnedPlayer.transform;
            dogAI.player = spawnedPlayer.transform;
            dogAI.ghostManager = ghostManager;

            if (life != null)
            {
                life.OnPlayerReset += dogAI.OnPlayerDied;
            }
        }

        // Wire all ghost detections in the scene
        GhostDetection[] allDetections = FindObjectsByType<GhostDetection>(FindObjectsSortMode.None);
        foreach (GhostDetection ghostDetection in allDetections)
        {
            ghostDetection.player = spawnedPlayer.transform;
        }
    }
}