using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AudioPlayerSync : AudioSyncer
{
    public AudioClip Clip;
    public bool IsPlaying => Sources.Any(s => s.isPlaying);
    public int LoopLength = 1;
    public List<AudioSource> Sources = new();
    public float Volume = 1.0f;

    private bool isLooping = false;
    private int loopStart = 0;
    private int loopEnd = int.MaxValue;
    private float[] m_audioSpectrum;
    public float m_spectrum;

    public void Awake()
    {
        m_audioSpectrum = new float[128];
    }

    public void Stop()
    {
        Sources.ForEach(s => { s.Stop(); });
        isLooping = false;
    }

    public void Stop(int stopBeat)
    {
        if (stopBeat > CurrentBeat)
        {
            Sources.ForEach(s =>
            {
                s.SetScheduledEndTime(stopBeat * BeatInterval);
            });
        }
        isLooping = false;
    }

    public void Play(AudioClip clip)
    {
        Clip = clip;
        Play();
    }

    public void Play()
    {
        var audioSource = GetAudioSource();
        audioSource.clip = Clip;
        audioSource.volume = Volume;
        audioSource.Play();
        isLooping = false;
    }

    public void Play(AudioClip clip, int startBeat = 0, int stopBeat = 0)
    {
        Clip = clip;
        Play(startBeat, stopBeat);
    }

    public void Play(int startBeat = 0, int stopBeat = 0)
    {
        var audioSource = GetAudioSource();
        audioSource.clip = Clip;
        audioSource.volume = Volume;
        audioSource.PlayScheduled(startBeat * BeatInterval);
        if (stopBeat > CurrentBeat)
        {
            audioSource.SetScheduledEndTime(stopBeat * BeatInterval);
            loopEnd = stopBeat;
        }
        loopStart = startBeat + LoopLength;
        isLooping = false;
    }

    public void Loop(int startBeat = 0, int stopBeat = 0)
    {
        Play(startBeat, stopBeat);
        isLooping = true;
    }

    public void Loop(AudioClip clip, int startBeat = 0, int stopBeat = 0)
    {
        Play(clip, startBeat, stopBeat);
        isLooping = true;
    }

    public void StopOnLoopEnd()
    {
        isLooping = false;
    }

    private AudioSource GetAudioSource()
    {
        AudioSource source = Sources.FirstOrDefault(s => !s.isPlaying);
        if (source == null)
        {
            source = gameObject.AddComponent<AudioSource>();
            source.clip = Clip;
            Sources.Add(source);
        }
        return source;
    }

    public float GetSpectrumData()
    {
        var source = Sources.FirstOrDefault(s => s.isPlaying);
        if (source != null)
        {
            source.GetSpectrumData(m_audioSpectrum, 0, FFTWindow.Hamming);
            m_spectrum = m_audioSpectrum[0] * 100;
        }
        else
        {
            m_spectrum = 0;
        }

        return m_spectrum;
    }

    public void Update()
    {
        if (!IsPlaying || !isLooping) { return; }

        // trigger kick on each beat
        double time = AudioSettings.dspTime;
        double nextTime = loopStart * BeatInterval;
        double stopTime = loopEnd * BeatInterval;
        double lookAhead = Mathf.Clamp((float)(LoopLength * BeatInterval / 2), 0.15f, 2f);
        if (nextTime < stopTime && time + lookAhead > nextTime)
        {
            Loop(loopStart);
        }
    }
}