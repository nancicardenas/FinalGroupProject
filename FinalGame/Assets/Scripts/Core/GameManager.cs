using UnityEngine;

/// <summary>
/// Persists across scenes via DontDestroyOnLoad.
/// Stores the selected cat data for spawning in gameplay scenes.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("All Cat Prefabs (same order as CatSelector)")]
    public GameObject[] catPrefabs;

    [Header("All Animator Controllers (same order as CatSelector)")]
    public RuntimeAnimatorController[] animatorControllers;

    [HideInInspector] public int selectedCatIndex = 0;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        selectedCatIndex = PlayerPrefs.GetInt("SelectedCatIndex", 0);
    }

    public GameObject GetSelectedCatPrefab()
    {
        if (catPrefabs == null || catPrefabs.Length == 0) return null;
        int index = Mathf.Clamp(selectedCatIndex, 0, catPrefabs.Length - 1);
        return catPrefabs[index];
    }

    public RuntimeAnimatorController GetSelectedAnimatorController()
    {
        if (animatorControllers == null || animatorControllers.Length == 0) return null;
        int index = Mathf.Clamp(selectedCatIndex, 0, animatorControllers.Length - 1);
        return animatorControllers[index];
    }
}