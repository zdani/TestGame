using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Health manager implementation
public class HealthManager : IHealthManager
{
    public float CurrentHealth { get; private set; }
    public float MaxHealth { get; private set; }
    public bool IsAlive => CurrentHealth > 0f;
    private Image _healthBarSprite;
    private MonoBehaviour _coroutineRunner;
    private Coroutine _healthAnimationCoroutine;
    
    [Header("Health Bar Animation Settings")]
    public float animationDuration = 0.3f;
    public AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    public HealthManager(float maxHealth, Image healthBarSprite, MonoBehaviour coroutineRunner)
    {
        MaxHealth = maxHealth;
        CurrentHealth = maxHealth;
        _healthBarSprite = healthBarSprite;
        _coroutineRunner = coroutineRunner;
        
        // Initialize health bar to full
        _healthBarSprite.fillAmount = 1f;
    }

    private void UpdateHealthBar()
    {
        // Stop any existing animation
        if (_healthAnimationCoroutine != null)
        {
            _coroutineRunner.StopCoroutine(_healthAnimationCoroutine);
        }
        
        // Start new animation
        _healthAnimationCoroutine = _coroutineRunner.StartCoroutine(AnimateHealthBar(CurrentHealth / MaxHealth));
    }

    private IEnumerator AnimateHealthBar(float targetFillAmount)
    {
        float startFillAmount = _healthBarSprite.fillAmount;
        float elapsedTime = 0f;

        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / animationDuration;
            float easedProgress = animationCurve.Evaluate(progress);
            
            _healthBarSprite.fillAmount = Mathf.Lerp(startFillAmount, targetFillAmount, easedProgress);
            
            yield return null;
        }

        // Ensure we reach the exact target value
        _healthBarSprite.fillAmount = targetFillAmount;
        _healthAnimationCoroutine = null;
    }

    public void TakeDamage(float damage)
    {
        if (!IsAlive) return;

        float oldHealth = CurrentHealth;
        CurrentHealth = Mathf.Max(0f, CurrentHealth - damage);

        // Update health bar directly
        UpdateHealthBar();

        // Trigger health changed event through GameEvents for other systems
        GameEvents.Instance.TriggerHealthChanged(CurrentHealth);

        if (CurrentHealth <= 0f && oldHealth > 0f)
        {
            // Trigger player died event through GameEvents
            GameEvents.Instance?.TriggerPlayerDied();
        }
    }

    public void Heal(float amount)
    {
        if (!IsAlive) return;

        float oldHealth = CurrentHealth;
        CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + amount);

        if (CurrentHealth != oldHealth)
        {
            // Update health bar directly
            UpdateHealthBar();
            
            // Trigger health changed event through GameEvents for other systems
            GameEvents.Instance.TriggerHealthChanged(CurrentHealth);
        }
    }

    public void SetMaxHealth(float maxHealth)
    {
        MaxHealth = maxHealth;
        CurrentHealth = Mathf.Min(CurrentHealth, MaxHealth);
    }
}
