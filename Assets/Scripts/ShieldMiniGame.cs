using Assets.Scripts;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShieldMiniGame : AudioSyncer, IMiniGame
{
    public static ShieldMiniGame Instance;

    public LightningRenderer lightningRenderer1;
    public LightningRenderer lightningRenderer2;
    public bool isActive = false;
    public bool isEnabled = false;

    public float startOffset;
    public float endOffset;
    public float activationDistance = 0.8f;
    public int ShieldCount = 12;
    public GameObject ShieldArcPrefab;
    public GameObject BulletPrefab;
    public GameObject Catcher;
    public List<AudioClip> ArpLoops;
    public AudioClip ShieldUpSound;
    public AudioClip ShieldDownSound;
    public int PatternLength = 16;
    public int LevelBeats = 32;

    private List<SMGShieldController> shieldArcs = new();
    private List<GameObject> bullets = new();
    private int Level = 0;
    private int ActiveShields = 0;
    private Camera mainCamera;
    private List<int> bulletOrder = new();
    private int currentBullet = 0;
    private AudioPlayerSync ArpLoopTrack;
    private AudioPlayerSync OneShotTrack;
    public int StartBeat = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    void RandomizeBulletOrder()
    {
        // Create a list of integers
        if (!bulletOrder.Any())
        {
            for (int i = 0; i < ShieldCount; i++)
            {
                bulletOrder.Add(i);
            }
        }

        // Shuffle the list
        bulletOrder = bulletOrder.OrderBy(x => Random.value).ToList();
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
        if (!isEnabled) return;
        if (Mouse.current != null && mainCamera != null)
        {
            // Get the mouse position in screen space
            Vector2 mousePosition = Mouse.current.position.ReadValue();

            // Convert the screen position of the mouse to world position
            Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, transform.position.z));
            mouseWorldPosition.z = transform.position.z;
            float distance = (mouseWorldPosition - transform.position).magnitude;
            if (!Catcher.activeSelf && distance < activationDistance)
            {
                Catcher.SetActive(true);
            }
            else if (Catcher.activeSelf && distance > activationDistance)
            {
                Catcher.SetActive(false);
            }
        }
    }

    public override void OnStart()
    {
        base.OnStart();
        RandomizeBulletOrder();
        mainCamera = Camera.main;
        ArpLoopTrack = AudioManager.Instance.GetTrack(ArpLoops[0]);
        OneShotTrack = AudioManager.Instance.GetTrack(ShieldDownSound);
        OneShotTrack.Volume = 0.6f;
        for (int i = 0; i < ShieldCount; i++)
        {
            // place arcs in a circle
            var shield = Instantiate(ShieldArcPrefab, transform);
            shield.transform.rotation = Quaternion.Euler(0f, 0f, 360f / ShieldCount * i);
            shield.GetComponent<SMGShieldController>().ShieldNum = i;
            shieldArcs.Add(shield.GetComponent<SMGShieldController>());
        }
    }

    public void ActivateShield(int shieldNum)
    {
        var shield = shieldArcs[shieldNum];
        if (!shield.IsActive)
        {
            ActiveShields++;
            shield.Activate();
        }
        if (ActiveShields == ShieldCount && !isActive)
        {
            RandomizeBulletOrder();
            StartBeat = GetNextClosestBar(PatternLength);
            isActive = true;
            ArpLoopTrack.Loop(ArpLoops[0], StartBeat, StartBeat + LevelBeats);
            ArpLoopTrack.Loop(ArpLoops[1], StartBeat + LevelBeats, StartBeat + LevelBeats * 2);
            ArpLoopTrack.Loop(ArpLoops[2], StartBeat + LevelBeats * 2);
            bullets.ForEach(b => b.GetComponent<SMGBulletController>().Disable());
            OneShotTrack.Play(ShieldUpSound);
        }
    }

    public void DeactivateShield(SMGShieldController shield)
    {
        if (!shield.IsActive)
        {
            shieldArcs.FirstOrDefault(s => s.IsActive)?.Deactivate();
        }
        else
        {
            shield.Deactivate();
        }
        if (ActiveShields > 0)
        {
            ActiveShields--;
        }
        else if (isActive)
        {
            StopMiniGame();
            RandomizeBulletOrder();
        }
    }

    public void StopMiniGame()
    {
        ActiveShields = 0;
        Level = 0;
        if (isActive)
        {
            OneShotTrack.Play(ShieldDownSound);
        }
        isActive = false;
        ArpLoopTrack.Stop(GetNextClosestBar(4));
    }

    public void Enable()
    {
        isEnabled = true;
        isActive = false;
        ActiveShields = 0;
    }

    public void Disable()
    {
        isEnabled = false;
        StopMiniGame();
        shieldArcs.ForEach(arc => arc.Deactivate());
    }

    MonsterController MobWithoutShield()
    {
        foreach (var monster in SpawnController.Instance.Monsters)
        {
            if (!monster.HasShield)
                return monster;
        }

        return null;
    }

    void FireBullet()
    {
        int currentBeat = CurrentPartialBeat;
        if (!isEnabled || (isActive && currentBeat < StartBeat) || (currentBeat % 2 == 1)) { return; }
        var bullet = bullets.FirstOrDefault(b => !b.activeSelf);
        if (bullet == null)
        {
            bullet = Instantiate(BulletPrefab, transform);
            bullets.Add(bullet);
        }
        int shieldNum = bulletOrder[currentBullet % ShieldCount];
        currentBullet++;
        float angle = 360f / ShieldCount * shieldNum;
        bullet.GetComponent<SMGBulletController>().Launch(shieldNum, angle, transform.position);
    }

    private bool alternate = false;

    void AddShieldToMonster()
    {
        if (!isActive || CurrentBeat < StartBeat)
        {
            return;
        }
        var noShieldMonster = MobWithoutShield();

        if (noShieldMonster != null)
        {
            noShieldMonster.AddShield();
            var renderer = alternate ? lightningRenderer1 : lightningRenderer2;
            alternate = !alternate;
            var startPos = renderer.transform.position;
            var endPos = noShieldMonster.transform.position;
            var dir = (endPos - startPos).normalized;
            var startPosOffs = startPos + dir * startOffset;
            var endPosOffs = endPos - dir * endOffset;
            renderer.Shoot(startPosOffs, endPosOffs);
        }
    }

    public override void OnBeat()
    {
        FireBullet();

        if (CurrentPartialBeat % (4 / Mathf.Pow(2, Level)) == 0)
        {
            AddShieldToMonster();
        }

        if (isActive && CurrentBeat - StartBeat > LevelBeats * (Level + 1))
        {
            Level = Mathf.Min(2, Level + 1);
        }

        base.OnBeat();
    }
}