using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SkipToLevels : MonoBehaviour
{
    [SerializeField] private bool skipToLevels = true;

    private void Start()
    {
        if (skipToLevels)
        {
            PlayerPrefs.SetInt("SelectedCatIndex", 0);
            PlayerPrefs.Save();
            SceneManager.LoadScene("AncientGreekIsland");
        }
        else
        {
            PlayerPrefs.SetInt("SelectedCatIndex", 0);
        }
    }
}
