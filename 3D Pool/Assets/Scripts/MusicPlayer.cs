using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MusicPlayer : MonoBehaviour
{
    public static MusicPlayer Instance;

    [SerializeField] List<AudioClip> playlist;
    [SerializeField] AudioSource audioSource;
    private int currentTrackIndex = 0;

    void Awake()
    {
        // Singleton check
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        if (playlist.Count > 0 && audioSource != null && !audioSource.isPlaying)
        {
            PlayTrack(currentTrackIndex);
        }
    }

    void Update()
    {
        if (!audioSource.isPlaying && playlist.Count > 0)
        {
            PlayNextTrack();
        }
    }

    void PlayTrack(int index)
    {
        if (index >= 0 && index < playlist.Count)
        {
            audioSource.clip = playlist[index];
            audioSource.Play();
        }
    }

    void PlayNextTrack()
    {
        currentTrackIndex = (currentTrackIndex + 1) % playlist.Count;
        PlayTrack(currentTrackIndex);
    }
}
