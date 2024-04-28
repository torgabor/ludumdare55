using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnController : AudioSyncer
{
    public KickMiniGame KickMiniGame;
    public MonsterController InvaderPrefab;
    public SpriteRenderer MoveExtents;
    public int Delay = 2;
    public List<MonsterController> Monsters ;

    public static SpawnController Instance;
    
    public void Awake()
    {
        Monsters = new List<MonsterController>();
        Instance = this;
    }
    
    private int current = 0;

    public void OnDie(MonsterController monster)
    {
        Monsters.Remove(monster);
    }
    public override void OnBeat()
    {
        base.OnBeat();
        if (current >= Delay)
        {
            var invader = Instantiate(InvaderPrefab, gameObject.transform.position, Quaternion.identity);
            invader.moveExtents = MoveExtents;
            invader.spawner = this;
            Monsters.Add(invader);
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
