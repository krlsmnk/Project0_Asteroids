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
    public GameObject healthBarPrefab;
    private GameObject myHealthBarInstance;
    public Canvas canvasHUD;

    public ParticleSystem dustPrefab, boomPrefab;

    [SerializeField] public string[] powerupNames;
    public GameObject powerupPrefab;


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
        updateHealthBar();
    }

    public void TakeDamage(int damageAmount)
    {
        //Debug.Log("Took Damage: " +  damageAmount);
        CurrentHealth -= damageAmount;
        CurrentHealth = (int)Mathf.Clamp(CurrentHealth, 0, MaxHealth);
        updateHealthBar();
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

        if(biggus)SpawnPowerup(gameObject.transform.position);

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

        canvasHUD = FindObjectOfType<Canvas>();
        if (canvasHUD != null)
        {
            CreateHealthBarForSelf();
            updateHealthBar();
        }
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
        //if we ram the player
        if (collision.gameObject.GetComponent<PlayerScript>() != null)
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
        else
        {
            return;
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

    public void updateHealthBar()
    {
        if (myHealthBarInstance == null) CreateHealthBarForSelf();
        myHealthBarInstance.GetComponent<HealthBarScript>().UpdateHealthBar(CurrentHealth, MaxHealth);
        //Debug.Log("Asteroid sent: " + CurrentHealth + " / " + MaxHealth + "To the HealthBarScript");
                
    }

    public void CreateHealthBarForSelf()
    {

        canvasHUD = FindObjectOfType<Canvas>();
        if (canvasHUD != null)
        {
            // Instantiate the health bar under the CanvasHUD
            myHealthBarInstance = Instantiate(healthBarPrefab, canvasHUD.transform);

            // Set the target and offset for the health bar
            HealthBarScript scriptInstance = myHealthBarInstance.GetComponent<HealthBarScript>();
            scriptInstance.targetToFollow = gameObject;
        }

    }

    private void SpawnPowerup(Vector3 spawnPosition)
    {
        bool sucessfulSpawn = Random.Range(0, 3) == 2;
        if (sucessfulSpawn)
        {
            // Choose a random index
            int randomIndex = Random.Range(0, powerupNames.Length);

            // Instantiate the powerup at the given position
            GameObject powerupObject = Instantiate(powerupPrefab, spawnPosition, powerupPrefab.transform.rotation);

            // Initialize the powerup with its corresponding name and value
            Powerup powerup = powerupObject.GetComponent<Powerup>();
            powerup.Initialize(powerupNames[randomIndex], scale);
        }
       
    }

}




