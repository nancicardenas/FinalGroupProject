using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Manages lives, death, and per-life reset of all interactables.
/// </summary>
public class PlayerLife : MonoBehaviour
{
    [Header("Lives")]
    public int maxLives = 9;
    public int currentLives;

    [Header("Spawn")]
    public Transform spawnPoint;

    [Header("Manual Reset")]
    public KeyCode resetKey = KeyCode.Mouse1;

    public System.Action OnPlayerDied;
    public System.Action OnPlayerReset;

    private bool isDead = false;
    public bool IsDead { get { return isDead; } }

    // Cached references — found once, used every reset
    private KeyPickup[] allKeys;
    private Gate[] allGates;
    private TrapZone[] allTraps;
    private ExitDoor[] allDoors;

    void Start()
    {
        currentLives = maxLives;

        // Cache ALL interactables at scene start, including inactive
        allKeys = Object.FindObjectsByType<KeyPickup>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        allGates = Object.FindObjectsByType<Gate>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        allTraps = Object.FindObjectsByType<TrapZone>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        allDoors = Object.FindObjectsByType<ExitDoor>(FindObjectsInactive.Include, FindObjectsSortMode.None);
    }

    void Update()
    {
        if (Input.GetKeyDown(resetKey) && !isDead)
        {
            Die();
        }
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;

        StartCoroutine(DeathSequence());
    }

    private IEnumerator DeathSequence()
    {
        currentLives--;
        Debug.Log("Died! Lives remaining: " + currentLives);

        if (AudioManager.Instance != null) AudioManager.Instance.PlayDeath();

        // Play death animation
        PlayerAnimator playerAnimator = GetComponentInChildren<PlayerAnimator>();
        if (playerAnimator != null)
        {
            playerAnimator.TriggerDeath();
        }

        // Save ghost recording
        OnPlayerDied?.Invoke();

        // Wait for death animation to play
        yield return new WaitForSeconds(1f);

        if (currentLives <= 0)
        {
            Debug.Log("All lives gone! Full level reset.");
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            yield break;
        }

        // Spawn ghosts
        OnPlayerReset?.Invoke();

        // Reset everything for new life
        ResetForNewLife();
    }

    void ResetForNewLife()
    {
        isDead = false;

        // Play respawn animation
        PlayerAnimator playerAnimator = GetComponentInChildren<PlayerAnimator>();
        if (playerAnimator != null)
        {
            playerAnimator.TriggerRespawn();
        }

        // Teleport player to spawn
        CharacterController cc = GetComponent<CharacterController>();
        if (cc != null)
        {
            cc.enabled = false;
            transform.position = spawnPoint.position;
            cc.enabled = true;
        }

        // Clear player's key
        PlayerInteraction interaction = GetComponent<PlayerInteraction>();
        if (interaction != null)
        {
            interaction.hasKey = false;
        }

        // Reset all interactables using cached references
        ResetAllInteractables();
    }

    void ResetAllInteractables()
    {
        foreach (var key in allKeys)
        {
            if (key != null) key.ResetKey();
        }

        foreach (var gate in allGates)
        {
            if (gate != null) gate.ResetGate();
        }

        foreach (var trap in allTraps)
        {
            if (trap != null) trap.ResetTrap();
        }

        foreach (var door in allDoors)
        {
            if (door != null) door.ResetDoor();
        }
    }
}