using UnityEngine;
using System.Collections;

public class TunnelMonoBehavior : MonoBehaviour
{
    [SerializeField] private SpriteRenderer fillObject;
    
    
    [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 1, 1, 1.5f);
    [SerializeField] private AnimationCurve opacityCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private float animationDuration = 0.5f;
    [SerializeField] private Color successColor = Color.green;
    [SerializeField] private Color failColor = Color.red;
    [SerializeField] private float fadeDuration = 0.5f;

    public void TriggerResult(bool success)
    {
        StartCoroutine(AnimateResult(success));
    }

    private IEnumerator AnimateResult(bool success)
    {
        Color targetColor = success ? successColor : failColor;
        fillObject.color = targetColor;
        
        float elapsedTime = 0f;
        Vector3 startScale = transform.localScale;

        while (elapsedTime < animationDuration)
        {
            float normalizedTime = elapsedTime / animationDuration;
            
            
            float scaleMultiplier = scaleCurve.Evaluate(normalizedTime);
            transform.localScale = startScale * scaleMultiplier;
                        
            Color currentColor = fillObject.color;
            currentColor.a = opacityCurve.Evaluate(normalizedTime);
            fillObject.color = currentColor;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }

    public void FadeFail()
    {
        StartCoroutine(FadeOutAndDestroy());
    }

    private IEnumerator FadeOutAndDestroy()
    {
        float elapsedTime = 0f;

        Color startColor = fillObject.color;

        while (elapsedTime < fadeDuration)
        {
            float normalizedTime = elapsedTime / fadeDuration;
            
            Color currentColor = fillObject.color;
            currentColor.a = Mathf.Lerp(startColor.a, 0f, normalizedTime);
            fillObject.color = currentColor;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }
}
