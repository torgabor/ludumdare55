using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class PlayKickOnBeat : AudioSyncer
{
    public int StepsPerBeat = 1;
    public AudioClip Clip;
    public double HitTime = 0;
    public float Threshold = 0.2f;

    private double nextEventTime;
    private List<AudioSource> audioSources;
    private bool running = false;
    private int currentSource = 0;

    public override void OnStart()
    {
        audioSources = new List<AudioSource>();
        nextEventTime = AudioSettings.dspTime + 2.0f;
        running = true;
    }

    private AudioSource GetAudioSource()
    {
        AudioSource source = audioSources.FirstOrDefault(s => !s.isPlaying);
        if (source == null)
        {
            source = gameObject.AddComponent<AudioSource>();
            source.clip = Clip;
            audioSources.Add(source);
        }
        return source;
    }

    public override void OnBeat()
    {
        Debug.Log($"OnBeat: {CurrentBeat}");
    }

    public void Play(AudioClip audioClip)
    {
        var audioSource = GetAudioSource();
        audioSource.clip = audioClip;
        audioSource.Play();
    }

    void Update()
    {
        if (!running)
        {
            return;
        }

        double time = AudioSettings.dspTime;
        double scheduleTime = 60.0f / AudioManager.Instance.BPM / StepsPerBeat;
        if (time + scheduleTime > nextEventTime)
        {
            currentSource = (currentSource + 1) % StepsPerBeat;
            GetAudioSource().PlayScheduled(nextEventTime);
            nextEventTime += scheduleTime;
        }
    }
}
