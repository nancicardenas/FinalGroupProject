using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SkipToLevels : MonoBehaviour
{
    [SerializeField] private bool skipToLevels = true;

    public enum Levels : int
    {
        greekIsland,
        woodland,
        asia
    }

    public Levels levels;
    
    private void Start()
    {
        if (skipToLevels)
        {
            PlayerPrefs.SetInt("SelectedCatIndex", 0);
            PlayerPrefs.Save();
            switch (levels)
            {
                case Levels.greekIsland:
                    SceneManager.LoadScene("AncientGreekIsland");
                    break;
                case Levels.woodland:
                    SceneManager.LoadScene("AlpineWoodland");
                    break;
                case Levels.asia:
                    SceneManager.LoadScene("Asia");
                    break;
            }
        }
        else
        {
            PlayerPrefs.SetInt("SelectedCatIndex", 0);
        }
    }
}
