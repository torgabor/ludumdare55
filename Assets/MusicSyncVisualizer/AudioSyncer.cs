using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Parent class responsible for extracting beats from..
/// ..spectrum value given by AudioSpectrum.cs
/// </summary>
public class AudioSyncer : MonoBehaviour
{
    public static int CurrentBeat => (int)Mathf.Floor((float)(AudioSettings.dspTime / 60d * AudioManager.Instance.BPM));
    public static int ClosestBeat => TimeToBeat(AudioSettings.dspTime);
    public static int TimeToBeat(double seconds) => (int)Mathf.Round((float)(seconds / 60d * AudioManager.Instance.BPM));
    public static double BeatInterval => 60d / AudioManager.Instance.BPM;
    public static double DspTime => AudioSettings.dspTime;

    public Action Beat;

    private int lastBeat = 0;

    public virtual void OnStart()
    {
        lastBeat = CurrentBeat;
    }

    public virtual void OnAwake()
    {

    }


    private void Awake()
    {
        OnAwake();
    }

    private void Start()
    {
        OnStart();
    }

    /// <summary>
    /// Inherit this to cause some behavior on each beat
    /// </summary>
    public virtual void OnBeat()
    {
        Beat?.Invoke();
    }

    /// <summary>
    /// Inherit this to do whatever you want in Unity's update function
    /// Typically, this is used to arrive at some rest state..
    /// ..defined by the child class
    /// </summary>
    public virtual void OnUpdate()
    {
        if (CurrentBeat >= GameController.GameStartBeat && CurrentBeat != lastBeat)
        {
            OnBeat();
            lastBeat = CurrentBeat;
        }

        //// update audio value
        //m_previousAudioValue = m_audioValue;
        //m_audioValue = AudioSpectrum.spectrumValue;

        //// if audio value went below the bias during this frame
        //if (m_previousAudioValue > m_settings.bias &&
        //	m_audioValue <= m_settings.bias)
        //{
        //	// if minimum beat interval is reached
        //	if (m_timer > m_settings.timeStep)
        //		OnBeat();
        //}

        //// if audio value went above the bias during this frame
        //if (m_previousAudioValue <= m_settings.bias &&
        //	m_audioValue > m_settings.bias)
        //{
        //	// if minimum beat interval is reached
        //	if (m_timer > m_settings.timeStep)
        //		OnBeat();
        //}

        //m_timer += Time.deltaTime;
    }

    private void Update()
    {
        OnUpdate();
    }




    
}
