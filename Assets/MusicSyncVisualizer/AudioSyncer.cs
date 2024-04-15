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

    public Action Beat;

    private int lastBeat = 0;

    public virtual void OnStart()
    {
        lastBeat = AudioManager.Instance.CurrentBeat;
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
        m_timer = 0;
        m_isBeat = true;
    }

    /// <summary>
    /// Inherit this to do whatever you want in Unity's update function
    /// Typically, this is used to arrive at some rest state..
    /// ..defined by the child class
    /// </summary>
    public virtual void OnUpdate()
    {
        if (AudioManager.Instance.CurrentBeat != lastBeat)
        {
            OnBeat();
            lastBeat = AudioManager.Instance.CurrentBeat;
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

    public AudioSyncSettings m_settings;

    private float m_previousAudioValue;
    private float m_audioValue;
    private float m_timer;

    protected bool m_isBeat;
}
