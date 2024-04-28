using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

[RequireComponent(typeof(LineRenderer))]
public class LightningRenderer : MonoBehaviour
{
    private LineRenderer lineRenderer;
    public float segmentsPerLength;
    public float noiseStrength = 10;
    public float noiseFrequency = 5;
    public int octaves = 4;
    private MaterialPropertyBlock _colorBlock;
    public float fadeTime = 1.0f;
    public AnimationCurve fadeCurve;


    // public void OnValidate()
    // {
    //     lineRenderer = GetComponent<LineRenderer>();
    //     Shoot(targetPos);
    // }

    
    public void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        _colorBlock = new MaterialPropertyBlock();
    }

    public void Shoot(Vector3 startPos, Vector3 targetPos)
    {
        var dir = targetPos - startPos;

        var len = dir.magnitude;
        int posCount = Mathf.CeilToInt(len * segmentsPerLength) + 1;

        lineRenderer.positionCount = posCount;

        //Since the lighting is always in the XY plane (since this is 2d) the eccentricity should be normal to the direction
        var eccDir = Vector3.Cross(dir, Vector3.forward).normalized;
        for (int i = 0; i < posCount; i++)
        {
            var pos = ((float)i / (float)(posCount - 1)) * dir + startPos;
            var ecc = i == 0 || i == posCount - 1
                ? 0
                : Perlin(noiseStrength, noiseFrequency, new Vector2(pos.x, pos.y), octaves);
            lineRenderer.SetPosition(i, pos + ecc * eccDir);
        }

        StartCoroutine(Fade());
    }

    public IEnumerator Fade()
    {
        float elapsed = 0f;
        while (elapsed < fadeTime)
        {
            float p = (elapsed / fadeTime);
            float l = fadeCurve.Evaluate(p);
            _colorBlock.SetColor("_TintColor", new Color(1,1,1,l));
            lineRenderer.SetPropertyBlock(_colorBlock);
            elapsed += Time.deltaTime;
            yield return null;
        }
        Clear();
    }

    public void Clear()
    {
        lineRenderer.positionCount = 0;
    }


    public static float Perlin(float strength, float frequency, Vector2 pos, int octaves)
    {
        const float frequencyMult = 2f;
        const float amplitudeMult = 0.5f;
        float val = 0f;
        for (int i = 0; i < octaves; i++)
        {
            val += (Mathf.PerlinNoise(pos.x * frequency, pos.y * frequency) - 0.5f) * strength;
            frequency *= frequencyMult;
            strength *= amplitudeMult;
        }

        return val;
    }
}