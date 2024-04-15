using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class KickMiniGame : AudioSyncer
{
    public AudioClip Kick;
    public AudioClip KickBad;
    public AudioClip Startup;
    public AudioClip BassLoop;

    public double Threshold = 0.2;
    public bool IsRunning = false;
    public SpriteRenderer BeatSpriteRenderer;
    public SpriteRenderer Indicator;
    public float OpacitySpeed = 1;
    public SpriteMask Mask;
    public float Progress;

    private float beatOpacity;
    private int nextBeat;
    private double countDownStartTime;
    private int prevHitCount;
    private int hitCount;

    private AudioPlayerSync KickTrack;
    private AudioPlayerSync KickBadTrack;
    private AudioPlayerSync StartupTrack;
    private AudioPlayerSync BassLoopTrack;

    public override void OnAwake()
    {
        base.OnAwake();
        GameController.OnStartGame += OnGameStart;
    }

    public void OnGameStart()
    {
        KickTrack = AudioManager.Instance.GetTrack(Kick);
        KickTrack.LoopLength = 1;

        KickBadTrack = AudioManager.Instance.GetTrack(KickBad);
        StartupTrack = AudioManager.Instance.GetTrack(Startup);
        BassLoopTrack = AudioManager.Instance.GetTrack(BassLoop);

        nextBeat = GameController.GameStartBeat;
        StartupTrack.Loop(nextBeat);
    }

    public void OnMouseDown()
    {
        double hitTime = DspTime;
        double beatTime = ClosestBeat * BeatInterval;
        if (Mathf.Abs((float)(beatTime - hitTime)) < Threshold)
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
            KickTrack.Play();
            hitCount++;
            Progress = hitCount * 0.25f;
        }
        else
        {
            Progress += 0.25f;
        }

        if (!IsRunning && hitCount == 4)
        {
            IsRunning = true;
            nextBeat = ClosestBeat + 1;
            countDownStartTime = nextBeat * BeatInterval;
            KickTrack.Loop(nextBeat);
            BassLoopTrack.Loop(nextBeat);
            StartupTrack.Stop(nextBeat);
            StartupTrack.Loop(nextBeat);
        }
    }

    private void Miss()
    {
        KickBadTrack.Play();
        Indicator.color = Color.red;
        if (!IsRunning)
        {
            hitCount = 0;
            Progress = hitCount * 0.25f;
        }
        else
        {
            Progress -= 0.25f;
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

    public override void OnUpdate()
    {
        base.OnUpdate();


        if (IsRunning && countDownStartTime - DspTime < 0)
        {
            Progress -= Time.deltaTime / ((float)BeatInterval * 16);
        }

        if (IsRunning && Progress <= 0)
        {
            IsRunning = false;
            KickTrack.Stop();
            BassLoopTrack.StopOnLoopEnd();
        }

        Mask.alphaCutoff = Progress;

        // dim center animation
        var c = BeatSpriteRenderer.color;
        BeatSpriteRenderer.color = new Color(c.r, c.g, c.b, beatOpacity);
        beatOpacity = Mathf.Max(0, beatOpacity - Time.deltaTime * beatOpacity * OpacitySpeed);

        // trigger kick on each beat
        double time = AudioSettings.dspTime;
        double nextTime = nextBeat * BeatInterval;
        if (time + BeatInterval / 2 > nextTime) // load 0.2 seconds before next beat
        {
            if (!IsRunning && hitCount != 0 && hitCount == prevHitCount)
            {
                Miss();
            }
            nextBeat += 1;
            prevHitCount = hitCount;
        }
    }
}
