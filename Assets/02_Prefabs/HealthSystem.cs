using UnityEngine;
using UnityEngine.Events;

public class HealthSystem : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;

    [Header("Events")]
    public UnityEvent<float> OnHealthChanged;
    public UnityEvent OnDeath;

    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;
    public float HealthPercentage => currentHealth / maxHealth;
    public bool IsAlive => currentHealth > 0f;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        if (!IsAlive) return;

        currentHealth = Mathf.Max(0f, currentHealth - damage);
        OnHealthChanged?.Invoke(HealthPercentage);

        Debug.Log($"[HealthSystem] {gameObject.name} took {damage} damage. " +
                  $"HP: {currentHealth}/{maxHealth}");

        if (!IsAlive)
        {
            OnDeath?.Invoke();
            Debug.Log($"[HealthSystem] {gameObject.name} has died!");
        }
    }

    public void Heal(float amount)
    {
        if (!IsAlive) return;

        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        OnHealthChanged?.Invoke(HealthPercentage);

        Debug.Log($"[HealthSystem] {gameObject.name} healed {amount}. " +
                  $"HP: {currentHealth}/{maxHealth}");
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(HealthPercentage);
    }
}