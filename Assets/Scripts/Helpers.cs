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

     public static Sprite GenerateArcSprite(float arcDegrees, float lineWidth, Color arcColor, int textureSize = 256)
    {
        // Create a new texture
        Texture2D texture = new Texture2D(textureSize, textureSize);
        
        // Clear texture with transparent pixels
        Color[] pixels = new Color[textureSize * textureSize];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = Color.clear;
        texture.SetPixels(pixels);
        
        // Calculate center and drawing parameters
        Vector2 center = new Vector2(textureSize / 2f, textureSize / 2f);
        float pixelRadius = (textureSize / 2f) * 0.8f; // Slightly smaller to ensure padding
        int segments = 50;
        
        // Draw the arc
        float deltaTheta = (arcDegrees * Mathf.Deg2Rad) / segments;
        float theta = 0f;
        Vector2 prevPoint = Vector2.zero;
        
        for (int i = 0; i <= segments; i++)
        {
            float x = center.x + pixelRadius * Mathf.Cos(theta);
            float y = center.y + pixelRadius * Mathf.Sin(theta);
            Vector2 point = new Vector2(x, y);
            
            if (i > 0)
            {
                DrawLine(texture, prevPoint, point, lineWidth * (textureSize / 100f), arcColor);
            }
            
            prevPoint = point;
            theta += deltaTheta;
        }
        
        texture.Apply();
        
        return Sprite.Create(
            texture, 
            new Rect(0, 0, textureSize, textureSize), 
            new Vector2(0.5f, 0.5f), 
            100f
        );
    }
    
    private static void DrawLine(Texture2D tex, Vector2 from, Vector2 to, float width, Color color)
    {
        Vector2 direction = (to - from).normalized;
        Vector2 perpendicular = new Vector2(-direction.y, direction.x);
        
        for (float d = 0; d <= Vector2.Distance(from, to); d += 0.5f)
        {
            Vector2 point = Vector2.Lerp(from, to, d / Vector2.Distance(from, to));
            
            for (float w = -width/2; w <= width/2; w += 0.5f)
            {
                Vector2 pixelPos = point + perpendicular * w;
                int x = Mathf.RoundToInt(pixelPos.x);
                int y = Mathf.RoundToInt(pixelPos.y);
                
                if (x >= 0 && x < tex.width && y >= 0 && y < tex.height)
                {
                    tex.SetPixel(x, y, color);
                }
            }
        }
    }

    public static void DrawRectangle(float size, Transform transform)
    {
        Vector3[] points = new Vector3[]
        {
            new Vector3(-size, size, 0),
            new Vector3(size, size, 0),
            new Vector3(size, -size, 0),
            new Vector3(-size, -size, 0)
        };
        
        for (int i = 0; i < 4; i++)
        {
            Vector3 worldPoint1 = transform.TransformPoint(points[i]);
            Vector3 worldPoint2 = transform.TransformPoint(points[(i + 1) % 4]);
            Gizmos.DrawLine(worldPoint1, worldPoint2);
        }
    }
}