using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

/// <summary>
/// Shows tutorial text prompts at the right moments.
/// Uses trigger zones placed in the level.
/// </summary>

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }

    [Header("UI")]
    public TextMeshProUGUI promptText;
    public GameObject promptPanel;

    [Header("Timing")]
    public float defaultDisplayTime = 5f;
    private Coroutine hideCoroutine;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        HidePrompt();
        ShowPrompt("Welcome to Echo Alley!\nUse WASD to move. Hold SHIFT to run.", 6f);
    }

    public void ShowPrompt(string message, float duration = 0f)
    {
        if (hideCoroutine != null) StopCoroutine(hideCoroutine);
        promptText.text = message;
        promptPanel.SetActive(true);
        float displayTime = duration > 0 ? duration : defaultDisplayTime;
        hideCoroutine = StartCoroutine(HideAfterDelay(displayTime));
    }

    public void HidePrompt()
    {
        promptPanel.SetActive(false);
        promptText.text = "";
    }

    IEnumerator HideAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        HidePrompt();
    }

    //triggers when gate is opened 
    public void CompleteTutorial(string nextScene)
    {
        ShowPrompt("Nice! You're ready to start.", 2f);
        StartCoroutine(LoadNextSceneAfterDelay(nextScene));
    }

    IEnumerator LoadNextSceneAfterDelay(string sceneName)
    {
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene(sceneName);

    }
}
