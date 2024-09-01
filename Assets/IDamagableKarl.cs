using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageableKarl
{
    public int MaxHealth { get; set; }
    public int CurrentHealth { get; set; }
    //public GameObject healthBarPrefab;
    public void TakeDamage(int DamageAmount);
    public void updateHealthBar();
    public void CreateHealthBarForSelf();
    public void Die();
}
