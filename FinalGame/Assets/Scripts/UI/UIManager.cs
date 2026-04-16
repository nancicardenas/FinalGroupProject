using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Title Screen")]
    public GameObject optionsPanel;

    [Header("Options")]
    public Slider volumeSlider;

    void Start()
    {
        // Make sure options panel starts hidden
        if (optionsPanel != null)
            optionsPanel.SetActive(false);

        // Load saved volume (default 1.0)
        if (volumeSlider != null)
        {
            float savedVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
            volumeSlider.value = savedVolume;
            AudioListener.volume = savedVolume;
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        }
    }

    // --- Title Screen Button Handlers ---
    public void OnPlayClicked()
    {
        SceneManager.LoadScene("CatSelection");
    }

    public void OnOptionsClicked()
    {
        if (optionsPanel != null)
            optionsPanel.SetActive(true);
    }

    public void OnExitClicked()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    public void OnBackClicked()
    {
        if (optionsPanel != null)
            optionsPanel.SetActive(false);
    }

    // --- Options ---
    void OnVolumeChanged(float value)
    {
        AudioListener.volume = value;
        PlayerPrefs.SetFloat("MasterVolume", value);
    }
}
