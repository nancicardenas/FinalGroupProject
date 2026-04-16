using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class CatSelector : MonoBehaviour
{
    [Header("Cat Prefabs — assign in order")]
    public GameObject[] catPrefabs; // 6 prefabs: AmericanCurl, Bobtail, European, Persian, ScottishFold, Sphinx

    [Header("Cat Names — must match prefab order")]
    public string[] catNames;

    [Header("UI References")]
    public Button confirmButton;
    public TextMeshProUGUI selectedText;
    public Button[] catButtons; // 6 buttons in same order as prefabs
    private int selectedIndex = -1;

    void Start()
    {
        confirmButton.interactable = false;

        // Wire each cat button
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

        // Visual feedback: highlight selected button
        for (int i = 0; i < catButtons.Length; i++)
        {
            ColorBlock colors = catButtons[i].colors;
            if (i == index)
            {
                colors.normalColor = new Color(0.5f, 0.8f, 0.5f, 1f); // green tint
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

        // Save selected cat index so Tutorial scene can read it
        PlayerPrefs.SetInt("SelectedCatIndex", selectedIndex);
        PlayerPrefs.Save();
        SceneManager.LoadScene("Tutorial");
    }
}
