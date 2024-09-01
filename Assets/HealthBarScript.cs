using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBarScript : MonoBehaviour
{
    public GameObject healthBarPrefab, targetToFollow;
    private int healthBarVerticalOffset;
    

    public void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        if(healthBarPrefab == null) healthBarPrefab = gameObject;

        Slider healthSlider = healthBarPrefab.GetComponentInChildren<Slider>();
        healthSlider.value = (currentHealth/maxHealth)*100;

        TextMeshProUGUI healthText = healthBarPrefab.GetComponentInChildren<TextMeshProUGUI>();
        healthText.text = currentHealth.ToString();

        // Hide or unhide the health bar based on health status
        if (currentHealth == maxHealth)
        {
            gameObject.SetActive(false); // Hide the health bar if health is full
        }
        else
        {
            gameObject.SetActive(true); // Unhide the health bar if health is not full
        }

    }

    private void Start()
    {
        healthBarVerticalOffset = 1;
        healthBarPrefab = gameObject;
        /*
        //Get GameObject of type HealthBar prefab from this.parent
        
        //Get the IDamageableKarl interface on that object so you can get its currenthealth and maxhealth
        IDamageableKarl damageInterfaceRef = targetToFollow.GetComponent<IDamageableKarl>();
        //Add a listener to the "takeDamage" function on the object (required by the interface), and call UpdateHealthBar after takeDamage gets called
        UpdateHealthBar(damageInterfaceRef.CurrentHealth, damageInterfaceRef.MaxHealth); //Update HealthBar on BeginPlay
        */
    }

    void Update()
    {
        if (targetToFollow != null)
        {
            // Convert the world position of the target to screen space
            Vector3 screenPosition = Camera.main.WorldToScreenPoint(targetToFollow.transform.position + new Vector3(0, 0, healthBarVerticalOffset));

            // Get the RectTransform component of the health bar
            RectTransform rectTransform = gameObject.GetComponent<RectTransform>();

            // Adjust the anchored position in the canvas
            rectTransform.position = screenPosition;
        }
        else
        {
            Destroy(gameObject);
        }
    }

}
