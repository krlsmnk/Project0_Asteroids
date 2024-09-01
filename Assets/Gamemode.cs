using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using TMPro;

public class GameMode : MonoBehaviour
{
    public static GameMode Instance { get; private set; }

    public GameObject asteroidPrefab;
    public int asteroidLimit;
    public float spawnCooldown;
    public float minSpeed;
    public float maxSpeed;
    public int asteroidSpawnScale;

    private float lastSpawnTime = 0f;
    private int currentAsteroids = 0;
    public int score, currentLevel;

    public AudioMixer audioMixer; // Reference to the AudioMixer
    private AudioSource musicSource;
    public AudioClip[] explosionSounds;
    public AudioSource explosionSource;

    private Canvas canvasHUD;
    private TextMeshProUGUI scoreNStuff;

    private float xMin, xMax, zMin, zMax;

    void Start()
    {
        canvasHUD = FindObjectOfType<Canvas>();
        asteroidLimit = 2;
        spawnCooldown = 5f;
        currentLevel = 1;
        minSpeed = 1f;
        maxSpeed = 2f;
        asteroidSpawnScale = 4;

        /*
        // Assuming the camera is orthographic and aligned to the screen's dimensions
            Camera cam = Camera.main;
            Vector3 screenBounds = cam.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, cam.transform.position.z));
            xMin = -screenBounds.x;
            xMax = screenBounds.x;
            zMin = -screenBounds.y;
            zMax = screenBounds.y;
        */

        xMin = -19;
        xMax = 19;
        zMin = -10;
        zMax = 10;
        score = 0;

        updateUI();
        

        musicSource = GetComponent<AudioSource>();
        if (musicSource == null) { Debug.LogError("No AudioSource component found on this GameObject."); }
        else
        {
            musicSource.Play();
            audioMixer.SetFloat("Pitch", 1);
        }

        StartCoroutine(SpawnAsteroids());
        StartCoroutine(WaitForAudioToEnd());


    }

    void updateUI()
    {
        if(canvasHUD == null) canvasHUD = FindObjectOfType<Canvas>();
        if(scoreNStuff == null) scoreNStuff = canvasHUD.GetComponentInChildren<TextMeshProUGUI>();

        scoreNStuff.text = "Score: " + score.ToString() + " \t \t Level: " + currentLevel.ToString();
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Optional: Keeps this instance alive across scenes
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
                asteroidScript.Initialize(asteroidSpawnScale);
            }

        currentAsteroids++;
    }

    public void HandleAsteroidDestroyed(bool biggus)
    {
        updateUI();

        //Debug.Log("Score: " + score);
        // Decrease the asteroid count
        if (biggus)currentAsteroids--;

        explosionSource.clip = explosionSounds[Random.Range(0, explosionSounds.Length)];
        explosionSource.Play();

    }

    public int GetScore()
    {
        return score;
    }

    void LevelRampUp()
    {
        currentLevel++;
        switch (currentLevel%3)
        {
            case 0:
                asteroidLimit++;
                break;
            case 1:
                minSpeed *= 1.5f;
                maxSpeed *= 1.5f;
                break;
            case 2:
                spawnCooldown *= 0.9f;
                asteroidSpawnScale++;
                break;
            default:
                break;
        }

        updateUI();

        //Speed up the music
        SetPlaybackSpeed(0.9f + (float)currentLevel/10.0f);
        musicSource.Play();
        StartCoroutine(WaitForAudioToEnd());


    }

    public void SetPlaybackSpeed(float playbackSpeed)
    {
        if (musicSource != null)
        {
            // Adjust the pitch in the AudioMixer without changing the actual pitch of the sound
            audioMixer.SetFloat("Pitch", playbackSpeed);
        }
    }

    IEnumerator WaitForAudioToEnd()
    {
        // Wait until the audio source is done playing
        while (musicSource.isPlaying)
        {
            yield return null; // Wait for the next frame
        }

        // Audio has finished playing, now trigger your function
        LevelRampUp();
    }

}
