﻿using UnityEngine;
using UnityEngine.Serialization;


public class InvaderProjectileController : MonoBehaviour
{
    public SpriteRenderer moveExtents;
    public Vector3 beatMove = new Vector3(0, 1, 0);
    private AudioSyncMove mover;

    public void Awake()
    {
        mover = GetComponent<AudioSyncMove>();
        mover.Beat += OnBeat;
    }

    public void OnBeat()
    {
        if (!moveExtents.bounds.Contains(transform.position))
        {
            Destroy(gameObject);
        }

        var target = transform.position + beatMove;
        mover.MoveToTarget(target);
    }

}