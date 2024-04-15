using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioSyncScale : AudioSyncer
{


    private IEnumerator MoveToScale()
    {
        float _timer = 0;

        while (Mathf.Abs(_timer - m_settings.totalTime) > 0.01f)
        {

            float t = _timer / m_settings.totalTime;
            float tmod = m_settings.curve.Evaluate(t);
            var currScale = restScale * Mathf.Lerp(1f, this.beatScale, tmod);
            _timer += Time.deltaTime;

            transform.localScale = currScale;

            yield return null;
        }

        m_isBeat = false;
    }

    public override void OnBeat()
    {
        base.OnBeat();
        transform.localScale = restScale;
        StopCoroutine("MoveToScale");
        StartCoroutine("MoveToScale");
    }

    public void Start()
    {
        restScale = transform.localScale;
    }

    public float beatScale = 1.1f;
    public Vector3 restScale = Vector3.one;
}
