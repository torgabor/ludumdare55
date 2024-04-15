using System;
using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
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
    public Transform shootPosition;
    public ShooterController shootPrefab;

    private void Shoot()
    {
        var projectile = Instantiate(shootPrefab, shootPosition.position, quaternion.identity);
        projectile.moveExtents = moveExtents;
        projectile.OnBeat();
    }

    public bool HasShield
    {
        get => shieldRenderer.gameObject.activeInHierarchy;
        set => shieldRenderer.gameObject.SetActive(value);
    }

    private bool _isDying;

    public void Start()
    {
        mover = GetComponent<AudioSyncMove>();
        mover.Beat += OnBeat;
    }

    public void OnBeat()
    {
        if (_isDying)
        {
            DoDie();
            return;
        }

        bool shouldShoot = Random.value < shootChance;
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
        //AudioManager.Instance.Play(dieSound);
        Destroy(this.gameObject);
    }

    public void Hit()
    {
        _isDying = true;
        if (HasShield)
        {
            StartCoroutine(nameof(DestroyShield), true);
        }
    }

    IEnumerable DestroyShield(bool turnOn)
    {
        HasShield = true;
        var time = 0f;
        var targetTime = shieldFadeTime;
        while (time < targetTime)
        {
            var t = turnOn ? time / targetTime : 1.0f - time / targetTime;
            var color = shieldRenderer.color;
            color.a = t;
            shieldRenderer.color = color;
            time += Time.deltaTime;
            yield return null;
        }

        if (!turnOn)
        {
            HasShield = false;
        }
    }
}