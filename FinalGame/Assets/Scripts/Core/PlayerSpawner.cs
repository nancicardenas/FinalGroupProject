using UnityEngine;

/// <summary>
/// Spawns the player at the spawn point with the selected cat model.
/// Attach to an empty GameObject in the Tutorial scene.
/// Assigns references that other scripts need.
/// </summary>
public class PlayerSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public Transform spawnPoint;
    public GameObject playerShellPrefab; // The Player prefab WITHOUT a cat model child

    [Header("Scene References to Wire")]
    public CameraFollow cameraFollow;
    public GhostManager ghostManager;
    public LivesUI livesUI;

    void Start()
    {
        SpawnPlayer();
    }

    void SpawnPlayer()
    {
        // Instantiate the player shell
        GameObject player = Instantiate(playerShellPrefab, spawnPoint.position, Quaternion.identity);
        player.name = "Player";

        // Get selected cat model
        GameObject catPrefab = null;
        if (GameManager.Instance != null)
        {
            catPrefab = GameManager.Instance.GetSelectedCatPrefab();
        }

        if (catPrefab != null)
        {
            // Spawn cat model as child of player
            GameObject catModel = Instantiate(catPrefab, player.transform);
            catModel.name = "CatModel";
            catModel.transform.localPosition = Vector3.zero;
            catModel.transform.localRotation = Quaternion.identity;

            // Set up animator
            Animator anim = catModel.GetComponent<Animator>();
            if (anim != null)
            {
                // Assign your PlayerAnimator controller
                // You'll need a reference or load from Resources
                anim.applyRootMotion = false;
            }
            // Add PlayerAnimator script to the cat model
            catModel.AddComponent<PlayerAnimator>();
        }

        // Wire references
        PlayerLife life = player.GetComponent<PlayerLife>();
        if (life != null)
        {
            life.spawnPoint = spawnPoint;
        }

        if (cameraFollow != null)
        {
            cameraFollow.target = player.transform;
        }

        GhostRecorder recorder = player.GetComponent<GhostRecorder>();

        if (ghostManager != null)
        {
            ghostManager.playerRecorder = recorder;
            ghostManager.playerLife = life;

            // Re-subscribe events since player was just created
            if (life != null)
            {
                life.OnPlayerDied += () => {
                    // GhostManager handles this
                };
            }
        }
        if (livesUI != null)
        {
            livesUI.playerLife = life;
        }
    }
}
