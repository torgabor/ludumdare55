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
    public int BPM = 120;

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
        audioListener = this.GetComponent<AudioListener>();
        source = GetComponent<AudioSource>();
    }

    public void Play(AudioClip dieSound)
    {
        source.PlayOneShot(dieSound);
    }
}

