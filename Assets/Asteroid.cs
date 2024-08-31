using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class Asteroid : MonoBehaviour, IDamageableKarl
{
    public int scale;
    public float flySpeed;
    public float rotationSpeed;
    public bool biggus = false;
    
    private int asteroidDamage;
    float distanceOffset = .75f;
    
    private Vector3 rotationAxis;

    public AudioClip[] crackSounds;
    public AudioClip impactPlayer;
    public AudioMixer audioMixer; // Reference to the AudioMixer
    public AudioSource audioPlayer;

    //IDamageable Interface Vars
    public int CurrentHealth { get; set;}
    public int MaxHealth { get; set; }

    public ParticleSystem dustPrefab, boomPrefab;


    public void Initialize(int customScale)
    {
        scale = customScale;
        biggus = true;
        // Set the initial scale of the asteroid
            transform.localScale = Vector3.one * customScale;
        // Calculate damage based on size and speed
            asteroidDamage = (int)Mathf.Floor(scale * flySpeed);
        // Set the MaxHealth value
            MaxHealth = scale;
        // Initialize CurrentHealth to MaxHealth at the start
            CurrentHealth = MaxHealth;
    }

    public void TakeDamage(int damageAmount)
    {
        //Debug.Log("Took Damage: " +  damageAmount);
        CurrentHealth -= damageAmount;
        CurrentHealth = (int)Mathf.Clamp(CurrentHealth, 0, MaxHealth);
        if (CurrentHealth <= 0) Split();
        else
        {
            audioPlayer.clip = crackSounds[Random.Range(0, crackSounds.Length)];
            audioPlayer.Play();
            ParticleSystem dustInstance = Instantiate(dustPrefab, new Vector3(gameObject.transform.position.x, 2, gameObject.transform.position.z), Quaternion.identity);
            dustInstance.startSize = dustInstance.startSize * scale;
            Destroy(dustInstance.gameObject, dustInstance.main.duration);

        }

    }
    public void Die()
    {
        GameMode.Instance.score += scale;
        GameMode.Instance.HandleAsteroidDestroyed(biggus);
        ParticleSystem boomInstance = Instantiate(boomPrefab, new Vector3(gameObject.transform.position.x, 2, gameObject.transform.position.z), Quaternion.identity);
        boomInstance.startSize = boomInstance.startSize * scale;
        Destroy(boomInstance.gameObject, boomInstance.main.duration); 
        Destroy(gameObject);
    }



    void Start()
    {
        //Debug.Log(biggus);

        // Set the initial scale of the asteroid
        transform.localScale = Vector3.one * scale;

        // Randomize the rotation axis
        rotationAxis = Random.onUnitSphere;

        // Calculate damage based on size and speed
        asteroidDamage = (int)Mathf.Floor(scale * flySpeed);

        // Set the MaxHealth value
        MaxHealth = scale;
        // Initialize CurrentHealth to MaxHealth at the start
        CurrentHealth = MaxHealth;

        audioPlayer = GetComponent<AudioSource>();
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

        //don't hit other rocks or walls
        if (collision.gameObject.GetComponent<WallWrap>() != null || collision.gameObject.GetComponent<Asteroid>()!=null)
        {
            return;
        }
        // Check if hit by a bullet
        else if (collision.gameObject.CompareTag("bullet"))
        {
            // Bullet Handles Dealing Damage to This                        
        }
        else
        {

            //Debug.Log("Hit by something!");
            // Deal damage to damagable objects
            IDamageableKarl damageableObject = collision.gameObject.GetComponent<IDamageableKarl>();
            if (damageableObject != null)
            {
                damageableObject.TakeDamage(asteroidDamage);
            }

            // Call TakeDamage method
            TakeDamage(1);
        }
    }

    void Split()
    {
        if (scale > 1)
        {
            // Calculate new scale and speed
            float newScale = scale / 2f;
            float newSpeed = flySpeed * 1.5f;

            // Spawn two smaller asteroids
            SpawnAsteroid(newSpeed, 25f);
            SpawnAsteroid(newSpeed, -25f);
        }

        Die();
        
    }

    void SpawnAsteroid(float newSpeed, float angleOffset)
    {
        // Create a new rotation based on the angle offset
        Quaternion newRotation = Quaternion.Euler(0, angleOffset, 0) * transform.rotation;

        // Calculate the new position
        Vector3 newPosition = transform.position +
                              transform.forward * distanceOffset +
                              transform.right * distanceOffset * angleOffset / 25f;

        // Instantiate the new asteroid
        GameObject newAsteroid = Instantiate(gameObject, newPosition, newRotation);


        // Set the new asteroid's properties
        Asteroid asteroidScript = newAsteroid.GetComponent<Asteroid>();
        asteroidScript.scale = scale/2;
        asteroidScript.flySpeed = newSpeed;
        asteroidScript.biggus = false;
    }
}




