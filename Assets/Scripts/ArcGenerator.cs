using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class ArcGenerator : MonoBehaviour
{
    [SerializeField, Range(0f, 360f)]
    public float angle = 90f;
    
    [SerializeField, Min(0.1f)]
    public float innerRadius = 1f;
    
    [SerializeField, Min(0.1f)]
    public float outerRadius = 2f;

    [SerializeField, Min(2)]
    public int segments = 64;
    private MeshFilter meshFilter;


    [ContextMenu("Regenerate")]
    public void Regenerate(){
        meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh = GenerateArc();
    }

    public Mesh GenerateArc()
    {
        var mesh = new Mesh();
        mesh.name = "Arc";
        // Ensure outer radius is always larger than inner radius
        outerRadius = Mathf.Max(outerRadius, innerRadius + 0.1f);
        
        int vertexCount = (segments + 1) * 2;
        Vector3[] vertices = new Vector3[vertexCount];
        Vector2[] uvs = new Vector2[vertexCount];
        int[] triangles = new int[segments * 6];

        float angleInRadians = angle * Mathf.Deg2Rad;
        float angleStep = angleInRadians / segments;

        // Generate vertices and UVs
        for (int i = 0; i <= segments; i++)
        {
            float currentAngle = angleStep * i;
            float x = Mathf.Cos(currentAngle);
            float y = Mathf.Sin(currentAngle);

            // Inner vertex
            vertices[i * 2] = new Vector3(x * innerRadius, y * innerRadius, 0);
            // Outer vertex
            vertices[i * 2 + 1] = new Vector3(x * outerRadius, y * outerRadius, 0);

            // UV mapping
            float uvX = innerRadius / outerRadius; // For inner vertex
            uvs[i * 2] = new Vector2(0f, currentAngle / (2f * Mathf.PI));
            uvs[i * 2 + 1] = new Vector2(1f, currentAngle / (2f * Mathf.PI));
        }

        // Generate triangles
        int triangleIndex = 0;
        for (int i = 0; i < segments; i++)
        {
            int baseIndex = i * 2;
            
            // First triangle
            triangles[triangleIndex++] = baseIndex + 2;
            triangles[triangleIndex++] = baseIndex + 1; 
            triangles[triangleIndex++] = baseIndex;

            // Second triangle
            triangles[triangleIndex++] = baseIndex + 2;
            triangles[triangleIndex++] = baseIndex + 3;
            triangles[triangleIndex++] = baseIndex + 1;
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }
}
