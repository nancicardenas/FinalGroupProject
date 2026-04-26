using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Drives a sequence of image+text slides for story cutscenes.
/// Attach to the Canvas in IntroCutscene or EndingCutscene scenes.
/// </summary>
public class CutsceneManager : MonoBehaviour
{
    [Header("Slide Data")]
    public Sprite[] slideImages;
    [TextArea(3, 8)]
    public string[] slideTexts;

    [Header("UI References")]
    public Image displayImage;
    public TextMeshProUGUI storyText;
    public TextMeshProUGUI continuePrompt;
    public CanvasGroup fadeGroup;

    [Header("Settings")]
    public float fadeDuration = 0.8f;
    public float textSpeed = 0.03f; // seconds per character
    public string nextSceneName = "CatSelection";

    private int currentSlide = 0;
    private bool isTransitioning = false;
    private bool textFinished = false;
    private Coroutine typingCoroutine;

    void Start()
    {
        continuePrompt.gameObject.SetActive(false);
        ShowSlide(0);
    }

    void Update()
    {
        if (isTransitioning) return;

        // Any key or click to advance
        if (Input.anyKeyDown || Input.GetMouseButtonDown(0))
        {
            if (!textFinished)
            {
                // Skip typewriter — show full text immediately
                if (typingCoroutine != null) StopCoroutine(typingCoroutine);
                storyText.text = slideTexts[currentSlide];
                textFinished = true;
                continuePrompt.gameObject.SetActive(true);
            }
            else
            {
                // Advance to next slide
                currentSlide++;
                if (currentSlide >= slideImages.Length)
                {
                    // Done — go to next scene
                    StartCoroutine(FadeAndLoadScene());
                }
                else
                {
                    StartCoroutine(TransitionToSlide(currentSlide));
                }
            }
        }
    }

    void ShowSlide(int index)
    {
        displayImage.sprite = slideImages[index];
        displayImage.preserveAspect = true;
        continuePrompt.gameObject.SetActive(false);
        textFinished = false;
        typingCoroutine = StartCoroutine(TypeText(slideTexts[index]));
    }

    IEnumerator TypeText(string fullText)
    {
        storyText.text = "";
        foreach (char c in fullText)
        {
            storyText.text += c;
            yield return new WaitForSeconds(textSpeed);
        }
        textFinished = true;
        continuePrompt.gameObject.SetActive(true);
    }

    IEnumerator TransitionToSlide(int index)
    {
        isTransitioning = true;

        // Fade out
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            fadeGroup.alpha = 1f - (t / fadeDuration);
            yield return null;
        }
        fadeGroup.alpha = 0f;

        // Swap content
        ShowSlide(index);

        // Fade in
        t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            fadeGroup.alpha = t / fadeDuration;
            yield return null;
        }
        fadeGroup.alpha = 1f;

        isTransitioning = false;
    }

    IEnumerator FadeAndLoadScene()
    {
        isTransitioning = true;

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            fadeGroup.alpha = 1f - (t / fadeDuration);
            yield return null;
        }

        SceneManager.LoadScene(nextSceneName);
    }
}