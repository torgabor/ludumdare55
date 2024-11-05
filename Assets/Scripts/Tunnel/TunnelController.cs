using UnityEngine;
using System.Collections.Generic;
using System.IO.Compression;
using JetBrains.Annotations;

public enum SpawnSide
{
    Left = 0,
    Right = 1,
    Top = 2,
    Bottom = 3
}
    

public class TunnelController : AudioSyncer
{
    [SerializeField] private float tunnelZSpeed = 10f;        // Speed at which objects move through the tunnel    
    [SerializeField] private float tunnelWidth = 10f;        // Width of the tunnel (distance between opposite walls)
    [SerializeField] private float centerSpawnDistance = 2f; // Distance from center where objects initially spawn
    [SerializeField] private float segmentPauseTime = 0.2f;  // Time to pause between tunnel segments
    [SerializeField] private int numberOfSegments = 3;       // Number of segments the tunnel is divided into    
    
    [SerializeField] private TunnelMonoBehavior obstaclePrefab;      // Prefab to spawn as obstacles
    [SerializeField] private float spawnInterval = 1f;       // Time between obstacle spawns
    [SerializeField] private float minScale = 0.2f;          // Minimum scale of obstacles (when far away)
    [SerializeField] private float maxScale = 2f;            // Maximum scale of obstacles (when close to camera)
    
    public AnimationCurve moveCurve;
    [SerializeField] private int lingerBeats = 1;          

    public float tunnelZLength => tunnelZfar - tunnelZnear;

    public int faultClearBeats;

    public int requiredMatches = 3;

    private float spawnTimer;

    public float tunnelZfar = 20f;
    public float tunnelZnear = 10f;

    public float tickTime = 0.5f;
    private List<TunnelObject> tunnelObjects = new List<TunnelObject>();

    public InputActions inputActions;

    public int matchCount = 0;
    
    public LightningController lightningController;
    
    [System.Serializable]
    private class TunnelObject
    {
        public TunnelMonoBehavior gameObject;
        public float zPosition; // Virtual Z position for perspective calculation
        public Vector2 startPosition; // Initial spawn position
        public Vector2 targetPosition; // Target position for perspective calculation
        public SpawnSide spawnSide;
        public int currentSegment = 0;
        public float segmentTimer = 0f;
        public int lingerEndBeat = 0;
        public bool isLingering = false;
        
        public bool pauseMovement = false;
    }

    
    
    public override void OnUpdate()
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

    public override void OnBeat()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        // Check for input on key down only
        if (inputActions.Player.TunnelUp.triggered)
        {
            HandleMatch(SpawnSide.Top);
        }
        if (inputActions.Player.TunnelDown.triggered)
        {
            HandleMatch(SpawnSide.Bottom);
        }
        if (inputActions.Player.TunnelLeft.triggered)
        {
            HandleMatch(SpawnSide.Left);
        }
        if (inputActions.Player.TunnelRight.triggered)
        {
            HandleMatch(SpawnSide.Right);
        }
    }

    private void HandleMatch(SpawnSide side)
    {
        Debug.Log("Match on " + side);
        if( tunnelObjects.Count > 0 )        
        {
            var obj = tunnelObjects[tunnelObjects.Count - 1];
            if(obj.spawnSide == side ){
                obj.gameObject.TriggerResult(true);
                AddMatch();
            }
            else
            {
                obj.gameObject.TriggerResult(false);
                TriggerFault();
            }
            obj.pauseMovement = true;
            tunnelObjects.RemoveAt(tunnelObjects.Count - 1);
        }
    }

    private MonsterController MobWithoutShoot()
    {
        foreach (var monster in SpawnController.Instance.Monsters)
        {
            if (!monster.shootActive)
                return monster;
        }

        return null;
    }

    private void AddMatch()
    {
        //TODO: Add anims here
        matchCount++;
        if (matchCount == requiredMatches){
            var mob = MobWithoutShoot();
            if (mob != null){
                mob.SetShootActive();
                lightningController.ShootLightning(transform.position, mob.transform.position);
            }
        }
    }

    public override void OnStart()
    {
        inputActions = new InputActions();
        inputActions.Enable();
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
        
        var newObj = Instantiate(obstaclePrefab, transform.TransformPoint(spawnPosition), Quaternion.Euler(0, 0, rotation), transform);
        

        TunnelObject tunnelObj = new TunnelObject
        {
            gameObject = newObj,
            zPosition = tunnelZfar,
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
            if(obj.pauseMovement) continue;
            // Calculate segment boundaries based on numberOfSegments
            float segmentLength = (tunnelZfar - tunnelZnear) / numberOfSegments;
            float segmentStart = tunnelZfar - (obj.currentSegment + 1) * segmentLength;
            float segmentEnd = tunnelZfar - obj.currentSegment * segmentLength;

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
        float segmentZLength = tunnelZLength / numberOfSegments;
        float segmentStart = tunnelZLength - (obj.currentSegment + 1) * segmentZLength;
        float segmentEnd = tunnelZLength - obj.currentSegment * segmentZLength;
        
        // Move object closer (decrease Z)
        obj.zPosition -= tunnelZSpeed * Time.deltaTime;

        // Calculate progress within current segment
        float segmentZProgress = 1f - ((obj.zPosition - segmentStart) / segmentZLength);
        segmentZProgress = Mathf.Clamp01(segmentZProgress);

        // Calculate overall progress for scaling
        float zProgress = 1f-(obj.zPosition / tunnelZfar);
        float currentScale = Mathf.Lerp(minScale, maxScale, zProgress);
        float moveProgress = moveCurve.Evaluate(zProgress);

        // Calculate position (objects move toward edges as they get closer)
        Vector2 newPosition = Vector2.Lerp(obj.startPosition, obj.targetPosition, moveProgress);

        // Apply transformations
        obj.gameObject.transform.localPosition = newPosition;
        obj.gameObject.transform.localScale = new Vector3(currentScale, currentScale, 1f);
    }

    public void TriggerFault()
    {
        matchCount = 0;

        Debug.Log("Fault triggered");
        //TODO: Add fault animation 
    }

    private void CleanupObjects()
    {
        bool faultTriggered = false;
        foreach (var obj in tunnelObjects)
        {
            if (obj.isLingering && obj.lingerEndBeat > this.CurrentPartialBeat)
            {
                faultTriggered = true;
            }
        }

        if ( faultTriggered )
        {            
            foreach (var obj in tunnelObjects)
            {
                Destroy(obj.gameObject);
            }
            tunnelObjects.Clear();

            TriggerFault();
        }      
    }
    
    private void OnDrawGizmos()
    {
        // Draw inner rectangle (spawn positions)
        Gizmos.color = Color.cyan;
        Helpers.DrawRectangle(centerSpawnDistance, transform);
        
        // Draw outer rectangle (target positions)
        Gizmos.color = Color.red;
        Helpers.DrawRectangle(tunnelWidth/2f, transform);
        
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
        float segmentLength = tunnelZLength / numberOfSegments;        
        
        // Draw a sphere at each segment boundary
        for (int i = 0; i <= numberOfSegments; i++)
        {
            float zPos = i * segmentLength;
            Vector3 segmentPosition = transform.TransformPoint(new Vector3(0, 0, zPos));
            Gizmos.DrawWireSphere(segmentPosition, dotSize);
        }
    }



}
