using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour, IDamageableKarl
{
    public float moveSpeed = 2f;
    public float rotationSpeed = 200f;
    public GameObject bulletPrefab;
    public int bulletSpeed = 400;
    public float bulletSpawnOffset = 1f;
    public float shootCooldown = 0.5f;  // Time in seconds between shots
    private float lastShootTime = 0f;   // Time when the last shot was fired

    public Camera mainCamera;
    private bool dead;

    //IDamageable Interface Vars
    public int CurrentHealth { get; set; }
    public int MaxHealth { get; set; }

    private bool useSlowMo = false;
    public ParticleSystem boomPrefab;

    // Get the audioSource
    public AudioSource thrusters, deathExplosion;

    public void TakeDamage(int damageAmount)
    {
        CurrentHealth -= damageAmount;
        CurrentHealth = (int)Mathf.Clamp(CurrentHealth, 0, MaxHealth);
        if (CurrentHealth <= 0) Die();

    }
    public void Die()
    {
        dead = true;
        StartCoroutine(HandleDeathSequence());
    }

    private IEnumerator HandleDeathSequence()
    {
        ParticleSystem boomInstance = Instantiate(boomPrefab, new Vector3(gameObject.transform.position.x, 1, gameObject.transform.position.z), Quaternion.identity);
        boomInstance.Play();
        yield return new WaitForSeconds(.5f);
                
        // Pause or slow down the game
        if (useSlowMo)
        {
            // Play the sound effect            
            //musicSource.SetFloat("Pitch", 0.02f);
            Time.timeScale = 0.1f; // Slow down the game
            Time.fixedDeltaTime = 0.1f * Time.timeScale; // Adjust physics step            
        }
        else
        {
            // Play the sound effect
            deathExplosion.Play();
            Time.timeScale = 0f; // Pause the game
        }        

        // Start the camera zoom effect
        StartCoroutine(ZoomCamera(boomInstance.main.duration * (1 / 0.1f) - 5f));
        //wait before playing the SFX
        yield return new WaitForSecondsRealtime(5.25f);
        deathExplosion.Play();
        yield return new WaitForSecondsRealtime(3.5f);
        StartCoroutine(shrinkShip(3200));
        // Wait until the particle effect is done
        yield return new WaitForSecondsRealtime(boomInstance.main.duration * (1/0.175f));

        // Resume normal game speed
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f; // Reset physics step

        Destroy(gameObject);
    }


    void Start()
    {
        // Set the MaxHealth value
        MaxHealth = 25;
        // Initialize CurrentHealth to MaxHealth at the start
        CurrentHealth = MaxHealth;
        //Assign thruster audio
        thrusters = GetComponent<AudioSource>();
        useSlowMo = true;
    }


    void FixedUpdate()
    {
        if(!dead) HandleMovement();        
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Backslash)) Die();
        if (!dead) HandleShooting();
        HandleExit();
    }

    void HandleMovement()
    {
        float moveInput = Input.GetAxis("Vertical");
        float rotateInput = Input.GetAxis("Horizontal");

        // Move the player forward and backward
        transform.Translate(Vector3.forward * moveInput * moveSpeed * Time.deltaTime);
        if (thrusters != null && moveInput != 0 && !thrusters.isPlaying) thrusters.Play();
        else if (moveInput==0)thrusters.Stop();

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

    private IEnumerator ZoomCamera(float duration)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            //mainCamera.orthographicSize = Mathf.Lerp(10, 3, elapsedTime / duration);
            mainCamera.transform.position = new Vector3 (Mathf.Lerp(0, gameObject.transform.position.x, elapsedTime / duration), Mathf.Lerp(15, 3, elapsedTime / duration), Mathf.Lerp(0, gameObject.transform.position.z, elapsedTime / duration));            
            elapsedTime += Time.unscaledDeltaTime; // Use unscaled time to ignore time scale changes
            //if (elapsedTime > duration/3);
            yield return null; // Wait for the next frame
        }        
    }

    private IEnumerator shrinkShip(float duration)
    {
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, elapsedTime / duration);
            elapsedTime += Time.unscaledDeltaTime; // Use unscaled time to ignore time scale changes
            yield return null; // Wait for the next frame
        }
        
    }

}
