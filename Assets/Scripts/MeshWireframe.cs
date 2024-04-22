using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class MeshWireframeC : MonoBehaviour
{
    public float inset = 0.1f;
    
    public Mesh originalMesh; 

#if UNITY_EDITOR
    private void OnValidate()
    {
        // (called whenever the object is updated)
        UpdateMesh();
    }
#endif

    public static (Vector3[] verts, Vector2[] uvs, int[] indices) BreakModelIntoTriangles(Mesh model)
    {
        Vector3[] vertices = model.vertices;
        Vector2[] uvs = model.uv;
        int[] indices = model.triangles;

        int triangleCount = indices.Length / 3;
        int vertexCount = triangleCount * 3;

        Vector3[] resultVertices = new Vector3[vertexCount];
        Vector2[] resultUVs = new Vector2[vertexCount];
        int[] resultIndices = new int[vertexCount];

        int vertexIndex = 0;

        for (int i = 0; i < indices.Length; i += 3)
        {
            for (int j = 0; j < 3; j++)
            {
                int index = indices[i + j];
                resultVertices[vertexIndex] = vertices[index];
                resultUVs[vertexIndex] = uvs[index];
                resultIndices[vertexIndex] = vertexIndex;
                vertexIndex++;
            }
        }

        return (vertices, uvs, indices);
    }

    public static void InsetTriangleMesh(Mesh mesh, Vector3[] vertices, Vector2[] uvs, int[] indices, float insetAmount)
    {
        int triangleCount = indices.Length / 3;
        int vertexCount = triangleCount * 6;
        int indexCount = triangleCount * 18;

        Vector3[] resultVertices = new Vector3[vertexCount];
        Vector2[] resultUVs = new Vector2[vertexCount];
        int[] resultIndices = new int[indexCount];

        int vertexIndex = 0;
        int indexIndex = 0;

        for (int i = 0; i < indices.Length; i += 3)
        {
            Vector3 v1 = vertices[indices[i]];
            Vector3 v2 = vertices[indices[i + 1]];
            Vector3 v3 = vertices[indices[i + 2]];

            Vector2 uv1 = uvs[indices[i]];
            Vector2 uv2 = uvs[indices[i + 1]];
            Vector2 uv3 = uvs[indices[i + 2]];

            Vector3 center = (v1 + v2 + v3) / 3f;
            Vector3 insetV1 = Vector3.Lerp(v1, center, insetAmount);
            Vector3 insetV2 = Vector3.Lerp(v2, center, insetAmount);
            Vector3 insetV3 = Vector3.Lerp(v3, center, insetAmount);

            // Original triangle vertices
            resultVertices[vertexIndex] = v1;
            resultVertices[vertexIndex + 1] = v2;
            resultVertices[vertexIndex + 2] = v3;

            // Inset triangle vertices
            resultVertices[vertexIndex + 3] = insetV1;
            resultVertices[vertexIndex + 4] = insetV2;
            resultVertices[vertexIndex + 5] = insetV3;

            // Original triangle UVs
            resultUVs[vertexIndex] = uv1;
            resultUVs[vertexIndex + 1] = uv2;
            resultUVs[vertexIndex + 2] = uv3;

            // Inset triangle UVs
            resultUVs[vertexIndex + 3] = uv1;
            resultUVs[vertexIndex + 4] = uv2;
            resultUVs[vertexIndex + 5] = uv3;
            

            // Edge triangle indices
            resultIndices[indexIndex ++] = vertexIndex + 0;
            resultIndices[indexIndex ++] = vertexIndex + 1;
            resultIndices[indexIndex ++] = vertexIndex + 3;
            
            resultIndices[indexIndex ++] = vertexIndex + 3;
            resultIndices[indexIndex ++] = vertexIndex + 1;
            resultIndices[indexIndex ++] = vertexIndex + 4;
            
            resultIndices[indexIndex ++] = vertexIndex + 4;
            resultIndices[indexIndex ++] = vertexIndex + 1;
            resultIndices[indexIndex ++] = vertexIndex + 2;
            
            resultIndices[indexIndex ++] = vertexIndex + 5;
            resultIndices[indexIndex ++] = vertexIndex + 4;
            resultIndices[indexIndex ++] = vertexIndex + 2;
            
            resultIndices[indexIndex ++] = vertexIndex + 0;
            resultIndices[indexIndex ++] = vertexIndex + 3;
            resultIndices[indexIndex ++] = vertexIndex + 2;
            
            resultIndices[indexIndex ++] = vertexIndex + 3;
            resultIndices[indexIndex ++] = vertexIndex + 5;
            resultIndices[indexIndex ++] = vertexIndex + 2;
            

            vertexIndex += 6;            
        }

        
        mesh.vertices = resultVertices;
        mesh.uv = resultUVs;
        mesh.triangles = resultIndices;
    }

    [ContextMenu("Update Mesh")]
    public void UpdateMesh()
    {
        if (!gameObject.activeSelf || !GetComponent<MeshRenderer>().enabled || ! originalMesh)
            return;



        var m = new Mesh();
        var (verts, uvs, indices) = BreakModelIntoTriangles(originalMesh);
        InsetTriangleMesh(m,verts, uvs, indices, inset);
        m.RecalculateNormals();
        m.name = originalMesh.name + " Wireframe";
        GetComponent<MeshFilter>().mesh = m;
    }
}