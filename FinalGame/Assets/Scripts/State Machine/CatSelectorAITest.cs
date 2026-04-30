using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Handles cat selection. Stores the selected index in PlayerPrefs
/// so the Tutorial scene can read it and spawn the right cat.
/// </summary>
public class CatSelectorAITest : MonoBehaviour
{
    [Header("Cat Prefabs (specific color variants in order)")]
    public GameObject[] catPrefabs;
    // 0: ChibiCatV1_americanCurl_generic_grey
    // 1: ChibiCatV1_bobtail_generic_carey
    // 2: ChibiCatV1_european_generic_siam
    // 3: ChibiCatV1_persian_generic_orange
    // 4: ChibiCatV1_sphinx_generic_black

    [Header("Animator Controllers (one per breed, same order)")]
    public RuntimeAnimatorController[] animatorControllers;
    // 0: AC_AmericanCurl
    // 1: AC_Bobtail
    // 2: AC_European
    // 3: AC_Persian
    // 4: AC_Sphinx

    [Header("Cat Display Names")]
    public string[] catNames;

    [Header("UI References")]
    public Button confirmButton;
    public TextMeshProUGUI selectedText;
    public Button[] catButtons; // 5 buttons in same order

    private int selectedIndex = -1;

    void Start()
    {
        confirmButton.interactable = false;

        for (int i = 0; i < catButtons.Length; i++)
        {
            int index = i; // capture for closure
            catButtons[i].onClick.AddListener(() => SelectCat(index));
        }

        confirmButton.onClick.AddListener(OnConfirm);
    }

    void SelectCat(int index)
    {
        selectedIndex = index;
        selectedText.text = "Selected: " + catNames[index];
        confirmButton.interactable = true;

        // Visual feedback: highlight selected, dim others
        for (int i = 0; i < catButtons.Length; i++)
        {
            ColorBlock colors = catButtons[i].colors;
            if (i == index)
            {
                colors.normalColor = new Color(0.5f, 0.8f, 0.5f, 1f); // green highlight
            }
            else
            {
                colors.normalColor = Color.white;
            }
            catButtons[i].colors = colors;
        }
    }

    void OnConfirm()
    {
        if (selectedIndex < 0) return;

        // Save selection for other scenes to read
        PlayerPrefs.SetInt("SelectedCatIndex", selectedIndex);
        PlayerPrefs.Save();

        SceneManager.LoadScene("Human AI Test");
    }
}