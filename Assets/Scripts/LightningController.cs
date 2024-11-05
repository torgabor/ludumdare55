using UnityEngine;

public class LightningController : MonoBehaviour
{
    public LightningRenderer lightningRenderer1;
    public LightningRenderer lightningRenderer2;
    public float startOffset;
    public float endOffset;

    private bool alternate = false;

    public void ShootLightning(Vector3 startPos, Vector3 endPos)
    {
        var renderer = alternate ? lightningRenderer1 : lightningRenderer2;
        alternate = !alternate;
        
        var dir = (endPos - startPos).normalized;
        var startPosOffs = startPos + dir * startOffset;
        var endPosOffs = endPos - dir * endOffset;
        renderer.Shoot(startPosOffs, endPosOffs);
    }
} 