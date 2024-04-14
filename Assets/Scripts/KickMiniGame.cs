using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class PlayKickOnBeat : AudioSyncer
{
    public AudioClip Kick;
    public AudioClip Startup;
    public AudioClip KickBad;
    public AudioClip BaseMusicLoop;
    public AudioClip BassLoop;
    public double Threshold = 0.2;
    public bool IsRunning = false;
    public SpriteRenderer BeatSpriteRenderer;
    public SpriteRenderer Indicator;
    public float OpacitySpeed = 1;
    public SpriteMask Mask;

    private List<AudioSource> audioSources;
    private float beatOpacity = 0f;
    private int nextBeat = 0;
    private int currentBaseMusicStartBeat = 0;
    private int currentBassLoopStartBeat = 0;
    private double countDownStartTime = AudioSettings.dspTime;

    public override void OnStart()
    {
        base.OnStart();
        audioSources = new List<AudioSource>();
        nextBeat = CurrentBeat + 5;
        currentBaseMusicStartBeat = nextBeat;
        currentBassLoopStartBeat = nextBeat;
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

    private int prevHitCount = 0;
    private int hitCount = 0;

    public void OnMouseDown()
    {
        OnClick();
    }

    void OnClick()
    {
        double hitTime = AudioSettings.dspTime;
        double nextTime = (nextBeat - 1) * BeatInterval;
        if (Mathf.Abs((float)(nextTime - hitTime)) < Threshold)
        {
            Hit();
        }
        else
        {
            Miss();
        }
    }

    private void Hit()
    {
        beatOpacity = 1f;
        if (!IsRunning)
        {
            Play(Kick);
            hitCount++;
            Mask.alphaCutoff = hitCount * 0.25f;
        }
        else
        {
            Mask.alphaCutoff += 0.25f;
        }

        if (!IsRunning && hitCount == 4)
        {
            audioSources.ForEach(s => { s.Stop(); });
            int closestBeat = ClosestBeat;
            nextBeat = closestBeat + 2;
            currentBaseMusicStartBeat = nextBeat;
            currentBassLoopStartBeat = nextBeat;
            Play(Startup, closestBeat * BeatInterval);
            IsRunning = true;
            countDownStartTime = nextBeat * BeatInterval;
        }
    }

    private void Miss()
    {
        Play(KickBad);
        Indicator.color = Color.red;
        if (!IsRunning)
        {
            hitCount = 0;
            Mask.alphaCutoff = hitCount * 0.25f;
        }
        else
        {
            Mask.alphaCutoff -= 0.25f;
        }
    }

    public override void OnBeat()
    {
        base.OnBeat();

        Indicator.color = Color.white;

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

        if (IsRunning && countDownStartTime - AudioSettings.dspTime < 0)
        {
            Mask.alphaCutoff -= Time.deltaTime / ((float)BeatInterval * 16);
        }

        if (Mask.alphaCutoff <= 0)
        {
            IsRunning = false;
        }

        // dim center animation
        var c = BeatSpriteRenderer.color;
        BeatSpriteRenderer.color = new Color(c.r, c.g, c.b, beatOpacity);
        beatOpacity = Mathf.Max(0, beatOpacity - Time.deltaTime * beatOpacity * OpacitySpeed);

        // trigger kick on each beat
        double time = AudioSettings.dspTime;
        double nextTime = nextBeat * BeatInterval;
        if (time + BeatInterval / 2 > nextTime) // load 0.2 seconds before next beat
        {
            if (IsRunning)
            {
                Play(Kick, nextTime);
            }
            else if (hitCount != 0 && hitCount == prevHitCount)
            {
                Miss();
            }
            nextBeat += 1;
            prevHitCount = hitCount;
        }

        // trigger base music
        nextTime = currentBaseMusicStartBeat * BeatInterval;
        if (time + 1d > nextTime)
        {
            Play(BaseMusicLoop, nextTime);
            currentBaseMusicStartBeat += 32;
        }

        // trigger base music
        nextTime = currentBassLoopStartBeat * BeatInterval;
        if (time + 1d > nextTime)
        {
            if (IsRunning)
            {
                Play(BassLoop, nextTime);
            }
            currentBassLoopStartBeat += 8;
        }
    }
}
