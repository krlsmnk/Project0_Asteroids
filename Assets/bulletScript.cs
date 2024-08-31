using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class bulletScript : MonoBehaviour
{
    public float lifetime = 2f;            // Bullet lifetime in seconds
    public int bulletDamage;           // Damage value of the bullet
    public GameObject owner;               // Reference to the owner of the bullet
    public int speed;
    public Rigidbody bulletsRigidbody;
    public AudioClip[] bulletSounds;

    void Start()
    {
        // Destroy the bullet after its lifetime expires
        Destroy(gameObject, 2f);
        bulletsRigidbody = GetComponent<Rigidbody>();

        //play randomBullet sound
        AudioSource.PlayClipAtPoint(bulletSounds[Random.Range(0, bulletSounds.Length)], gameObject.transform.position, 1f);

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
        // Deal damage to damagable objects
        IDamageableKarl damageableObject = hitObject.GetComponent<IDamageableKarl>();
        if (damageableObject != null)
        {
            damageableObject.TakeDamage(bulletDamage);
        }
        else 
        { 
            //Debug.Log("Hit something without interface: " + hitObject.name);        
        }

        // Additional logic can be added here (e.g., spawning effects)
    }
}