using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SMGShieldController : MonoBehaviour
{
    private SpriteRenderer sr;
    public bool IsActive => sr.enabled;
    public int ShieldNum;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    // Start is called before the first frame update
    void Start()
    {
        sr.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Deactivate()
    {
        sr.enabled = false;
    }

    public void Activate()
    {
        sr.enabled = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Bullet")
        {
            if (IsActive)
            {
                sr.enabled = false;
                ShieldMiniGame.Instance.DeactivateShield(this);
            }
        }
    }
}
