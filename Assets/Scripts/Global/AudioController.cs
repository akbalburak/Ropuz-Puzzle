using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class AudioController : MonoBehaviour
{
    public static AudioController Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    [Header("Where all the musics will played.")]
    public AudioSource MusicPlayer;

    [Header("Where all the sounds will played.")]
    public AudioSource SoundPlayer;

    [Header("Click audio will play when click a button.")]
    public AudioClip SNDClick;

    private void Start()
    {
        // We are checking the state of audio settings.
        RefreshState();

        // We are playing the clip.
        MusicPlayer.Play();
    }

    private void Update()
    {
        // When any button click any object. We will play audio in audio player.
        if (Input.GetMouseButtonDown(0) && EventSystem.current.currentSelectedGameObject != null)
            SoundPlayer.PlayOneShot(SNDClick);
    }

    internal void RefreshState()
    {
        // We are setting the volume depends on the musics state.
        MusicPlayer.volume = SaveLoadController.Instance.SaveData.IsMusicActive ? 1 : 0;

        // We are setting the volume depends on the sound state.
        SoundPlayer.volume = SaveLoadController.Instance.SaveData.IsSoundActive? 1 : 0;
    }
}
