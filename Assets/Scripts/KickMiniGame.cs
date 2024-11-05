using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class KickMiniGame : AudioSyncer, IMiniGame
{
    public AudioClip Kick;
    public AudioClip KickBad;
    public AudioClip InitLoop;
    public AudioClip Startup;
    public AudioClip BassLoop;
    public AudioClip BassLoop2;

    public double Threshold = 0.2;
    public bool IsRunning = false;
    public SpriteRenderer ProgressIndicator;
    public SpriteRenderer BeatSpriteRenderer;
    public SpriteRenderer Indicator;
    public Color indicatorOrigColor1;
    public Color indicatorOrigColor2;
    public int LevelBeats = 64;
    public int Level = 0;

    public float OpacitySpeed = 1;
    public float Progress;
    private int startBeat;
    private bool isEnabled = false;
    private float displayProgress;

    private double countDownStartTime;
    private float beatOpacity;
    private int prevHitCount;
    private int hitCount;

    private AudioPlayerSync OneShots;
    private AudioPlayerSync KickTrack;
    private AudioPlayerSync InitLoopTrack;
    private AudioPlayerSync BassLoopTrack;
    private AudioPlayerSync Bass2LoopTrack;
    private MaterialPropertyBlock indicatorBlock;
    private MaterialPropertyBlock progressBlock;

    private InputActions inputActions;

    public SphereController sphereController;

    void Start()
    {
        inputActions = new InputActions();
        inputActions.Enable();
        progressBlock = new MaterialPropertyBlock();
        indicatorBlock = new MaterialPropertyBlock();
        ProgressIndicator.GetPropertyBlock(progressBlock);
        Indicator.GetPropertyBlock(indicatorBlock);
        KickTrack = AudioManager.Instance.GetTrack(Kick);
        KickTrack.LoopLength = 1;
        sphereController.soundEffectController.player = KickTrack;

        OneShots = AudioManager.Instance.GetTrack(KickBad);
        InitLoopTrack = AudioManager.Instance.GetTrack(InitLoop);
        BassLoopTrack = AudioManager.Instance.GetTrack(BassLoop);
        Bass2LoopTrack = AudioManager.Instance.GetTrack(BassLoop2);

        if (GameController.Instance.Sandbox)
        {
            StartMiniGame();
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
            Progress = Mathf.Min(1f, Progress + 0.25f);
        }

        if (!IsRunning && hitCount == 4)
        {
            StartMiniGame();
        }
    }

    public void StartMiniGame()
    {
        // start loop
        IsRunning = true;
        OneShots.Play(Startup);
        InitLoopTrack.Stop(startBeat);
        BassLoopTrack.Stop(startBeat);
        Bass2LoopTrack.Stop(startBeat);
        startBeat = ClosestBeat + 2;
        countDownStartTime = startBeat * BeatInterval;
        KickTrack.Loop(startBeat);
        BassLoopTrack.Loop(startBeat, startBeat + LevelBeats);
        Bass2LoopTrack.Loop(startBeat + LevelBeats);
        InitLoopTrack.Loop(startBeat);
        GameController.Instance.StartMainGameLoop(startBeat);
    }

    private void Miss()
    {
        OneShots.Play(KickBad);
        indicatorBlock.SetColor("_Color1", Color.red);
        indicatorBlock.SetColor("_Color2", Color.red);
        Indicator.SetPropertyBlock(indicatorBlock);
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

        indicatorBlock.SetColor("_Color1", indicatorOrigColor1);
        indicatorBlock.SetColor("_Color2", indicatorOrigColor2);
        Indicator.SetPropertyBlock(indicatorBlock);

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
        if (GameController.Instance.Sandbox)
        {
            Progress = 1;
        }
        if (isEnabled && inputActions.Player.Kick.WasPerformedThisFrame())
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

        if (IsRunning && countDownStartTime - DspTime < 0)
        {
            Progress -= Time.deltaTime / ((float)BeatInterval * 16);
        }

        if (IsRunning && Progress <= 0)
        {
            StopMiniGame();
        }

        displayProgress = Mathf.MoveTowards(displayProgress, Progress, 2f * Time.deltaTime);
        progressBlock.SetFloat("_Progress", displayProgress);
        ProgressIndicator.SetPropertyBlock(progressBlock);


        // dim center animation
        var c = BeatSpriteRenderer.color;
        c.a = beatOpacity;
        BeatSpriteRenderer.color = c;
        beatOpacity = Mathf.Max(0, beatOpacity - Time.deltaTime * beatOpacity * OpacitySpeed);

        // trigger kick on each beat
        double time = AudioSettings.dspTime;
        double nextTime = startBeat * BeatInterval;
        if (time + BeatInterval / 2 > nextTime) // load 0.2 seconds before next beat
        {
            if (!IsRunning && hitCount != 0 && hitCount == prevHitCount)
            {
                Miss();
            }

            startBeat += 1;
            prevHitCount = hitCount;
        }
    }

    public void StopMiniGame()
    {
        IsRunning = false;
        KickTrack.Stop();
        BassLoopTrack.Stop(GetNextClosestBar(8));
        Bass2LoopTrack.Stop(GetNextClosestBar(8));
        GameController.Instance.StopMainGameLoop();
    }

    public void Enable()
    {
        isEnabled = true;
        startBeat = GameController.GameStartBeat;
        InitLoopTrack.Loop(startBeat);
    }

    public void Disable()
    {
        isEnabled = false;
        StopMiniGame();
    }
}