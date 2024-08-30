using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMode : MonoBehaviour
{
    public GameObject asteroidPrefab;
    public int asteroidLimit = 10;
    public float spawnCooldown = 5f;
    public float levelIncrementSeconds = 30f;
    public float minSpeed = 1f;
    public float maxSpeed = 3f;

    private float timer = 0f;
    private float lastSpawnTime = 0f;
    private int currentAsteroids = 0;
    private int score = 0;

    private float xMin, xMax, zMin, zMax;

    void Start()
    {
        // Assuming the camera is orthographic and aligned to the screen's dimensions
        Camera cam = Camera.main;
        Vector3 screenBounds = cam.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, cam.transform.position.z));
        xMin = -screenBounds.x;
        xMax = screenBounds.x;
        zMin = -screenBounds.y;
        zMax = screenBounds.y;

        StartCoroutine(SpawnAsteroids());
    }

    void Update()
    {
        // Increment the timer
        timer += Time.deltaTime;

        // Decrease spawn cooldown over time
        if (timer >= levelIncrementSeconds)
        {
            spawnCooldown = Mathf.Max(1f, spawnCooldown - 0.5f); // Example of reducing cooldown, ensuring it doesn't go below 1 second
            timer = 0f;
        }
    }

    IEnumerator SpawnAsteroids()
    {
        while (true)
        {
            if (currentAsteroids < asteroidLimit && Time.time >= lastSpawnTime + spawnCooldown)
            {
                SpawnAsteroid();
                lastSpawnTime = Time.time;
            }

            yield return new WaitForSeconds(0.1f); // Check frequently
        }
    }

    void SpawnAsteroid()
    {
        // Determine a random edge of the screen to spawn the asteroid
        Vector3 spawnPosition = Vector3.zero;
        int edge = Random.Range(0, 4); // 0 = xMin, 1 = xMax, 2 = yMin, 3 = yMax

        switch (edge)
        {
            case 0: spawnPosition = new Vector3(xMin, 0f, Random.Range(zMin, zMax)); break;
            case 1: spawnPosition = new Vector3(xMax, 0f, Random.Range(zMin, zMax)); break;
            case 2: spawnPosition = new Vector3(Random.Range(xMin, xMax), 0f, zMin); break;
            case 3: spawnPosition = new Vector3(Random.Range(xMin, xMax), 0f, zMax); break;
        }

        // Instantiate the asteroid
        GameObject asteroid = Instantiate(asteroidPrefab, spawnPosition, Quaternion.identity);

        // Set the asteroid's forward direction towards the center
        Vector3 center = Vector3.zero; // Assuming the center is at (0,0)
        asteroid.transform.forward = (center - spawnPosition).normalized;

        // Assign a random speed within the specified range
        Asteroid asteroidScript = asteroid.GetComponent<Asteroid>();
        if (asteroidScript != null)
        {
            asteroidScript.flySpeed = Random.Range(minSpeed, maxSpeed);
            asteroidScript.OnAsteroidDestroyed += HandleAsteroidDestroyed; // Register to the destruction event
        }

        currentAsteroids++;
    }

    void HandleAsteroidDestroyed(Asteroid asteroid)
    {
        // Add scale to the score
        score += (int)asteroid.scale;

        // Decrease the asteroid count
        currentAsteroids--;
    }

    public int GetScore()
    {
        return score;
    }
}
