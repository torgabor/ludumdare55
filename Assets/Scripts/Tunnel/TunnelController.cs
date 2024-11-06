using UnityEngine;
using System.Collections.Generic;
using System.IO.Compression;
using JetBrains.Annotations;
using UnityEngine.Timeline;

public enum SpawnSide
{
    Left = 0,
    Right = 1,
    Top = 2,
    Bottom = 3
}

public enum State
{
    Stopped, Faulted, Active
}


public class TunnelController : AudioSyncer
{
    public AudioClip FaultSound;
    private AudioPlayerSync OneShotTrack;    
    public List<AudioClip> TechnoLoops = new();

    private List<AudioPlayerSync> TechnoLoopTracks = new(); 

    [SerializeField] private float tunnelWidth = 10f;        // Width of the tunnel (distance between opposite walls)
    [SerializeField] private float centerSpawnDistance = 2f; // Distance from center where objects initially spawn

    [SerializeField] private int numberOfSegments = 3;       // Number of segments the tunnel is divided into    

    [SerializeField] private TunnelMonoBehavior obstaclePrefab;      // Prefab to spawn as obstacles

    [SerializeField] private float maxScale = 2f;            // Maximum scale of obstacles (when close to camera)

    public AnimationCurve moveCurve;
    [SerializeField] private int lingerBeats = 1;

    public float tunnelZLength => tunnelZfar;

    public int faultClearBeats;

    public int requiredMatches = 3;

    public int spawnBeats = 1;

    public int segmentBeats = 1;


    public State state = State.Stopped;

    public float tunnelZfar = 20f;
    public float tunnelZnear = 10f;

    private Queue<TunnelObject> tunnelObjects = new Queue<TunnelObject>();

    public InputActions inputActions;

    public int matchCount = 0;

    public int matchMax = 4;

    public float progressCircleLevel = 0f;

    public LightningController lightningController;

    public SpriteRenderer progressCircle;

    public MaterialPropertyBlock progressBlock;
    public SpriteRenderer mainSprite;

    public float pulseSpeed = 2f;
    public float progressSpeed = 1.6f;

    public int LevelBeats = 32;

    public int startBeat = 0;

    public int PatternLength = 8;

    public SpriteRenderer tunnelBackground;  // Add this field
    public float fadeSpeed = 2f;             // Add this field
    float currentBackgroundAlpha = 0f;

    [System.Serializable]
    private class TunnelObject
    {
        public TunnelMonoBehavior gameObject;

        public Vector2 startPosition; // Initial spawn position
        public Vector2 targetPosition; // Target position for perspective calculation
        public SpawnSide spawnSide;
        public int startBeat = 0;
    }

    public void Awake()
    {
        progressBlock = new MaterialPropertyBlock();
        mainSprite.GetPropertyBlock(progressBlock);
        progressBlock.SetFloat("_Progress", 0f);
        mpb = new MaterialPropertyBlock();
    }
    MaterialPropertyBlock mpb;
    public override void OnUpdate()
    {
        base.OnUpdate();

        // Update existing objects
        UpdateTunnelObjects();

        HandleInput();

        // Consolidated visual state handling
        float targetBackgroundAlpha = state == State.Active ? 1f : 0f;
        currentBackgroundAlpha = Mathf.Lerp(currentBackgroundAlpha, targetBackgroundAlpha, fadeSpeed * Time.deltaTime);
        tunnelBackground.GetPropertyBlock(mpb);
        mpb.SetFloat("_Alpha", currentBackgroundAlpha);
        tunnelBackground.SetPropertyBlock(mpb);

        // Main sprite color handling based on state
        if (state == State.Faulted)
        {
            float pulseAmount = (Mathf.Sin(Time.time * Mathf.PI * 2f * pulseSpeed) + 1f) * 0.5f;
            mainSprite.color = Color.Lerp(new Color(0.0f, 0f, 0f, 0f), Color.red, pulseAmount);
        }
        else if (state == State.Active)
        {
            float pulseAmount = (Mathf.Sin(Time.time * Mathf.PI * 2f * pulseSpeed) + 1f) * 0.5f;
            mainSprite.color = Color.Lerp(Color.white, new Color(1f, 0.5f, 0f), pulseAmount);
        }
        else
        {
            mainSprite.color = Color.white;
        }

        // Progress circle update
        var progress = (float)matchCount / matchMax;
        progressCircleLevel += (progress - progressCircleLevel) * progressSpeed * Time.deltaTime;
        progressBlock.SetFloat("_Progress", progressCircleLevel);
        progressCircle.SetPropertyBlock(progressBlock);

    }

