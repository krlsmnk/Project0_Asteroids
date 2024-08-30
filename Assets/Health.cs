using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    public int maxHealth = 100;
    private int currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            Die();
        }

        Debug.Log(gameObject.name + ": I'm hit!");
    }

    void Die()
    {
        // Handle the object's death (e.g., destroy it, play an animation, etc.)
        Destroy(gameObject);
    }
}
