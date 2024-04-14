using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class PlayKickOnBeat : AudioSyncer
{
    public AudioClip Kick;
    public AudioClip BaseMusicLoop;
    public double Threshold = 0.2;
    public bool IsRunning = false;
    public SpriteRenderer beatSpriteRenderer;
    public float OpacitySpeed = 1;

    private List<AudioSource> audioSources;
    private float beatOpacity = 0f;
    private int currentBeat = 0;
    private int currentBaseMusicStartBeat = 0;

    public override void OnStart()
    {
        base.OnStart();
        audioSources = new List<AudioSource>();
        currentBeat = CurrentBeat + 5;
        currentBaseMusicStartBeat = CurrentBeat + 5;
    }

    private AudioSource GetAudioSource()
    {
        AudioSource source = audioSources.FirstOrDefault(s => !s.isPlaying);
        if (source == null)
        {
            source = gameObject.AddComponent<AudioSource>();
            source.clip = Kick;
            audioSources.Add(source);
        }
        return source;
    }

    public void OnMouseDown()
    {
        double hitTime = AudioSettings.dspTime;
        double nextTime = (currentBeat - 1) * BeatInterval;
        if (Mathf.Abs((float)(nextTime - hitTime)) < Threshold)
        {
            Play(Kick);
            beatOpacity = 1f;
        }
    }

    public override void OnBeat()
    {
        base.OnBeat();

        // animate center
        if (IsRunning)
        {
            beatOpacity = Mathf.Min(beatOpacity + 0.5f, 1f);
        }
        else
        {
            beatOpacity = Mathf.Min(beatOpacity + 0.1f, 1f);
        }
    }

    public void Play(AudioClip audioClip)
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

    public override void OnUpdate()
    {
        base.OnUpdate();

        // dim center animation
        var c = beatSpriteRenderer.color;
        beatSpriteRenderer.color = new Color(c.r, c.g, c.b, beatOpacity);
        beatOpacity = Mathf.Max(0, beatOpacity - Time.deltaTime * beatOpacity * OpacitySpeed);

        // trigger kick on each beat
        double time = AudioSettings.dspTime;
        double nextTime = currentBeat * BeatInterval;
        if (time + 0.2d > nextTime) // load 0.2 seconds before next beat
        {
            if (IsRunning)
            {
                Play(Kick, nextTime);
            }
            currentBeat += 1;
            Debug.Log(CurrentBeat);
        }

        // trigger base music on every 32 beat
        time = AudioSettings.dspTime;
        nextTime = currentBaseMusicStartBeat * BeatInterval;
        if (time + 2d > nextTime) // load 2 seconds before next loop
        {
            Play(BaseMusicLoop, nextTime);
            currentBaseMusicStartBeat += 32;
            Debug.Log(CurrentBeat);
        }
    }
}
