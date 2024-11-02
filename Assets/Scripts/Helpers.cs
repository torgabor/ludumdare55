using UnityEngine;

public static class Helpers
{
    public static Mesh GenerateArcMesh(float startAngle, float endAngle, float innerRadius, float outerRadius,
        int numSegments, float uvStart, float uvEnd)
    {
        Mesh mesh = new Mesh();

        Vector3[] vertices = new Vector3[(numSegments + 1) * 2];
        Vector2[] uv = new Vector2[(numSegments + 1) * 2];
        int[] triangles = new int[numSegments * 6];

        float angleIncrement = (endAngle - startAngle) / numSegments;
        float uvIncrement = (uvEnd - uvStart) / numSegments;


        for (int i = 0; i <= numSegments; i++)
        {
            float angle = i * angleIncrement + startAngle;
            float x = Mathf.Cos(angle * Mathf.Deg2Rad);
            float y = Mathf.Sin(angle * Mathf.Deg2Rad);

            var dir = new Vector3(x, y, 0f);
            vertices[i * 2] = dir * innerRadius;
            vertices[i * 2 + 1] = dir * outerRadius;
            uv[i * 2] = new Vector2(0f, uvStart + i * uvIncrement);
            uv[i * 2 + 1] = new Vector2(0f, uvStart + i * uvIncrement);
            if (i != 0)
            {
                int triangleIndex = (i - 1) * 6;
                triangles[triangleIndex] = (i - 1) * 2;
                triangles[triangleIndex + 1] = (i - 1) * 2 + 1;
                triangles[triangleIndex + 2] = (i) * 2 + 1;
                triangles[triangleIndex + 3] = i*2;
                triangles[triangleIndex + 4] = (i-1*2);
                triangles[triangleIndex + 5] = (i*2) + 1;
            }
        }

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;

        return mesh;
    }
}