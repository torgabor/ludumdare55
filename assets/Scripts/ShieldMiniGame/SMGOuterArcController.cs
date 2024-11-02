using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SMGOuterArcController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("trigger name " + collision.gameObject.name);
        if (collision.gameObject.tag == "Bullet")
        {
            gameObject.GetComponent<SpriteRenderer>().enabled = false;
        }
    }
}
