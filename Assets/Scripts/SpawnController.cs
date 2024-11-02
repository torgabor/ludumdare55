using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnController : AudioSyncer
{
    public KickMiniGame KickMiniGame;
    public MonsterController InvaderPrefab;
    public SpriteRenderer MoveExtents;
    public int Delay = 2;
    public List<MonsterController> Monsters;

    public static SpawnController Instance;
    public Transform spawnParent;

    public void Start()
    {
        Monsters = new List<MonsterController>();
        Instance = this;
        Monsters.AddRange(spawnParent.GetComponentsInChildren<MonsterController>());
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
            var invader = Instantiate(InvaderPrefab, gameObject.transform.position, Quaternion.identity, spawnParent);

            invader.moveExtents = MoveExtents;
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