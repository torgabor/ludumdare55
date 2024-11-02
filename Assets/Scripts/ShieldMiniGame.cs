using Assets.Scripts;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShieldMiniGame : AudioSyncer, IMiniGame
{
    public static ShieldMiniGame Instance;

    public LightningRenderer lightningRenderer;
    public bool isActive = false;
    public bool isEnabled = false;
    [Range(0, 1)] public float shieldChance;

    public float startOffset;
    public float endOffset;
    public float activationDistance = 0.8f;
    public int ShieldCount = 12;
    public GameObject ShieldArcPrefab;
    public GameObject BulletPrefab;
    public GameObject Catcher;
    public int PatternResetBeatInterval = 16;
    public AudioClip ArpLoop;

    private List<SMGShieldController> shieldArcs = new();
    private List<GameObject> bullets = new();
    private int activeShields = 0;
    private Camera mainCamera;
    private List<int> bulletOrder = new();
    private int currentBullet = 0;
    private AudioPlayerSync ArpLoopTrack;
    private int startBeatActive = 0;

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
        ArpLoopTrack = AudioManager.Instance.GetTrack(ArpLoop);
        for (int i = 0; i < ShieldCount; i++)
        {
            // place arcs in a circle
            var shield = Instantiate(ShieldArcPrefab, transform);
            shield.transform.rotation = Quaternion.Euler(0f, 0f, 360f / ShieldCount * i);
            shield.GetComponent<SMGShieldController>().ShieldNum = i;
            shieldArcs.Add(shield.GetComponent<SMGShieldController>());
        }
        Disable();
    }

    public void ActivateShield(int shieldNum)
    {
        var shield = shieldArcs[shieldNum];
        if (!shield.IsActive)
        {
            activeShields++;
            shield.Activate();
        }
        if (activeShields == ShieldCount && !isActive)
        {
            isActive = true;
            RandomizeBulletOrder();
            startBeatActive = GetNextClosestBar(16);
            Debug.Log($"Starting Arp looop in {startBeatActive - GameController.Instance.GameMainLoopStartBeat} beats..");
            ArpLoopTrack.Loop(startBeatActive);
            bullets.ForEach(b => b.GetComponent<SMGBulletController>().Disable());
        }
    }

    public void DeactivateShield(SMGShieldController shield)
    {
        activeShields--;
        shield.Deactivate();
        if (activeShields == 0 && isActive)
        {
            StopMiniGame();
            RandomizeBulletOrder();
        }
    }

    public void StopMiniGame()
    {
        activeShields = 0;
        isActive = false;
        ArpLoopTrack.Stop(GetNextClosestBar(4));
    }

    public void Enable()
    {
        StopMiniGame();
        isEnabled = true;
    }

    public void Disable()
    {
        isEnabled = false;
        StopMiniGame();
        Catcher.SetActive(false);
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
        int currentBeat = CurrentBeat;
        if (!isEnabled || (isActive && currentBeat < startBeatActive)) { return; }
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

    void AddShieldToMonster()
    {
        if (!isActive)
        {
            return;
        }
        var addShield = Random.value < shieldChance;
        var noShieldMonster = MobWithoutShield();

        if (addShield && noShieldMonster != null)
        {
            noShieldMonster.AddShield();
            var startPos = lightningRenderer.transform.position;
            var endPos = noShieldMonster.transform.position;
            var dir = (endPos - startPos).normalized;
            var startPosOffs = startPos + dir * startOffset;
            var endPosOffs = endPos - dir * endOffset;
            lightningRenderer.Shoot(startPosOffs, endPosOffs);
        }
    }

    public override void OnBeat()
    {
        FireBullet();

        AddShieldToMonster();

        base.OnBeat();
    }
}