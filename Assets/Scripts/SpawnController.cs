using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnController : AudioSyncer
{
    public KickMiniGame KickMiniGame;
    public MonsterController InvaderPrefab;
    public SpriteRenderer MoveExtents;
    public int Delay = 2;

    private int current = 0;

    public override void OnBeat()
    {
        base.OnBeat();
        if (current >= Delay)
        {
            var invader = Instantiate(InvaderPrefab, gameObject.transform.position, Quaternion.identity);
            invader.moveExtents = MoveExtents;
        }

        if (KickMiniGame.IsRunning)
        {
            current++;
        }
        else
        {
            current = 0;
        }
    }
}
