using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthController : MonoBehaviour
{
    private float originalScaleX;

    // Start is called before the first frame update
    void Start()
    {
        originalScaleX = gameObject.transform.localScale.x;
    }

    public void SetHealth(float health)
    {
        gameObject.transform.localScale = new Vector3(originalScaleX * health, 1, 1);
    }
}
