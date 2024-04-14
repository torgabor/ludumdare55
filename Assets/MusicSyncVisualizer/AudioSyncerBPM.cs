using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


/// <summary>
/// Parent class responsible for extracting beats from..
/// ..spectrum value given by AudioSpectrum.cs
/// </summary>
public class AudioSyncerBPM : MonoBehaviour
{
    private int lastBeat = 0;
    public AudioClip Clip;
    public float BeatsPerMinute = 120f;
    public int CurrentBeat
    {
        get
        {
            float beatsPerSecond = BeatsPerMinute / 60;
            return (int)Mathf.Floor(beatsPerSecond * Time.realtimeSinceStartup);
        }
    }

    public void Start()
    {
        lastBeat = CurrentBeat;
    }

    public virtual void OnBeat()
    {
        AudioManager.Instance.Play(Clip);
    }

    public virtual void FixedUpdate()
    {
        if (lastBeat != CurrentBeat)
        {
            lastBeat = CurrentBeat;
            OnBeat();
        }
    }

    private void Update()
    {

    }
}
