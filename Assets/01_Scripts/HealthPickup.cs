using UnityEngine;
using System.Collections;

public class HealthPickup : MonoBehaviour
{   

   public float healAmount = 25f;
    public GameObject pickupEffect;

    private void OnTriggerEnter(Collider other)
    {
        HealthSystem health = other.GetComponent<HealthSystem>();
        if (health != null && health.IsAlive)
        {
            health.Heal(healAmount);
            Debug.Log($"[HealthPickup] Healed {other.name} for {healAmount}");

            if (pickupEffect != null)
                Instantiate(pickupEffect, transform.position, Quaternion.identity);

            Destroy(gameObject);
        }
    }
}