using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    public float maxHealth;
    public float currentHealth;
    public float minDamageInterval = 0f;
    public float normalizedHealth => HelperUtilities.Remap(currentHealth, 0, maxHealth, 0, 1);
    public float timeOfLastDamage { get; private set; } = 0;
    public float timeSinceLastDamage => Time.time - timeOfLastDamage;

    public bool startWithMaxHealth = true;
    public bool IsAlive => !healthDepleted;
    public bool CanTakeDamage => timeSinceLastDamage >= minDamageInterval;


    public UnityEvent OnDamageTaken;
    public UnityEvent OnHealthDepleted;

    private bool healthDepleted = false;

    void Awake()
    {
        if (startWithMaxHealth)
        {
            ResetHealth();
        }
    }

    void Update()
    {
        if (!healthDepleted && currentHealth <= 0)
        {
            healthDepleted = true;
            OnHealthDepleted?.Invoke();
        }
    }

    public void UpdateHealth(float updateAmount)
    {
        currentHealth += updateAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
        healthDepleted = false;
    }

    public void TakeDamage(float damage)
    {
        if (CanTakeDamage)
        {
            UpdateHealth(-damage);
            OnDamageTaken?.Invoke();
            timeOfLastDamage = Time.time;
        }
    }
}