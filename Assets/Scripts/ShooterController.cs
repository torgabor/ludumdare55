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
        
        private AudioSyncMove mover;
        public float shootChance = 0.5f;
        public Transform shootPosition;
        public ProjectileController shootPrefab;
        public Transform enemies;
        
        public void Start()
        {
            mover = GetComponent<AudioSyncMove>();
            mover.Beat+=OnBeat;
        }
        
        public void OnBeat()
        {
            var left = transform.position + Vector3.left * beatMoveDistance;
            var right = transform.position + Vector3.right * beatMoveDistance;
            bool leftBlocked = !moveExtents.bounds.Contains(left);
            
            bool randomLeft = Random.value < 0.5;
            if (randomLeft && !leftBlocked)
            {
                mover.MoveToTarget(left);
            }
            else
            {
                mover.MoveToTarget(right);
            }
            if (Random.value < shootChance)
            {
                Shoot();
            }
        }

        private void Shoot()
        {
            var projectile = Instantiate(shootPrefab, shootPosition.position, quaternion.identity );
            projectile.moveExtents = moveExtents;
            projectile.OnBeat();
        }
    }
}