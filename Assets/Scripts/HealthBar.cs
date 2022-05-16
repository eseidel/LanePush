using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(LookAtCamera))]
public class HealthBar : MonoBehaviour
{
    float currentHealth;
    float maxHealth;
    public Slider healthBarUI;
    public void SetMaxHealth(float health)
    {
        maxHealth = health;
        UpdateSliderValue();
    }

    public void SetCurrentHealth(float health)
    {
        currentHealth = health;
        UpdateSliderValue();
    }

    void UpdateSliderValue()
    {
        healthBarUI.value = currentHealth / maxHealth;
    }
}
