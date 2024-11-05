using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUp : MonoBehaviour
{
    public GameObject MiniGame;
    public AudioClip PowerUpSound;

    private AudioPlayerSync audioTrack;

    // Start is called before the first frame update
    void Start()
    {
        audioTrack = AudioManager.Instance.GetTrack(PowerUpSound);
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Mob")
        {
            MiniGame.SetActive(true);
            gameObject.SetActive(false);
            audioTrack.Play();
        }
    }
}
