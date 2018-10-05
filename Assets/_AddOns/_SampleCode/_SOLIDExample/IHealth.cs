using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHealth
{
    event System.Action<float> OnHPPctChanged;
    event System.Action OnDied;
    void TakeDamage(int amount);
}