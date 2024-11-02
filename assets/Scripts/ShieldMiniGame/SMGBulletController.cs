using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SMGBulletController : MonoBehaviour
{
    public float Speed;
    private Rigidbody2D rb;
    public int ShieldNum;

    public void Launch(int shieldNum, float angle, Vector3 pos)
    {
        ShieldNum = shieldNum;
        gameObject.SetActive(true);
        if (rb == null) { rb = GetComponent<Rigidbody2D>(); }
        var rot = Quaternion.Euler(0, 0, angle);
        transform.rotation = rot;
        transform.position = pos;
        var rotVel = rot * Vector3.up;
        rb.velocity = new Vector2(rotVel.x, rotVel.y) * Speed;
    }

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Disable()
    {
        rb.velocity = Vector2.zero;
        gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Disable();
    }
}
