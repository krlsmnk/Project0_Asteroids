using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Asteroid : MonoBehaviour
{
    public float scale;
    public float flySpeed;
    public float rotationSpeed;
    public float sizeThreshold; // If scale is below this, the asteroid pops instead of splitting

    private float damage;
    float distanceOffset = 1;

    private Vector3 rotationAxis;

    public delegate void AsteroidDestroyed(Asteroid asteroid);
    public event AsteroidDestroyed OnAsteroidDestroyed;

    void OnDestroy()
    {
        if (OnAsteroidDestroyed != null)
        {
            OnAsteroidDestroyed(this);
        }
    }


    void Start()
    {
        // Set the initial scale of the asteroid
        transform.localScale = Vector3.one * scale;

        // Randomize the rotation axis
        rotationAxis = Random.onUnitSphere;

        // Calculate damage based on size and speed
        damage = scale * flySpeed;
    }

    void Update()
    {
        // Rotate the mesh around the randomized axis
        transform.GetChild(0).Rotate(rotationAxis, rotationSpeed * Time.deltaTime);

        // Move the asteroid forward
        transform.Translate(Vector3.forward * flySpeed * Time.deltaTime);
    }

    void OnTriggerEnter(Collider collision)
    {
        
        //don't hit other rocks
        if (collision.gameObject.CompareTag("asteroid"))
        {
            return;
        }

        // Check if hit by a bullet
        if (collision.gameObject.CompareTag("bullet"))
        {
            Debug.Log("Hit by bullet!");

            // Call Split method if hit by a bullet
            Split();
            
        }
        else
        {

            Debug.Log("Hit by something!");
            // Deal damage to non-bullet object
            IDamageable damageable = collision.gameObject.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage);
            }

            // Call Split method
            Split();
        }
    }

    void Split()
    {
        if (scale > sizeThreshold)
        {
            // Calculate new scale and speed
            float newScale = scale / 2f;
            float newSpeed = flySpeed * 1.5f;

            // Spawn two smaller asteroids
            SpawnAsteroid(newScale, newSpeed, 35f);
            SpawnAsteroid(newScale, newSpeed, -35f);
        }

        // Destroy the current asteroid
        Destroy(gameObject);
    }

    void SpawnAsteroid(float newScale, float newSpeed, float angleOffset)
    {
        // Create a new rotation based on the angle offset
        Quaternion newRotation = Quaternion.Euler(0, angleOffset, 0) * transform.rotation;

        // Instantiate the new asteroid
        GameObject newAsteroid = Instantiate(gameObject, transform.forward*distanceOffset*angleOffset, newRotation);

        // Set the new asteroid's properties
        Asteroid asteroidScript = newAsteroid.GetComponent<Asteroid>();
        asteroidScript.scale = newScale;
        asteroidScript.flySpeed = newSpeed;
    }
}

public interface IDamageable
{
    void TakeDamage(float damage);
}
