using UnityEngine;
using System.Collections.Generic;
using System.IO.Compression;

public class TunnelController : MonoBehaviour
{
    [Header("Tunnel Settings")]
    [SerializeField] private float tunnelSpeed = 10f;
    [SerializeField] private float tunnelLength = 20f;
    [SerializeField] private float tunnelWidth = 10f;
    [SerializeField] private float centerSpawnDistance = 2f;
    [SerializeField] private float segmentPauseTime = 0.2f;
    [SerializeField] private int numberOfSegments = 3;
    [SerializeField] private AnimationCurve movementCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
    
    [Header("Obstacle Settings")]
    [SerializeField] private GameObject obstaclePrefab;
    [SerializeField] private float spawnInterval = 1f;
    [SerializeField] private float minScale = 0.2f;
    [SerializeField] private float maxScale = 2f;
    
    [Header("Visualization Settings")]
    [SerializeField] private Color previewObstacleColor = new Color(1f, 1f, 1f, 0.3f); // White with 30% opacity
    
    [Header("Lingering Settings")]
    [SerializeField] private float lingerTime = 1f;
    
    private float spawnTimer;

    public float tickTime = 0.5f;
    private List<TunnelObject> tunnelObjects = new List<TunnelObject>();
    
    [System.Serializable]
    private class TunnelObject
    {
        public GameObject gameObject;
        public float zPosition; // Virtual Z position for perspective calculation
        public Vector2 startPosition; // Initial spawn position
        public Vector2 targetPosition; // Target position for perspective calculation
        public SpawnSide spawnSide;
        public int currentSegment = 0;
        public float segmentTimer = 0f;
        public float lingerTimer = 0f;
        public bool isLingering = false;

        // Calculate position within current segment as a Vector2
        public Vector2 GetSegmentPosition(float tunnelLength, float tunnelWidth)
        {
            float segmentLength = tunnelLength / currentSegment;
            float segmentStart = currentSegment * segmentLength;
            
            // Get relative position within segment (0 to 1)
            float relativePos = (zPosition - segmentStart) / segmentLength;
            relativePos = Mathf.Clamp01(relativePos);
            
            // Lerp between start and target positions based on segment progress
            return Vector2.Lerp(startPosition, targetPosition, relativePos);
        }


        
    }
    
    private void Update()
    {
        // Spawn new objects
        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval)
        {
            SpawnObstacle();
            spawnTimer = 0f;
        }
        
        // Update existing objects
        UpdateTunnelObjects();
        
