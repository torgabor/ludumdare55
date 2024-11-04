using UnityEngine;
using System.Collections.Generic;

public class ArcRotator : MonoBehaviour
{
    [System.Serializable]
    public class RingConfig
    {
        public float relativeSpeed = 1f;        
        public float rotationOffset = 0f;
        
    }

    [SerializeField]
    private GameObject arcPrefab;
    

    
    [SerializeField]
    private float baseSpeed = 30f;
    
    [SerializeField]
    private float startRadius = 1f;
    
    [SerializeField]
    private float ringSpacing = 0.3f;
    public float intensity = 1f;

    public float segmentAngle = 1f;

    [SerializeField]
    private List<RingConfig> ringConfigs = new List<RingConfig>();

    public Gradient gradient;

    [SerializeField, Range(0.0f, 1.0f)]
    public float ringWidth = 0.3f;

    public void Start()
    {
        GenerateRings();
        for (int i = 0; i < ringConfigs.Count; i++)
        {
            var child = transform.GetChild(i);
            float t = (float)i / (ringConfigs.Count - 1);
            child.GetComponent<MeshRenderer>().material.color = gradient.Evaluate(t) * intensity;
        }
    }


    //public MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();

    [ContextMenu("Generate Rings")]
    public void GenerateRings()
    {
        // Clear existing rings if any
        var children = transform.GetComponentsInChildren<ArcGenerator>();
        foreach (var child in children)
        {
            DestroyImmediate(child.gameObject);
        }

        // Generate new rings
        for (int i = 0; i < ringConfigs.Count; i++)
        {
            GameObject ring = Instantiate(arcPrefab, transform);
            ring.name = $"Ring_{i}";
            ring.layer = this.gameObject.layer;
            
            ArcGenerator arcGen = ring.GetComponent<ArcGenerator>();
            float currentRadius = startRadius + (i * ringSpacing);
            
            // Configure the arc
            arcGen.innerRadius = currentRadius;
            arcGen.outerRadius = currentRadius + (ringSpacing * ringWidth);
            arcGen.angle = segmentAngle;
            arcGen.Regenerate();

            // Set the material color
            MeshRenderer renderer = ring.GetComponent<MeshRenderer>();
            
            float t = (float)i / (ringConfigs.Count - 1);
            renderer.material.color = gradient.Evaluate(t);
            //renderer.SetPropertyBlock(materialPropertyBlock);

        }
    }

    public void Update()
    {        
        float time = Time.time;
        var childCount = transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            var child = transform.GetChild(i);
            float rotationDelta = baseSpeed * ringConfigs[i].relativeSpeed * Time.deltaTime;
            child.Rotate(0, 0, rotationDelta);
        }
    }

    // private void OnValidate()
    // {
    //     if (Application.isPlaying)
    //     {
    //         GenerateRings();
    //     }
    // }
}
