using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioSyncMove: AudioSyncer {

	private IEnumerator MoveToPos(Vector3 target)
	{
		Vector3 initial = transform.position;
		float _timer = 0;

		while (Mathf.Abs(_timer- m_settings.totalTime) > 0.01f)
		{
			float t = _timer / m_settings.totalTime;
			float tmod =m_settings.curve.Evaluate(t);
			var curr = Vector3.Lerp(initial, target, tmod);
			transform.position = curr;
			_timer += Time.deltaTime;

			yield return null;
		}

		m_isBeat = false;
	}
	

	public void MoveToTarget(Vector3 target)
	{
		StopCoroutine(nameof(MoveToPos));
		StartCoroutine(nameof(MoveToPos), target);
	}
}
