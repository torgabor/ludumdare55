using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SMGBulletController : MonoBehaviour
{
    public float Speed;
    private Rigidbody2D rb;
    public int ShieldNum;

    public void Launch(float angle)
    {
        if (rb == null) { rb = GetComponent<Rigidbody2D>(); }
        var rot = Quaternion.Euler(0, 0, angle);
        transform.rotation = rot;
        var rotVel = rot * Vector3.up;
        rb.velocity = new Vector2(rotVel.x, rotVel.y) * Speed;
        gameObject.SetActive(true);
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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        rb.velocity = Vector2.zero;
        gameObject.SetActive(false);
    }
}
