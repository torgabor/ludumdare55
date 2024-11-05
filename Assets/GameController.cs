using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class GameController : AudioSyncer
{
    [DoNotSerialize] public static GameController Instance;

    public static int GameStartBeat = int.MaxValue;
    public int GameStartDelayBeats = 4;
    public int GameMainLoopStartBeat = int.MaxValue;
    public HealthController EnemyHealthController;
    public HealthController PlayerHealthController;
    public KickMiniGame KickMiniGame;
    public GameObject Menu;
    public GameObject GameOverText;
    public GameObject Instructions;
    public AudioClip Noice;
    public AudioClip Whatt;
    public ShieldMiniGame ShieldMiniGame;
    public int TimerSeconds = 180;

    public bool GameOver = false;
    public bool isBaseGameRunning = false;
    public bool Sandbox = false;

    private float enemyHP = 1f;
    private float playerHP = 1f;
    private InputActions inputActions;
    private bool isStarted = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    public override void OnBeat()
    {
        base.OnBeat();
        if (Sandbox) return;
        playerHP -= 1f / TimerSeconds;
        PlayerHealthController.SetHealth(playerHP);
        if (playerHP < 0f)
        {
            Lose();
        }
    }

    public override void OnStart()
    {
        base.OnStart();
        inputActions = new InputActions();
        inputActions.Enable();
        if (Sandbox)
        {
            StartGame();
        }
    }

    public void Damage()
    {
        enemyHP = Mathf.Clamp(enemyHP - 0.2f, 0, 1);
        EnemyHealthController.SetHealth(enemyHP);
        if (enemyHP == 0)
        {
            Win();
        }
    }

    void Lose()
    {
        if (GameOver) { return; }
        GameOverText.SetActive(true);
        var text = GameOverText.GetComponent<TMP_Text>();
        text.text = "Game Over";
        Menu.SetActive(true);
        GameOver = true;
        GameStartBeat = int.MaxValue;
        var track = AudioManager.Instance.GetTrack(Whatt);
        track.Volume = 0.8f;
        track.Play();
    }

    void Win()
    {
        if (GameOver) { return; }
        GameOverText.SetActive(true);
        var text = GameOverText.GetComponent<TMP_Text>();
        text.text = "Noice!";
        Menu.SetActive(true);
        GameOver = true;
        GameStartBeat = int.MaxValue;
        var track = AudioManager.Instance.GetTrack(Noice);
        track.Play();
    }

    public void StartGame()
    {
        if (isStarted)
        {
            return;
        }
        Menu.SetActive(false);
        GameStartBeat = CurrentBeat + GameStartDelayBeats;
        Instructions.SetActive(false);
        KickMiniGame.Enable();
        isStarted = true;
    }

    public void StartMainGameLoop(int beat)
    {
        if (isBaseGameRunning) { return; }
        isBaseGameRunning = true;
        ShieldMiniGame.Enable();
        GameMainLoopStartBeat = beat;
    }

    public void StopMainGameLoop()
    {
        if (!isBaseGameRunning) { return; }
        isBaseGameRunning = false;
        ShieldMiniGame.Disable();
        GameMainLoopStartBeat = int.MaxValue;
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
        if (inputActions.Player.Start.WasPerformedThisFrame())
        {
            StartGame();
        }
    }
}
