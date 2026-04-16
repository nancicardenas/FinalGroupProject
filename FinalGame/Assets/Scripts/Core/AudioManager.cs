using UnityEngine;

/// <summary>
/// Simple audio manager. Persists across scenes.
/// For the prototype, handles SFX only (no background music per requirements).
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Clips")]
    public AudioClip pickupSound;
    public AudioClip gateOpenSound;
    public AudioClip deathSound;
    public AudioClip jumpSound;
    public AudioClip interactSound;
    private AudioSource audioSource;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Load saved volume
        AudioListener.volume = PlayerPrefs.GetFloat("MasterVolume", 1f);
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    // Convenience methods
    public void PlayPickup() => PlaySFX(pickupSound);
    public void PlayGateOpen() => PlaySFX(gateOpenSound);
    public void PlayDeath() => PlaySFX(deathSound);
    public void PlayJump() => PlaySFX(jumpSound);
}
