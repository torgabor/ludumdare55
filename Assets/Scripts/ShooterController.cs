using TMPro;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DefaultNamespace
{
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

        public void Start()
        {
            mover = GetComponent<AudioSyncMove>();
            mover.Beat += OnBeat;
            damage = AudioManager.Instance.GetTrack(DamageClip);
        }

        public void OnBeat()
        {
            var left = transform.position + Vector3.left * beatMoveDistance;
            var right = transform.position + Vector3.right * beatMoveDistance;
            bool leftBlocked = !moveExtents.bounds.Contains(left);
            bool rightBlocked = !moveExtents.bounds.Contains(right);

            bool isMoving = Random.value > 0.3;
            bool isLeft = Random.value < 0.5;
            if (isMoving)
            {
                if (leftBlocked)
                {
                    mover.MoveToTarget(right);
                }
                else if (rightBlocked)
                {
                    mover.MoveToTarget(left);
                }
                else
                {
                    mover.MoveToTarget(isLeft ? left : right);
                }
            }

            if (Random.value < shootChance)
            {
                Shoot();
            }
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
                m.Hit();
                GameController.Instance.Damage();
                damage.Play();
            }
        }
    }
}