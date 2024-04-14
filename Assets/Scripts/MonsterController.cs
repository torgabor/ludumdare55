using UnityEngine;
using UnityEngine.Serialization;


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
            Destroy(this.gameObject);
            return;
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

    public void Die()
    {
        _isDying = true;
        var dieSound = dieSounds[Random.Range(0, dieSounds.Length)];
        AudioManager.Instance.Play(dieSound);
    }
}