using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnController : AudioSyncer
{
    public KickMiniGame KickMiniGame;
    public MonsterController InvaderPrefab;
    public SpriteRenderer MoveExtents;

    public override void OnBeat()
    {
        base.OnBeat();
        if (KickMiniGame.IsRunning)
        {
            var invader = Instantiate(InvaderPrefab, gameObject.transform.position, Quaternion.identity);
            invader.moveExtents = MoveExtents;
        }
    }
}
