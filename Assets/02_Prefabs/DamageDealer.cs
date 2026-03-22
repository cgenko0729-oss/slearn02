using UnityEngine;

public class DamageDealer : MonoBehaviour
{
    [Header("Damage Settings")]
    public float damageAmount = 10f;
    public float damageCooldown = 1f;

    private float lastDamageTime;

    private void OnTriggerStay(Collider other)
    {
        if (Time.time - lastDamageTime < damageCooldown)
            return;

        HealthSystem health = other.GetComponent<HealthSystem>();
        if (health != null && health.IsAlive)
        {
            health.TakeDamage(damageAmount);
            lastDamageTime = Time.time;
            Debug.Log($"[DamageDealer] Dealt {damageAmount} damage to {other.name}");
        }
    }
}