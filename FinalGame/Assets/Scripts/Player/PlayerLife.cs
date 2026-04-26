using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerLife : MonoBehaviour
{
    [Header("Lives")]
    public int maxLives = 9;
    public int currentLives;

    [Header("Spawn")]
    public Transform spawnPoint;

    [Header("Manual Reset")]
    public KeyCode resetKey = KeyCode.Mouse1; // Right Click

    // Events that other scripts can subscribe to
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

        // Notify listeners (GhostManager saves the recording here)
        OnPlayerDied?.Invoke();

        if (currentLives <= 0)
        {
            Debug.Log("All lives gone! Full level reset.");
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            return;
        }

        // Reset the level but keep ghost data
        OnPlayerReset?.Invoke();
        ResetPlayer();
    }

    void ResetPlayer()
    {
        isDead = false;

        // Reset position
        CharacterController cc = GetComponent<CharacterController>();
        if (cc != null)
        {
            cc.enabled = false;
            transform.position = spawnPoint.position;
            cc.enabled = true;
        }

        // Reset key
        PlayerInteraction interaction = GetComponent<PlayerInteraction>();
        if (interaction != null)
        {
            interaction.hasKey = false;
        }

        // Reset ALL interactables
        ResetAllInteractables();
    }

    void ResetAllInteractables()
    {
        // Reset keys
        KeyPickup[] keys = FindObjectsByType<KeyPickup>(FindObjectsSortMode.None);
        foreach (var key in keys)
        {
            key.ResetKey();
        }

        // Reset gates
        Gate[] gates = FindObjectsByType<Gate>(FindObjectsSortMode.None);
        foreach (var gate in gates)
        {
            gate.ResetGate();
        }

        // Reset traps
        TrapZone[] traps = FindObjectsByType<TrapZone>(FindObjectsSortMode.None);
        foreach (var trap in traps)
        {
            trap.ResetTrap();
        }

        // Reset exit door lock state
        ExitDoor[] doors = FindObjectsByType<ExitDoor>(FindObjectsSortMode.None);
        foreach (var door in doors)
        {
            door.ResetDoor();
        }
    }
}