    public override void OnBeat()
    {

        if (state != State.Faulted && (CurrentBeat % spawnBeats) == 0)
        {
            SpawnObstacle();
        }

        if (state == State.Active)
        {
            if ((CurrentBeat % 4) == 0)
            {
                var mob = MobWithoutShoot();
                if (mob != null)
                {
                    mob.SetShootActive();
                    lightningController.ShootLightning(transform.position, mob.transform.position);
                }
            }
        }
        bool triggerFault = false;
        foreach (var obj in tunnelObjects)
        {
            int lifeTime = CurrentBeat - obj.startBeat;
            int maxLifetime = (lingerBeats) + numberOfSegments * segmentBeats;
            if (lifeTime > maxLifetime)
            {
                triggerFault = true;
                break;
            }
        }
        if (triggerFault)
        {
            TriggerFault();
        }
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
        if (tunnelObjects.Count > 0)
        {
            var obj = tunnelObjects.Peek();
            if (obj.spawnSide == side)
            {
                obj.gameObject.TriggerResult(true);
                tunnelObjects.Dequeue();
                matchCount = Mathf.Min(matchCount + 1, matchMax);
                if (state == State.Stopped && matchCount == matchMax)
                {
                    state = State.Active;
                    startBeat = GetNextClosestBar(PatternLength);
                    foreach (var track in TechnoLoopTracks)
                    {
                        track.Stop(startBeat);
                    }
                    for (int i = 0; i < TechnoLoopTracks.Count; i++)
                    {
                        TechnoLoopTracks[i].Loop(startBeat + LevelBeats * i, startBeat + i == TechnoLoopTracks.Count - 1 ? int.MaxValue : startBeat + LevelBeats * (i + 1));
                    }
                }
            }
            else
            {
                TriggerFault();
            }
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



    public override void OnStart()
    {
        inputActions = new InputActions();
        inputActions.Enable();

        TechnoLoopTracks.Clear();
        for (int i = 0; i < TechnoLoops.Count; i++)
        {
            TechnoLoopTracks.Add(AudioManager.Instance.GetTrack(TechnoLoops[i]));
        }
        OneShotTrack = AudioManager.Instance.GetTrack(FaultSound);
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
                targetPosition = new Vector2(-tunnelWidth / 2f, 0f);
                rotation = 180f;
                break;
            case SpawnSide.Right:
                spawnPosition = new Vector2(centerSpawnDistance, 0f);
                targetPosition = new Vector2(tunnelWidth / 2f, 0f);
                rotation = 0;
                break;
            case SpawnSide.Top:
                spawnPosition = new Vector2(0f, centerSpawnDistance);
                targetPosition = new Vector2(0f, tunnelWidth / 2f);
                rotation = 90f;
                break;
            case SpawnSide.Bottom:
                spawnPosition = new Vector2(0f, -centerSpawnDistance);
                targetPosition = new Vector2(0f, -tunnelWidth / 2f);
                rotation = -90f;
                break;
        }

        var newObj = Instantiate(obstaclePrefab, transform.TransformPoint(spawnPosition), Quaternion.Euler(0, 0, rotation), transform);


        TunnelObject tunnelObj = new TunnelObject
        {
            gameObject = newObj,
            startPosition = spawnPosition,
            targetPosition = targetPosition,
            spawnSide = wall,
            startBeat = CurrentBeat
        };

        tunnelObjects.Enqueue(tunnelObj);
        SetObjectPosition(tunnelObj);
    }

    private void UpdateTunnelObjects()
    {
        foreach (var obj in tunnelObjects)
        {
            SetObjectPosition(obj);
        }
    }

    private void SetObjectPosition(TunnelObject obj)
    {

        var totalTime = numberOfSegments * segmentBeats * AudioSyncer.BeatInterval;
        var startTime = obj.startBeat * AudioSyncer.BeatInterval;
        var progress = (float)((AudioSyncer.DspTime - startTime) / totalTime);
        int segment = Mathf.FloorToInt(progress * numberOfSegments);
        var positionWithinSegment = (float)(progress - (float)segment / numberOfSegments);
        var actualPos = moveCurve.Evaluate(positionWithinSegment * numberOfSegments) /numberOfSegments;
        var adjustedZprogress = actualPos +  ((float)segment  / numberOfSegments);
        var zPosition = Mathf.Lerp(tunnelZfar, tunnelZnear, (float)adjustedZprogress);
        var scale = maxScale * tunnelZnear / zPosition;

        // Calculate position (objects move toward edges as they get closer)
        Vector2 newPosition = Vector2.Lerp(obj.startPosition, obj.targetPosition, adjustedZprogress);

        // Apply transformations
        obj.gameObject.transform.localPosition = newPosition;
        obj.gameObject.transform.localScale = new Vector3(scale, scale, 1f);
    }

    public void TriggerFault()
    {
        matchCount = 0;

        Debug.Log("Fault triggered");
        foreach (var track in TechnoLoopTracks)
        {
            track.Stop(GetNextClosestBar(PatternLength));
        }
        foreach (var obj in tunnelObjects)
        {
            obj.gameObject.TriggerResult(false);
        }
        tunnelObjects.Clear();
        state = State.Faulted;
        matchCount = 0;
        OneShotTrack.Play(FaultSound);
        ScheduleAction(() =>
        {
            state = State.Stopped;
        }, faultClearBeats + CurrentBeat);
    }


    private void OnDrawGizmos()
    {
        // Draw inner rectangle (spawn positions)
        Gizmos.color = Color.cyan;
        Helpers.DrawRectangle(centerSpawnDistance, transform);

        // Draw outer rectangle (target positions)
        Gizmos.color = Color.red;
        Helpers.DrawRectangle(tunnelWidth / 2f, transform);

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
        float halfWidth = tunnelWidth / 2f;
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

    }



}
