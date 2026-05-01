using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Title Screen")]
    public GameObject optionsPanel;

    [Header("Options Sliders")]
    public Slider masterSlider;
    public Slider musicSlider;
    public Slider sfxSlider;

    void Start()
    {
        if (optionsPanel != null)
            optionsPanel.SetActive(false);

        // Load saved volumes
        if (masterSlider != null)
        {
            masterSlider.value = PlayerPrefs.GetFloat("MasterVolume", 0.1f);
            masterSlider.onValueChanged.AddListener(OnMasterChanged);
        }

        if (musicSlider != null)
        {
            musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
            musicSlider.onValueChanged.AddListener(OnMusicChanged);
        }

        if (sfxSlider != null)
        {
            sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 0.8f);
            sfxSlider.onValueChanged.AddListener(OnSFXChanged);
        }
    }

    public void OnPlayClicked()
    {
        SceneManager.LoadScene("IntroCutscene");
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
}