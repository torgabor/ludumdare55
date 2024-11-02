using UnityEngine;

public class ShieldMiniGame : AudioSyncer
{
    public LightningRenderer lightningRenderer;
    public bool isActive;
    [Range(0, 1)] public float shieldChance;

    public float startOffset;
    public float endOffset;

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
        var addShield = Random.value < shieldChance;
        var noShieldMonster = MobWithoutShield();
        if (isActive && addShield && noShieldMonster != null)
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