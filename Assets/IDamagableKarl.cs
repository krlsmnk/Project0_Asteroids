using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageableKarl
{
    public int MaxHealth { get; set; }
    public int CurrentHealth { get; set; }
    public void TakeDamage(int DamageAmount);
    public void Die();
}
