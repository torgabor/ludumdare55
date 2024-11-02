using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SMGCatcherController : MonoBehaviour
{
    public Camera mainCamera;

    // Start is called before the first frame update
    void Start()
    {
        mainCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        if (Mouse.current != null && mainCamera != null)
        {
            // Get the mouse position in screen space
            Vector2 mousePosition = Mouse.current.position.ReadValue();

            // Convert the screen position of the mouse to world position
            Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, transform.position.z));

            // Calculate direction vector from this object to the mouse position
            Vector3 direction = mouseWorldPosition - transform.position;
            direction.z = 0; // Set z to 0 to keep the rotation on a 2D plane

            // Calculate the rotation that points towards the mouse
            Quaternion targetRotation = Quaternion.LookRotation(Vector3.forward, direction);

            // Apply the rotation to this game object
            transform.rotation = targetRotation;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Bullet"))
        {
            int shieldNum = collision.gameObject.GetComponent<SMGBulletController>().ShieldNum;
            ShieldMiniGame.Instance.ActivateShield(shieldNum);
        }
    }
}
