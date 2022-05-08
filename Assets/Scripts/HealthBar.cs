using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    float currentHealth;
    float maxHealth;
    public Slider healthBarUI;
    new Transform camera;
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

    private void Start()
    {
        camera = Camera.main.transform;

    }

    private void LateUpdate()
    {
        Vector3 v3T = transform.position + camera.transform.rotation * Vector3.forward;
        v3T.y = transform.position.y;
        transform.LookAt(v3T, Vector3.up);
    }
}
