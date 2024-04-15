using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(AudioListener))]

public class AudioManager : MonoBehaviour
{
    [DoNotSerialize] public static AudioManager Instance;

    public int BPM = 120;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    public AudioPlayerSync GetTrack(AudioClip clip)
    {
        var track = gameObject.AddComponent<AudioPlayerSync>();
        track.Clip = clip;
        track.enabled = true;
        track.LoopLength = (int)Mathf.Round((float)(clip.length / 60d * BPM));

        return track;
    }
}

