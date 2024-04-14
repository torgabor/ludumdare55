using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(AudioListener))]
[RequireComponent(typeof(AudioSource))]

public class AudioManager : MonoBehaviour
{
    [DoNotSerialize] public static AudioManager Instance;

    public AudioListener audioListener;
    public AudioSource source;
    public float BeatsPerMinute = 120f;

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
        audioListener = this.GetComponent<AudioListener>();
        source = GetComponent<AudioSource>();
    }

    public void Play(AudioClip dieSound)
    {
        source.PlayOneShot(dieSound);
    }
}

