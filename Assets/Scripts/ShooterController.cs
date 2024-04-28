using TMPro;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;


public class ShooterController : MonoBehaviour
{
    public float beatMoveDistance;
    public SpriteRenderer moveExtents;
    public float shootChance = 0.5f;
    public Transform shootPosition;
    public ProjectileController shootPrefab;
    public Transform enemies;
    public AudioClip DamageClip;

    private AudioSyncMove mover;
    private AudioPlayerSync damage;
    private BoxCollider2D boxCollider;

    public void Start()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        mover = GetComponent<AudioSyncMove>();
        mover.Beat += OnBeat;
        damage = AudioManager.Instance.GetTrack(DamageClip);
    }

    public void OnBeat()
    {
        var left = Vector3.left * beatMoveDistance;
        var right = Vector3.right * beatMoveDistance;
        bool leftBlocked = !CanMoveTo(left);
        bool rightBlocked = !CanMoveTo(right);

        bool isMoving = Random.value > 0.3;
        bool isLeft = Random.value < 0.5;
        if (isMoving)
        {
            Vector3 targetPos;

            if (leftBlocked)
                targetPos = right;
            else if (rightBlocked)
                targetPos = left;
            else
                targetPos = isLeft ? left : right ;

            mover.MoveToTarget(targetPos + transform.position);
        }

        if (Random.value < shootChance)
        {
            Shoot();
        }
    }

    private bool CanMoveTo(Vector3 dir)
    {
        if (!moveExtents.bounds.Contains(transform.position + dir))
        {
            return false;
        }

        var hit2D = Physics2D.Raycast(transform.position, dir, beatMoveDistance);
        if (hit2D.transform != null && hit2D.transform.GetComponent<ShooterController>() != null)
        {
            return false;
        }

        return true;
    }

    private void Shoot()
    {
        var projectile = Instantiate(shootPrefab, shootPosition.position, quaternion.identity);
        projectile.moveExtents = moveExtents;
        projectile.OnBeat();
    }


    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<MonsterController>() is { } m)
        {
            m.Hit(true);
            GameController.Instance.Damage();
            damage.Play();
        }
    }
}