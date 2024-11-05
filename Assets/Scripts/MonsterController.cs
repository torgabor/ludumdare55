using System;
using System.Collections;
using System.Collections.Generic;
using System.Transactions;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;


public enum MoveState
{
    MoveLeft,
    MoveRight,
    MoveDown
}

[RequireComponent(typeof(AudioSyncMove))]
public class MonsterController : MonoBehaviour
{
    public float beatMoveDistance;
    public SpriteRenderer moveExtents;
    public MoveState State = MoveState.MoveDown;
    private AudioSyncMove mover;
    public AudioClip[] dieSounds;
    public SpriteRenderer shieldRenderer;
    public float shieldFadeTime = 0.2f;
    public float shootChance = 0.1f;
    public bool shootActive = false;
    public Transform shootPosition;
    public InvaderProjectileController shootPrefab;
    public Gradient shieldGradient1;
    public Gradient shieldGradient2;
    private MaterialPropertyBlock shieldPropertyBlock;
    private AudioPlayerSync audioPlayer;

    public static int shieldMatCounter = 0;


    public Color shootColorBeat = Color.white;
    public Color shootColorRest = Color.white;

    private void Shoot()
    {
        var projectile = Instantiate(shootPrefab, shootPosition.position, quaternion.identity);
        projectile.moveExtents = moveExtents;
        projectile.OnBeat();
    }

    public bool HasShield => shieldRenderer.gameObject.activeInHierarchy;

    private bool _isDying;

    public void Start()
    {
        mover = GetComponent<AudioSyncMove>();
        mover.Beat += OnBeat;
        audioPlayer = AudioManager.Instance.GlobalAudioPlayer;
        var mat = new Material(shieldRenderer.material);
        shieldPropertyBlock = new MaterialPropertyBlock();
        
        mat.name += shieldMatCounter++;
        shieldRenderer.material = mat;
    }

    public void SetShootActive()
    {
        shootActive = true;
        var colorAnim = GetComponent<AudioSyncColor>();
        colorAnim.beatColor = shootColorBeat;
        colorAnim.restColor = shootColorRest;
    }

    public void OnBeat()
    {
        if (_isDying)
        {
            DoDie();
            return;
        }

        bool shouldShoot = Random.value < shootChance && shootActive;
        if (shouldShoot)
        {
            Shoot();
        }

        if (State == MoveState.MoveLeft)
        {
            var target = transform.position + Vector3.left * beatMoveDistance;
            if (!moveExtents.bounds.Contains(target))
            {
                State = MoveState.MoveDown;
            }
            else
            {
                mover.MoveToTarget(target);
            }
        }
        else if (State == MoveState.MoveRight)
        {
            var target = transform.position + Vector3.right * beatMoveDistance;
            if (!moveExtents.bounds.Contains(target))
            {
                State = MoveState.MoveDown;
            }
            else
            {
                mover.MoveToTarget(target);
            }
        }

        if (State == MoveState.MoveDown)
        {
            var target = transform.position + Vector3.down * beatMoveDistance;
            mover.MoveToTarget(target);
            //TODO what if reaching bottom
            var left = transform.position + Vector3.left * beatMoveDistance;
            State = moveExtents.bounds.Contains(left) ? MoveState.MoveLeft : MoveState.MoveRight;
        }
    }   

    private void DoDie()
    {
        var dieSound = dieSounds[Random.Range(0, dieSounds.Length)];
        audioPlayer.Volume = 0.4f;
        audioPlayer.Play(dieSound);
        SpawnController.Instance?.OnDie(this);
        Destroy(this.gameObject);
    }

    public void Hit(bool forceDie)
    {
        if (HasShield )
        {
            StartCoroutine(nameof(FadeShield), false);
            if (!forceDie)
            {
                return;
            }
        }

        _isDying = true;
        GetComponent<SpriteRenderer>().color = Color.red;
        var colorAnim = GetComponent<AudioSyncColor>();
        colorAnim.beatColor = Color.Lerp(Color.red, Color.white, 0.3f);
        colorAnim.restColor = Color.red;
        var scaleAnim = GetComponent<AudioSyncScale>();
        scaleAnim.beatScale = scaleAnim.beatScale * 1.2f;
        scaleAnim.restScale = scaleAnim.restScale * 1.2f;
    }

    public void AddShield()
    {
        StartCoroutine(nameof(FadeShield), true);
    }

    public IEnumerator FadeShield(bool turnOn)
    {
        shieldRenderer.gameObject.SetActive(true);
        var time = 0f;
        var targetTime = shieldFadeTime;
        while (time < targetTime)
        {
            var t = turnOn ? time / targetTime : 1.0f - time / targetTime;
            var color1 = shieldGradient1.Evaluate(t);
            var color2 = shieldGradient2.Evaluate(t);
            shieldPropertyBlock.SetColor("_Color1", color1);
            shieldPropertyBlock.SetColor("_Color2", color2);
            shieldRenderer.SetPropertyBlock(shieldPropertyBlock);
            time += Time.deltaTime;
            yield return null;
        }

        if (!turnOn)
        {
            shieldRenderer.gameObject.SetActive(false);
        }
    }
}
