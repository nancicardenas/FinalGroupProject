using UnityEngine;

/// <summary>
/// Persists across scenes. Holds the selected cat prefab reference.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("All Cat Prefabs (same order as CatSelector)")]
    public GameObject[] catPrefabs;

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

        // Read saved selection
        selectedCatIndex = PlayerPrefs.GetInt("SelectedCatIndex", 0);
    }

    public GameObject GetSelectedCatPrefab()
    {
        if (catPrefabs == null || catPrefabs.Length == 0) return null;
        int index = Mathf.Clamp(selectedCatIndex, 0, catPrefabs.Length - 1);
        return catPrefabs[index];
    }
}
