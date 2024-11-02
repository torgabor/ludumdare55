using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ShieldMiniGame : AudioSyncer
{
    public LightningRenderer lightningRenderer;
    public bool isActive;
    [Range(0, 1)] public float shieldChance;

    public float startOffset;
    public float endOffset;

    public int ShieldCount = 12;
    public GameObject ShieldArcPrefab;
    public GameObject BulletPrefab;

    private List<GameObject> arcs = new();
    private List<GameObject> bullets = new();
    private int activeArcs = 0;


    private void Start()
    {
        PartialBeats = 2;
        for (int i = 0; i < ShieldCount; i++)
        {
            // place arcs in a circle
            var arc = Instantiate(ShieldArcPrefab, transform);
            arc.transform.rotation = Quaternion.Euler(0f, 0f, 360f / ShieldCount * i);
            arcs.Add(arc);
        }
    }

    public void ActivateArc()
    {
        activeArcs++;
        if (activeArcs == ShieldCount)
        {
            isActive = true;
        }
    }

    public void DeactivateArc()
    {
        activeArcs--;
    }

    MonsterController MobWithoutShield()
    {
        foreach (var monster in SpawnController.Instance.Monsters)
        {
            if (!monster.HasShield)
                return monster;
        }

        return null;
    }

    public override void OnBeat()
    {
        // spawn bullets
        var bullet = bullets.FirstOrDefault(b => b.activeSelf);
        if (bullet == null)
        {
            bullet = Instantiate(BulletPrefab, transform);
        }
        float angle = 360f / ShieldCount * Random.Range(0, ShieldCount);
        bullet.GetComponent<SMGBulletController>().Launch(angle);

        if (!isActive)
        {
            return;
        }

        var addShield = Random.value < shieldChance;
        var noShieldMonster = MobWithoutShield();

        if (addShield && noShieldMonster != null)
        {
            noShieldMonster.AddShield();
            var startPos = lightningRenderer.transform.position;
            var endPos = noShieldMonster.transform.position;
            var dir = (endPos - startPos).normalized;
            var startPosOffs = startPos + dir * startOffset;
            var endPosOffs = endPos - dir * endOffset;
            lightningRenderer.Shoot(startPosOffs, endPosOffs);
        }

        base.OnBeat();
    }
}