        // Clean up objects that are too close (passed the camera)
        CleanupObjects();
    }
    
    private enum SpawnSide
    {
        Left = 0,
        Right = 1,
        Top = 2,
        Bottom = 3
    }
    
    private void SpawnObstacle()
    {
        if (obstaclePrefab == null) return;
        
        // Randomly choose a wall using the enum
        SpawnSide wall = (SpawnSide)Random.Range(0, 4);
        Vector2 spawnPosition = Vector2.zero;
        Vector2 targetPosition = Vector2.zero;
        float rotation = 0f;
        
        switch (wall)
        {
            case SpawnSide.Left:
                spawnPosition = new Vector2(-centerSpawnDistance, 0f);
                targetPosition = new Vector2(-tunnelWidth/2f, 0f);
                rotation = 180f;
                break;
            case SpawnSide.Right:
                spawnPosition = new Vector2(centerSpawnDistance, 0f);
                targetPosition = new Vector2(tunnelWidth/2f, 0f);
                rotation = 0;
                break;
            case SpawnSide.Top:
                spawnPosition = new Vector2(0f, centerSpawnDistance);
                targetPosition = new Vector2(0f, tunnelWidth/2f);
                rotation = 90f;
                break;
            case SpawnSide.Bottom:
                spawnPosition = new Vector2(0f, -centerSpawnDistance);  
                targetPosition = new Vector2(0f, -tunnelWidth/2f);
                rotation = -90f;
                break;
        }
        
        GameObject newObj = Instantiate(obstaclePrefab, transform.TransformPoint(spawnPosition), Quaternion.Euler(0, 0, rotation), transform);
        

        TunnelObject tunnelObj = new TunnelObject
        {
            gameObject = newObj,
            zPosition = tunnelLength,
            startPosition = spawnPosition,
            targetPosition = targetPosition,
            spawnSide = wall
        };
        
        tunnelObjects.Add(tunnelObj);
        SetObjectPosition(tunnelObj);
    }
    
    private void UpdateTunnelObjects()
    {
        foreach (var obj in tunnelObjects)
        {
            // Calculate segment boundaries based on numberOfSegments
            float segmentLength = tunnelLength / numberOfSegments;
            float segmentStart = tunnelLength - (obj.currentSegment + 1) * segmentLength;
            float segmentEnd = tunnelLength - obj.currentSegment * segmentLength;

            // Check if we need to pause between segments
            if (obj.zPosition <= segmentEnd && obj.currentSegment < numberOfSegments - 1)
            {
                obj.segmentTimer += Time.deltaTime;
                if (obj.segmentTimer >= segmentPauseTime)
                {
                    obj.currentSegment++;
                    obj.segmentTimer = 0f;
                }
                continue; // Skip movement during pause
            }

            SetObjectPosition(obj);
        }
    }
    
    private void SetObjectPosition(TunnelObject obj)
    {
        // Calculate segment boundaries using numberOfSegments
        float segmentLength = tunnelLength / numberOfSegments;
        float segmentStart = tunnelLength - (obj.currentSegment + 1) * segmentLength;
        float segmentEnd = tunnelLength - obj.currentSegment * segmentLength;
        
        // Move object closer (decrease Z)
        obj.zPosition -= tunnelSpeed * Time.deltaTime;

        // Calculate progress within current segment
        float segmentProgress = 1f - ((obj.zPosition - segmentStart) / segmentLength);
        segmentProgress = Mathf.Clamp01(segmentProgress);

        // Calculate overall progress for scaling
        float progressToCamera = 1f - (obj.zPosition / tunnelLength);
        float currentScale = Mathf.Lerp(minScale, maxScale, progressToCamera);

        // Calculate position (objects move toward edges as they get closer)
        Vector2 newPosition = Vector2.Lerp(obj.startPosition, obj.targetPosition, progressToCamera);

        // Apply transformations
        obj.gameObject.transform.localPosition = newPosition;
        obj.gameObject.transform.localScale = new Vector3(currentScale, currentScale, 1f);
    }

    private void CleanupObjects()
    {
        tunnelObjects.RemoveAll(obj =>
        {
            if (obj.zPosition <= 0f)
            {
                if (!obj.isLingering)
                {
                    obj.isLingering = true;
                    return false;
                }

                obj.lingerTimer += Time.deltaTime;
                if (obj.lingerTimer >= lingerTime)
                {
                    Destroy(obj.gameObject);
                    return true;
                }
            }
            return false;
        });
    }
    
    private void OnDrawGizmos()
    {
        // Draw inner rectangle (spawn positions)
        Gizmos.color = Color.cyan;
        DrawRectangle(centerSpawnDistance);
        
        // Draw outer rectangle (target positions)
        Gizmos.color = Color.red;
        DrawRectangle(tunnelWidth/2f);
        
        float dotSize = 0.1f;
        
        // Draw spawn points
        Gizmos.color = Color.blue;
        // Left spawn point
        Gizmos.DrawSphere(transform.TransformPoint(new Vector3(-centerSpawnDistance, 0, 0)), dotSize);
        // Right spawn point
        Gizmos.DrawSphere(transform.TransformPoint(new Vector3(centerSpawnDistance, 0, 0)), dotSize);
        // Top spawn point
        Gizmos.DrawSphere(transform.TransformPoint(new Vector3(0, centerSpawnDistance, 0)), dotSize);
        // Bottom spawn point
        Gizmos.DrawSphere(transform.TransformPoint(new Vector3(0, -centerSpawnDistance, 0)), dotSize);

        // Draw target points
        Gizmos.color = Color.green;
        float halfWidth = tunnelWidth/2f;
        // Left target point
        Gizmos.DrawSphere(transform.TransformPoint(new Vector3(-halfWidth, 0, 0)), dotSize);
        // Right target point
        Gizmos.DrawSphere(transform.TransformPoint(new Vector3(halfWidth, 0, 0)), dotSize);
        // Top target point
        Gizmos.DrawSphere(transform.TransformPoint(new Vector3(0, halfWidth, 0)), dotSize);
        // Bottom target point
        Gizmos.DrawSphere(transform.TransformPoint(new Vector3(0, -halfWidth, 0)), dotSize);

        // Draw segment boundary markers
        Gizmos.color = Color.magenta;
        float segmentLength = tunnelLength / numberOfSegments;        
        
        // Draw a sphere at each segment boundary
        for (int i = 0; i <= numberOfSegments; i++)
        {
            float zPos = i * segmentLength;
            Vector3 segmentPosition = transform.TransformPoint(new Vector3(0, 0, zPos));
            Gizmos.DrawWireSphere(segmentPosition, dotSize);
        }
    }


    private void DrawRectangle(float size)
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
