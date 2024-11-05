using UnityEngine;

public class WheelController : MonoBehaviour
{
    [Header("Speed Settings")]
    [SerializeField] private float currentSpeed = 0f;
    [SerializeField] private float maxSpeed = 100f;
    [SerializeField] private float minSpeed = 0f;
    [SerializeField] private float idealSpeedMin = 40f;
    [SerializeField] private float idealSpeedMax = 60f;
    [SerializeField] private float speedChangeRate = 5f;

    [Header("Random Speed Variation")]
    [SerializeField] private float randomVariationMin = -2f;
    [SerializeField] private float randomVariationMax = 2f;
    [SerializeField] private float variationInterval = 0.5f;

    [Header("Visual Feedback")]
    [SerializeField] private Material wheelMaterial;
    [SerializeField] private Color warningColor = Color.red;
    [SerializeField] private Color slowColor = Color.blue;
    [SerializeField] private float flashDuration = 0.2f;

    private Color originalColor;
    private float nextVariationTime;
    private float flashEndTime;

    private void Start()
    {
        if (wheelMaterial == null)
        {
            wheelMaterial = GetComponent<Renderer>()?.material;
        }
        if (wheelMaterial != null)
        {
            originalColor = wheelMaterial.color;
        }
    }

    private void Update()
    {
        // Apply random speed variations
        if (Time.time >= nextVariationTime)
        {
            float randomVariation = Random.Range(randomVariationMin, randomVariationMax);
            currentSpeed += randomVariation;
            nextVariationTime = Time.time + variationInterval;
        }

        // Clamp speed between min and max
        currentSpeed = Mathf.Clamp(currentSpeed, minSpeed, maxSpeed);

        // Rotate the wheel based on current speed
        transform.Rotate(Vector3.right * currentSpeed * Time.deltaTime);

        // Visual feedback
        UpdateVisualFeedback();
    }

    public void IncreaseSpeed()
    {
        currentSpeed += speedChangeRate * Time.deltaTime;
    }

    public void DecreaseSpeed()
    {
        currentSpeed -= speedChangeRate * Time.deltaTime;
    }

    private void UpdateVisualFeedback()
    {
        if (wheelMaterial == null) return;

        if (Time.time < flashEndTime)
        {
            // Flash is already in progress
            return;
        }

        if (currentSpeed < idealSpeedMin)
        {
            StartFlash(slowColor);
        }
        else if (currentSpeed > idealSpeedMax)
        {
            StartFlash(warningColor);
        }
        else
        {
            wheelMaterial.color = originalColor;
        }
    }

    private void StartFlash(Color flashColor)
    {
        wheelMaterial.color = flashColor;
        flashEndTime = Time.time + flashDuration;
        Invoke(nameof(ResetColor), flashDuration);
    }

    private void ResetColor()
    {
        wheelMaterial.color = originalColor;
    }
}
