using UnityEngine;

public class WeaponSystem : MonoBehaviour
{
    [Header("Weapon Settings")]
    public string weaponName = "Default Sword";
    public float damage = 25f;
    public float attackSpeed = 1.2f;
    public float attackRange = 2f;

    [Header("State")]
    public bool isAttacking = false;
    private float lastAttackTime;

    public void Attack()
    {
        if (Time.time - lastAttackTime < 1f / attackSpeed)
            return;

        isAttacking = true;
        lastAttackTime = Time.time;
        Debug.Log($"[WeaponSystem] Attacking with {weaponName} for {damage} damage!");

        // TODO: Add raycast hit detection
        // TODO: Add animation trigger
        // TODO: Add damage application to HealthSystem
    }

    // Still working on this...
    public void EquipWeapon(string name, float dmg, float speed)
    {
        weaponName = name;
        damage = dmg;
        attackSpeed = speed;
        // NOT FINISHED - need to add visual swap and audio

    }

}