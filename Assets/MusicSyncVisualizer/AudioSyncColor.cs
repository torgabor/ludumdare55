using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(SpriteRenderer))]
public class AudioSyncColor : AudioSyncer {
	
	private IEnumerator MoveToColor()
	{
		float _timer = 0;
		
		while (Mathf.Abs(_timer- m_settings.totalTime) > 0.01f)
		{
			float t = _timer / m_settings.totalTime;
			float tmod = m_settings.curve.Evaluate(t);
			var _curr = Color.Lerp(restColor, beatColor, tmod);
			_timer += Time.deltaTime;

			m_renderer.color = _curr;

			yield return null;
		}

		m_isBeat = false;
	}
	

	public override void OnBeat()
	{
		base.OnBeat();

		m_renderer.color = restColor;

		StopCoroutine(nameof(MoveToColor));
		StartCoroutine(nameof(MoveToColor));
	}

	private void Start()
	{
		m_renderer = GetComponent<SpriteRenderer>();
		restColor = m_renderer.color;
	}

	public Color beatColor;
	private Color restColor = Color.white;
	
	private SpriteRenderer m_renderer;
}
