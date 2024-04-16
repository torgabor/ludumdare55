using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class mpb : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        var mpb = new MaterialPropertyBlock();
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.GetPropertyBlock(mpb);
        float progress = Random.value;
        // Set the custom color property
        mpb.SetFloat("_Progress", progress);
        spriteRenderer.SetPropertyBlock(mpb);
    }
}
