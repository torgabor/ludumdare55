using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(AudioListener))]

public class AudioManager : MonoBehaviour
{
    [DoNotSerialize] public static AudioManager Instance;

    public AudioListener audioListener;
    public int BPM = 120;

    public int CurrentBeat => (int)Mathf.Floor((float)(AudioSettings.dspTime / 60d * BPM));
    public int ClosestBeat => (int)Mathf.Round((float)(AudioSettings.dspTime / 60d * BPM));
    public double BeatInterval => 60d / BPM;

    private List<AudioSource> audioSources;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        audioSources = new List<AudioSource>();
    }

    void Start()
    {

    }

    public void StopAllAudio()
    {
        audioSources.ForEach(s => { s.Stop(); });
    }

    private AudioSource GetAudioSource()
    {
        AudioSource source = audioSources.FirstOrDefault(s => !s.isPlaying);
        if (source == null)
        {
            source = gameObject.AddComponent<AudioSource>();
            audioSources.Add(source);
        }
        return source;
    }

    public virtual void Play(AudioClip audioClip)
    {
        var audioSource = GetAudioSource();
        audioSource.clip = audioClip;
        audioSource.Play();
    }

    public void Play(AudioClip audioClip, double time)
    {
        var audioSource = GetAudioSource();
        audioSource.clip = audioClip;
        audioSource.PlayScheduled(time);
    }
}

