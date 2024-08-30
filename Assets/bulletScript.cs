using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bulletScript : MonoBehaviour
{
    public float lifetime = 2f;            // Bullet lifetime in seconds
    public int damage = 10;                // Damage value of the bullet
    public GameObject owner;               // Reference to the owner of the bullet
    public int speed;
    public Rigidbody bulletsRigidbody;

    void Start()
    {
        // Destroy the bullet after its lifetime expires
        Destroy(gameObject, 2f);
        bulletsRigidbody = GetComponent<Rigidbody>();

    }

    void Update()
    {
        bulletsRigidbody.velocity = transform.forward * 50f;
    }


    void OnTriggerEnter(Collider collision)
    {
        // Check if the bullet hits something other than its owner
        if (collision.gameObject != owner)
        {
            // Perform any actions you want on impact (e.g., deal damage, create an explosion, etc.)
            OnHit(collision.gameObject);

            // Destroy the bullet upon impact
            Destroy(gameObject);
        }
    }

    void OnHit(GameObject hitObject)
    {
        // Example: Apply damage to the hit object if it has a health component
        Health health = hitObject.GetComponent<Health>();
        if (health != null)
        {
            health.TakeDamage(damage);
        }
        else { Debug.Log("Hit something without health: " + hitObject.name); }

        // Additional logic can be added here (e.g., spawning effects)
    }
}