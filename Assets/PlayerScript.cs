using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float rotationSpeed = 200f;
    public GameObject bulletPrefab;
    public int bulletSpeed = 400;
    public float bulletSpawnOffset = 1f;
    public float shootCooldown = 0.5f;  // Time in seconds between shots
    private float lastShootTime = 0f;   // Time when the last shot was fired

    void Update()
    {
        HandleMovement();
        HandleShooting();
        HandleExit();
    }

    void HandleMovement()
    {
        float moveInput = Input.GetAxis("Vertical");
        float rotateInput = Input.GetAxis("Horizontal");

        // Move the player forward and backward
        transform.Translate(Vector3.forward * moveInput * moveSpeed * Time.deltaTime);

        // Rotate the player
        transform.Rotate(Vector3.up, rotateInput * rotationSpeed * Time.deltaTime);
    }

    void HandleShooting()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Shoot();
        }
    }

    void Shoot()
    {
        if (bulletPrefab != null && Time.time >= lastShootTime + shootCooldown)
        {
            // Set bullet spawn point to player position + offset units in front
            (Instantiate(bulletPrefab, transform.position + transform.forward * bulletSpawnOffset, transform.rotation)).GetComponent<bulletScript>().speed = bulletSpeed;

            // Update the last shoot time to the current time
            lastShootTime = Time.time;
        }
    }

    void HandleExit()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }
    }
}
