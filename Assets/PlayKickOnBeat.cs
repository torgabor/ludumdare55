using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayKickOnBeat : MonoBehaviour
{
    public int StepsPerBeat = 1;
    public AudioClip clip;

    private double nextEventTime;
    private AudioSource[] audioSources;
    private bool running = false;
    private int currentSource = 0;

    void Start()
    {
        audioSources = new AudioSource[StepsPerBeat];
        for (int i = 0; i < StepsPerBeat; i++)
        {
            audioSources[i] = gameObject.AddComponent<AudioSource>();
            audioSources[i].clip = clip;
        }
        nextEventTime = AudioSettings.dspTime + 2.0f;
        running = true;
    }

    void Update()
    {
        if (!running)
        {
            return;
        }

        double time = AudioSettings.dspTime;

        if (time + 0.1f > nextEventTime)
        {
            audioSources[currentSource].PlayScheduled(nextEventTime);
            nextEventTime += 60.0f / AudioManager.Instance.BeatsPerMinute / StepsPerBeat;
            currentSource = (currentSource + 1) % StepsPerBeat;
        }
    }
}
