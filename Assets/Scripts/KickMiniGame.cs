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

    private float beatOpacity = 0f;
    private int nextBeat = 0;
    private int currentBaseMusicStartBeat = 0;
    private int currentBassLoopStartBeat = 0;
    private double countDownStartTime = AudioSettings.dspTime;
    private int prevHitCount = 0;
    private int hitCount = 0;

    public override void OnStart()
    {
        base.OnStart();
        nextBeat = AudioManager.Instance.CurrentBeat + 6;
        currentBaseMusicStartBeat = nextBeat;
        currentBassLoopStartBeat = nextBeat;
    }

    public void OnMouseDown()
    {
        OnClick();
    }

    void OnClick()
    {
        double hitTime = AudioSettings.dspTime;
        double beatTime = (AudioManager.Instance.ClosestBeat) * AudioManager.Instance.BeatInterval;
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
            AudioManager.Instance.Play(Kick);
            hitCount++;
            Mask.alphaCutoff = hitCount * 0.25f;
        }
        else
        {
            Mask.alphaCutoff += 0.25f;
        }

        if (!IsRunning && hitCount == 4)
        {
            AudioManager.Instance.StopAllAudio();
            int closestBeat = AudioManager.Instance.ClosestBeat;
            nextBeat = closestBeat + 2;
            currentBaseMusicStartBeat = nextBeat;
            currentBassLoopStartBeat = nextBeat;
            AudioManager.Instance.Play(Startup, closestBeat * AudioManager.Instance.BeatInterval);
            IsRunning = true;
            countDownStartTime = nextBeat * AudioManager.Instance.BeatInterval;
        }
    }

    private void Miss()
    {
        AudioManager.Instance.Play(KickBad);
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

    public override void OnUpdate()
    {
        base.OnUpdate();

        if (IsRunning && countDownStartTime - AudioSettings.dspTime < 0)
        {
            Mask.alphaCutoff -= Time.deltaTime / ((float)AudioManager.Instance.BeatInterval * 16);
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
        double nextTime = nextBeat * AudioManager.Instance.BeatInterval;
        if (time + AudioManager.Instance.BeatInterval / 2 > nextTime) // load 0.2 seconds before next beat
        {
            if (IsRunning)
            {
                AudioManager.Instance.Play(Kick, nextTime);
            }
            else if (hitCount != 0 && hitCount == prevHitCount)
            {
                Miss();
            }
            nextBeat += 1;
            prevHitCount = hitCount;
        }

        // trigger base music
        nextTime = currentBaseMusicStartBeat * AudioManager.Instance.BeatInterval;
        if (time + 1d > nextTime)
        {
            AudioManager.Instance.Play(BaseMusicLoop, nextTime);
            currentBaseMusicStartBeat += 32;
        }

        // trigger base music
        nextTime = currentBassLoopStartBeat * AudioManager.Instance.BeatInterval;
        if (time + 1d > nextTime)
        {
            if (IsRunning)
            {
                AudioManager.Instance.Play(BassLoop, nextTime);
            }
            currentBassLoopStartBeat += 8;
        }
    }
}
