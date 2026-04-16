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
    // Event that other scripts can subscribe to
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
        if (AudioManager.Instance != null) AudioManager.Instance.PlayDeath();
        isDead = true;
        currentLives--;
        Debug.Log("Died! Lives remaining: " + currentLives);
        // Notify listeners (GhostManager will use this)
        OnPlayerDied?.Invoke();

        if (currentLives <= 0)
        {
            Debug.Log("All lives gone! Full level reset.");
            // Reload the entire scene, clearing all ghosts
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
            cc.enabled = false; // must disable to teleport
            transform.position = spawnPoint.position;
            cc.enabled = true;
        }

        // Reset key
        PlayerInteraction interaction = GetComponent<PlayerInteraction>();

        if (interaction != null)
        {
            interaction.hasKey = false;
        }

        // Re-enable interactables
        // (Key and Gate need to come back for each attempt)
        ResetInteractables();
    }

    void ResetInteractables()
    {
        // Find all keys and gates and re-enable them
        KeyPickup[] keys = FindObjectsByType<KeyPickup>(FindObjectsSortMode.None);

        foreach (var key in keys)
        {
            key.gameObject.SetActive(true);
        }

        Gate[] gates = FindObjectsByType<Gate>(FindObjectsSortMode.None);

        foreach (var gate in gates)
        {
            gate.gameObject.SetActive(true);
        }
    }
}
