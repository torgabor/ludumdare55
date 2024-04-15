using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [DoNotSerialize] public static GameController Instance;

    public static int GameStartBeat = int.MaxValue;
    public static Action OnStartGame;

    public int GameStartDelayBeats = 4;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        GameStartBeat = AudioSyncer.CurrentBeat + GameStartDelayBeats;
        OnStartGame?.Invoke();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
