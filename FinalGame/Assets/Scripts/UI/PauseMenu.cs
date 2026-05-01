using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Handles the Escape key options overlay in any scene.
/// Attach to a PauseCanvas in every scene (or create one PauseCanvas prefab).
/// </summary>
public class PauseMenu : MonoBehaviour
{
    [Header("Panel")]
    public GameObject pausePanel;

    [Header("Sliders")]
    public Slider masterSlider;
    public Slider musicSlider;
    public Slider sfxSlider;

    [Header("Buttons")]
    public Button resumeButton;
    public Button returnToTitleButton;

    private bool isPaused = false;

    void Start()
    {
        pausePanel.SetActive(false);

        // Load saved values into sliders
        masterSlider.value = PlayerPrefs.GetFloat("MasterVolume", 0.1f);
        musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
        sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 0.8f);

        // Wire slider events
        masterSlider.onValueChanged.AddListener(OnMasterChanged);
        musicSlider.onValueChanged.AddListener(OnMusicChanged);
        sfxSlider.onValueChanged.AddListener(OnSFXChanged);

        // Wire buttons
        resumeButton.onClick.AddListener(Resume);
        returnToTitleButton.onClick.AddListener(ReturnToTitle);

        // Hide "Return to Title" if we're already on the title screen
        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene == "TitleScreen")
        {
            returnToTitleButton.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                Resume();
            else
                Pause();
        }
    }

    void Pause()
    {
        isPaused = true;
        pausePanel.SetActive(true);
        Time.timeScale = 0f; // freeze the game
    }

    void Resume()
    {
        isPaused = false;
        pausePanel.SetActive(false);
        Time.timeScale = 1f; // unfreeze the game
    }

    void OnMasterChanged(float value)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetMasterVolume(value);
    }

    void OnMusicChanged(float value)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetMusicVolume(value);
    }

    void OnSFXChanged(float value)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetSFXVolume(value);
    }

    void ReturnToTitle()
    {
        Time.timeScale = 1f; // unfreeze before loading
        if (AudioManager.Instance != null)
            AudioManager.Instance.StopMusic();
        SceneManager.LoadScene("TitleScreen");
    }
}