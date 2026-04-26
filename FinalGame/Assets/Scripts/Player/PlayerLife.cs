using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages lives, death, and per-life reset of all interactables.
/// At the start of each new life:
///   - Player teleports to spawn, loses key
///   - Key respawns (no one holds it)
///   - Gate respawns closed
///   - Trap respawns active
///   - Door resets
/// </summary>
public class PlayerLife : MonoBehaviour
{
    [Header("Lives")]
    public int maxLives = 9;
    public int currentLives;

    [Header("Spawn")]
    public Transform spawnPoint;

    [Header("Manual Reset")]
    public KeyCode resetKey = KeyCode.Mouse1; // Right Click

    // Events for GhostManager
    public System.Action OnPlayerDied;
    public System.Action OnPlayerReset;

    private bool isDead = false;

    void Start()
    {
        currentLives = maxLives;
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

        currentLives--;
        Debug.Log("Died! Lives remaining: " + currentLives);

        if (AudioManager.Instance != null) AudioManager.Instance.PlayDeath();

        // Save the ghost recording before anything resets
        OnPlayerDied?.Invoke();

        if (currentLives <= 0)
        {
            Debug.Log("All lives gone! Full level reset.");
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            return;
        }

        // Spawn ghosts from saved recordings
        OnPlayerReset?.Invoke();

        // Reset everything for the new life
        ResetForNewLife();
    }

    void ResetForNewLife()
    {
        isDead = false;

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

        // Reset all world objects
        ResetAllInteractables();
    }

    void ResetAllInteractables()
    {
        KeyPickup[] keys = FindObjectsByType<KeyPickup>(FindObjectsSortMode.None);
        foreach (var key in keys)
        {
            key.ResetKey();
        }

        Gate[] gates = FindObjectsByType<Gate>(FindObjectsSortMode.None);
        foreach (var gate in gates)
        {
            gate.ResetGate();
        }

        TrapZone[] traps = FindObjectsByType<TrapZone>(FindObjectsSortMode.None);
        foreach (var trap in traps)
        {
            trap.ResetTrap();
        }

        ExitDoor[] doors = FindObjectsByType<ExitDoor>(FindObjectsSortMode.None);
        foreach (var door in doors)
        {
            door.ResetDoor();
        }
    }
}