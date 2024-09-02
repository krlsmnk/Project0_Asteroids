using UnityEngine;
using UnityEngine.UI;

public class Powerup : MonoBehaviour
{
    public string powerupName; // The name of the powerup
    public int powerupVar; // The variable associated with the powerup (could be a duration, strength, etc.)
    public Sprite[] powerupSprites;

    public void Initialize(string name, int value)
    {
        powerupName = name;
        powerupVar = value;
        switch (name)
        {            
            case "Speed": gameObject.GetComponent<SpriteRenderer>().sprite = powerupSprites[1];
                break;
            case "Shield": gameObject.GetComponent<SpriteRenderer>().sprite = powerupSprites[2];
                break;
            case "Shoot": gameObject.GetComponent<SpriteRenderer>().sprite = powerupSprites[3];
                break;
        }        
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the object that entered the trigger has the PlayerScript component
        PlayerScript player = other.GetComponent<PlayerScript>();

        if (player != null)
        {
            // Call the ActivatePowerup method on the player, passing the powerup's values
            player.ActivatePowerup(powerupName, powerupVar);

            // Destroy the powerup after it has been collected
            Destroy(gameObject);
        }
    }
}
