using UnityEngine;
using System.Collections;

public class ShockwaveSpawner : MonoBehaviour
{
    [SerializeField] private ShockwaveController shockwavePrefab;

    public void SpawnShockwave(Vector3 hitPoint, Vector3 hitNormal)
    {
        if (shockwavePrefab == null) return;

        ShockwaveController shockwave = Instantiate(
            shockwavePrefab,
            hitPoint + hitNormal * 0.02f,
            Quaternion.identity
        );

        shockwave.Play(hitNormal);
    }

    //test context menu
    [ContextMenu("Test Spawn Shockwave")]
    private void TestSpawnShockwave()
    {
        SpawnShockwave(transform.position, Vector3.up);
    }

}