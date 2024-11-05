using Assets.Scripts;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShieldMiniGame : AudioSyncer, IMiniGame
{


    public static ShieldMiniGame Instance;

    public bool isActive = false;
    public bool isEnabled = false;

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
    private int ActiveShields => shieldArcs.Count(s => s.IsActive);
    private Camera mainCamera;
    private AudioPlayerSync ArpLoopTrack;
    private AudioPlayerSync OneShotTrack;
    public int StartBeat = 0;

    public ArpBackgroundController ArpBackground;

    private InputActions inputActions;

    public LightningController lightningController;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    public override void OnUpdate()
    {
        base.OnUpdate();

        if (!isEnabled) return;
        var catcherRot = GetCatcherRot();
        if (!Catcher.activeSelf && catcherRot != null)
        {
            Catcher.SetActive(true);
        }
        else if (Catcher.activeSelf && catcherRot == null)
        {
            Catcher.SetActive(false);
        }
        if (catcherRot != null)
        {
            Catcher.transform.rotation = catcherRot.Value;
        }
    }

    private Quaternion? GetCatcherRot()
    {
        bool hasGamepad = Gamepad.all.Count > 0;
        if (hasGamepad)
        {
            // Get the gamepad position in screen space
            Vector3 gamepadPosition = inputActions.Player.Arpeggio.ReadValue<Vector2>();
            gamepadPosition.z = 0;
            return Quaternion.LookRotation(Vector3.forward, gamepadPosition);
        }
        if (Mouse.current != null && mainCamera != null)
        {
            // Get the mouse position in screen space
            Vector2 mousePosition = Mouse.current.position.ReadValue();

            // Convert the screen position of the mouse to world position
            Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, transform.position.z));
            mouseWorldPosition.z = transform.position.z;
            Vector3 direction = (mouseWorldPosition - transform.position);
            direction.z = 0;
            float distance = direction.magnitude;
            if (distance < activationDistance)
            {
                return Quaternion.LookRotation(Vector3.forward, direction);
            }
        }

        return null;
    }

    public override void OnStart()
    {
        base.OnStart();
        mainCamera = Camera.main;
        inputActions = new InputActions();
        inputActions.Enable();
        ArpLoopTrack = AudioManager.Instance.GetTrack(ArpLoops[0]);
        ArpBackground.soundEffectController.player = ArpLoopTrack;
        ArpBackground.gameObject.SetActive(true);
        OneShotTrack = AudioManager.Instance.GetTrack(ShieldDownSound);
        OneShotTrack.Volume = 0.6f;
        for (int i = 0; i < ShieldCount; i++)
        {
            // place arcs in a circle
            var shield = Instantiate(ShieldArcPrefab, transform);
            shield.transform.rotation = Quaternion.Euler(0f, 0f, -360f / ShieldCount * i);
            shield.GetComponent<SMGShieldController>().ShieldNum = i;
            shieldArcs.Add(shield.GetComponent<SMGShieldController>());
        }
    }

    public void ActivateShield()
    {
        shieldArcs.FirstOrDefault(s => !s.IsActive)?.Activate();
        if (ActiveShields == ShieldCount && !isActive)
        {
            StartBeat = GetNextClosestBar(PatternLength);
            isActive = true;
            ArpLoopTrack.Loop(ArpLoops[0], StartBeat, StartBeat + LevelBeats);
            ArpLoopTrack.Loop(ArpLoops[1], StartBeat + LevelBeats, StartBeat + LevelBeats * 2);
            ArpLoopTrack.Loop(ArpLoops[2], StartBeat + LevelBeats * 2);
            bullets.ForEach(b => b.GetComponent<SMGBulletController>().Disable());
            OneShotTrack.Play(ShieldUpSound);
            AudioSyncer.ScheduleAction(() => { ArpBackground.enableStars = true; }, GetNextClosestBar(16));
        }
    }

    public void DeactivateShield()
    {
        shieldArcs.LastOrDefault(s => s.IsActive)?.Deactivate();
        if (ActiveShields == 0 && isActive)
        {
            StopMiniGame();
        }
    }

    public void StopMiniGame()
    {
        ArpBackground.enableStars = false;
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
        if (!isEnabled || (isActive && CurrentBeat < StartBeat) || (CurrentPartialBeat % 2 == 1)) { return; }
        var bullet = bullets.FirstOrDefault(b => !b.activeSelf);
        if (bullet == null)
        {
            bullet = Instantiate(BulletPrefab, transform);
            bullets.Add(bullet);
        }

        int shieldTarget = Random.Range(0, ShieldCount);
        float angle = 360f / ShieldCount * shieldTarget;
        bullet.GetComponent<SMGBulletController>().Launch(shieldTarget, angle, transform.position);
    }

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
            lightningController.ShootLightning(
                lightningController.transform.position,
                noShieldMonster.transform.position
            );
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