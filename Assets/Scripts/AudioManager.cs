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

    public AudioPlayerSync GlobalAudioPlayer;

    private void Start()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        GlobalAudioPlayer = GetTrack();
    }

    public AudioPlayerSync GetTrack(AudioClip clip = null)
    {
 
        var clipName = clip != null ? clip.name : "Global";

        //Find if track exists
        var children = transform.GetComponentsInChildren<AudioPlayerSync>();
        var player = children.FirstOrDefault(x => x.name == clipName);
        if (player != null)
        {
            return player;
        }
        var go = new GameObject();
        go.transform.parent = this.transform;
        go.name = clipName;
        var track = go.AddComponent<AudioPlayerSync>();
        track.Clip = clip;
        track.enabled = true;
        if (clip != null)
        {
            track.LoopLength = (int)Mathf.Round((float)(clip.length / 60d * BPM));
        }

        return track;
    }
}

