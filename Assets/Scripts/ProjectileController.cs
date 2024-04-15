using UnityEngine;
using UnityEngine.Serialization;


public class ProjectileController : MonoBehaviour
{
    public SpriteRenderer moveExtents;
    public Vector3 beatMove = new Vector3(0, 1, 0);
    private AudioSyncMove mover;
    private bool _isDying;
    public LayerMask collisionMask;

    public void Awake()
    {
        mover = GetComponent<AudioSyncMove>();
        mover.Beat += OnBeat;
    }

    public void OnBeat()
    {
        if  (_isDying ||!moveExtents.bounds.Contains(transform.position))
        {
            Destroy(this.gameObject);
        }

        var target = transform.position + beatMove;
        mover.MoveToTarget(target);
        // CheckForCollision();
    }

    bool CheckForCollision()
    {
        var hit = Physics2D.Raycast(transform.position, beatMove, beatMove.magnitude, collisionMask);
        if (hit.collider != null && hit.collider.GetComponent<MonsterController>() is {} m)
        {
            m.Hit();
            _isDying = true;
            return true;
        }

        return false;
    }
    public void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<MonsterController>() is { } m)
        {
            m.Hit();
            _isDying = true;
        }
    }
}