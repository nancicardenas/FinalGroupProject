using UnityEngine;

/// <summary>
/// Drop into any scene. Tells AudioManager which music track to play.
/// If no clip is assigned, music stops for that scene.
/// </summary>
public class SceneMusic : MonoBehaviour
{
    [Header("Music for this scene")]
    public AudioClip musicTrack;

    void Start()
    {
        if (AudioManager.Instance != null)
        {
            if (musicTrack != null)
            {
                AudioManager.Instance.PlayMusic(musicTrack);
            }
            else
            {
                AudioManager.Instance.StopMusic();
            }
        }
    }
}