using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SMGCatcherController : MonoBehaviour
{

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Bullet"))
        {
            int shieldNum = collision.gameObject.GetComponent<SMGBulletController>().ShieldNum;
            ShieldMiniGame.Instance.ActivateShield(shieldNum);
        }
    }
}
