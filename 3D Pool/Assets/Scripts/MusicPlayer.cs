using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MusicPlayer : MonoBehaviour
{
    public static MusicPlayer Instance;

    [SerializeField] List<AudioClip> playlist;
    [SerializeField] AudioSource audioSource;

    [SerializeField] bool shuffleEnabled = true; // Toggle this in Inspector

    private int currentTrackIndex = 0;
    private List<int> shuffledIndices = new List<int>();

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
        if (playlist.Count == 0 || audioSource == null) return;

        // Prepare shuffled list if needed
        if (shuffleEnabled)
        {
            shuffledIndices = Enumerable.Range(0, playlist.Count).OrderBy(i => Random.value).ToList();
            currentTrackIndex = 0;
            PlayTrack(shuffledIndices[currentTrackIndex]);
        }
        else
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
        currentTrackIndex++;

        if (shuffleEnabled)
        {
            if (currentTrackIndex >= shuffledIndices.Count)
            {
                // Reshuffle for next loop
                shuffledIndices = Enumerable.Range(0, playlist.Count).OrderBy(i => Random.value).ToList();
                currentTrackIndex = 0;
            }

            PlayTrack(shuffledIndices[currentTrackIndex]);
        }
        else
        {
            currentTrackIndex = currentTrackIndex % playlist.Count;
            PlayTrack(currentTrackIndex);
        }
    }
}
