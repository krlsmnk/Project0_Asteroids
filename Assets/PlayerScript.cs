using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour, IDamageableKarl
{
    public float moveSpeed = 1f;
    public float rotationSpeed = 150f;
    public GameObject bulletPrefab;
    public int bulletSpeed = 400;
    public float bulletSpawnOffset = 1f;
    public float shootCooldown;  // Time in seconds between shots
    private float lastShootTime = 0f;   // Time when the last shot was fired
    private int shielded =0;

    public Camera mainCamera;
    private bool dead;

    //IDamageable Interface Vars
    public int CurrentHealth { get; set; }
    public int MaxHealth { get; set; }
    public GameObject healthBarPrefab;
    private GameObject myHealthBarInstance;
    public Canvas canvasHUD;

    private bool useSlowMo = false;
    public ParticleSystem boomPrefab, shieldPrefab;
    private ParticleSystem shieldParticleInstance;

    private bool isJumping;
    private Collider shipCollider;
    private float jumpDuration, jumpHeight;
    private Vector3 movement; // To store movement input

    // Get the audioSource
    public AudioSource thrusters, deathExplosion;
    public AudioClip powerupPickupClip, shieldLossClip;

    public void updateHealthBar()
    {
        if (myHealthBarInstance == null) CreateHealthBarForSelf();
        myHealthBarInstance.GetComponent<HealthBarScript>().UpdateHealthBar(CurrentHealth, MaxHealth);
        //Debug.Log("Player sent: " + CurrentHealth + " / " + MaxHealth + "To the HealthBarScript");
    }

    public void TakeDamage(int damageAmount)
    {
        if(shielded > 0)
        {
            deathExplosion.PlayOneShot(shieldLossClip, 0.5f);
            shielded--;
            if(shielded <1) shieldParticleInstance.Stop();
            else if(shielded>=1) shieldParticleInstance.Play();
            var mainModule = shieldParticleInstance.main;
            mainModule.startSizeMultiplier = 0.25f + shielded;
        }
        else { 
            CurrentHealth -= damageAmount;
            CurrentHealth = (int)Mathf.Clamp(CurrentHealth, 0, MaxHealth);
            updateHealthBar();
            if (CurrentHealth <= 0) Die();
        }

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
        shootCooldown = 0.7f;
        jumpDuration = 1.25f;
        jumpHeight = 5f;
        shipCollider = GetComponent<Collider>();
        // Set the MaxHealth value
        MaxHealth = 25;
        // Initialize CurrentHealth to MaxHealth at the start
        CurrentHealth = MaxHealth;
        //Assign thruster audio
        thrusters = GetComponent<AudioSource>();
        useSlowMo = true;
        canvasHUD = FindObjectOfType<Canvas>();
            if (canvasHUD != null)
            {
                CreateHealthBarForSelf();
                updateHealthBar();
            }

        shieldParticleInstance = Instantiate(shieldPrefab, gameObject.transform.position + Vector3.up*0.25f, Quaternion.identity);
        shieldParticleInstance.transform.SetParent(gameObject.transform);
        shieldParticleInstance.Stop();

    }
    
    void FixedUpdate()
    {
        if(!dead) HandleMovement();        
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Backslash)) Die();
        if (!dead)
        {
            // Capture movement input (forward and backward)
            float moveInput = Input.GetAxis("Vertical");
            movement = transform.forward * moveInput * moveSpeed;

            if (Input.GetKeyDown(KeyCode.Space) && !isJumping)
            {
                StartCoroutine(Jump());
            }

            // Assuming the ship is on the XZ plane (Y is up)
            Plane plane = new Plane(Vector3.up, transform.position);
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (plane.Raycast(ray, out float enter))
            {
                Vector3 hitPoint = ray.GetPoint(enter);
                Vector3 direction = (hitPoint - transform.position).normalized;
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = lookRotation;
            }

            HandleShooting();
            HandleExit();

        }
        
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

    IEnumerator Jump()
    {
        isJumping = true;
        float initialY = transform.position.y;
        float elapsedTime = 0f;

        // Disable the collider during the jump
        shipCollider.enabled = false;

        // Ascend
        while (elapsedTime < jumpDuration / 2)
        {
            float newY = Mathf.Lerp(initialY, initialY + jumpHeight, elapsedTime / (jumpDuration / 2));
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Peak
        transform.position = new Vector3(transform.position.x, initialY + jumpHeight, transform.position.z);

        // Reset elapsed time for descent
        elapsedTime = 0f;

        // Descend
        while (elapsedTime < jumpDuration / 2)
        {
            float newY = Mathf.Lerp(initialY + jumpHeight, initialY, elapsedTime / (jumpDuration / 2));
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // End jump
        transform.position = new Vector3(transform.position.x, initialY, transform.position.z);

        // Re-enable the collider after the jump
        shipCollider.enabled = true;

        isJumping = false;
    }

    void HandleShooting()
    {
        if (Input.GetMouseButton(0))
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

    public void CreateHealthBarForSelf()
    {
        // Instantiate the health bar under the CanvasHUD
            myHealthBarInstance = Instantiate(healthBarPrefab, canvasHUD.transform);

            // Set the target and offset for the health bar
            HealthBarScript scriptInstance = myHealthBarInstance.GetComponent<HealthBarScript>();
            scriptInstance.targetToFollow = gameObject;
        
    }

    public void ActivatePowerup(string name, int value)
    {
        deathExplosion.PlayOneShot(powerupPickupClip, 0.35f);

        switch (name)
        {
            case "Health": CurrentHealth += value;
                if (CurrentHealth > MaxHealth) MaxHealth = CurrentHealth;
                updateHealthBar(); 
                break;
            case "Speed":
                moveSpeed += (float)value / 6;
                break;
            case "Shield":
                //enable shield particle
                shielded++;
                shieldParticleInstance.Play();
                var mainModule = shieldParticleInstance.main;
                mainModule.startSizeMultiplier = 0.25f + shielded;
                break;
            case "Shoot":
                shootCooldown = Mathf.Max(shootCooldown - (float)value / 100f, 0.05f);
                if(shootCooldown <= 0.05f)
                {
                    CurrentHealth += value;
                    if (CurrentHealth > MaxHealth) MaxHealth = CurrentHealth;
                    updateHealthBar();
                }
                break;
        }
    }


}
