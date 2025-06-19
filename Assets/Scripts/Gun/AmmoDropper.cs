using UnityEngine;
using System.Collections.Generic;

public class AmmoDropper : MonoBehaviour
{
    [Header("Ammo Drop Settings")]
    [Tooltip("Prefabs of ammo to drop")]
    public GameObject[] ammoPrefabs;
    public GameObject[] weaponPrefabs;

    [Tooltip("Minimum distance between dropped ammo items")]
    public float minDistanceBetweenAmmo = 0.5f;

    [Tooltip("Maximum attempts to find valid position for each ammo")]
    public int maxPlacementAttempts = 10;

    [Tooltip("Radius around NPC where ammo can be dropped")]
    public float dropRadius = 0.7f;

    private bool isQuitting = false;

    void OnApplicationQuit()
    {
        isQuitting = true;
    }

    public void DropAmmo()
    {
        if (isQuitting) return;

        if (ammoPrefabs == null || ammoPrefabs.Length == 0) return;

        List<Vector3> placedPositions = new List<Vector3>();

        foreach (GameObject ammoPrefab in ammoPrefabs)
        {
            if (ammoPrefab == null) continue;

            Vector3? validPosition = FindValidPosition(placedPositions);
            if (validPosition.HasValue)
            {
                Instantiate(ammoPrefab, validPosition.Value, Quaternion.identity);
                placedPositions.Add(validPosition.Value);
            }
        }

        if (weaponPrefabs != null && weaponPrefabs.Length > 0)
        {
            foreach (GameObject weaponPrefab in weaponPrefabs)
            {
                if (weaponPrefab == null) continue;

                Vector3? validPosition = FindValidPosition(placedPositions);
                if (validPosition.HasValue)
                {
                    GameObject weapon = Instantiate(weaponPrefab, validPosition.Value, Quaternion.identity);
                    FirearmShooting firearm = weapon.GetComponent<FirearmShooting>();
                    placedPositions.Add(validPosition.Value);
                }
            }
        }
    }

    private Vector3? FindValidPosition(List<Vector3> existingPositions)
    {
        for (int i = 0; i < maxPlacementAttempts; i++)
        {
            Vector3 randomOffset = Random.insideUnitCircle * dropRadius;
            Vector3 candidatePosition = transform.position + randomOffset;

            bool positionValid = true;
            foreach (Vector3 pos in existingPositions)
            {
                if (Vector3.Distance(candidatePosition, pos) < minDistanceBetweenAmmo)
                {
                    positionValid = false;
                    break;
                }
            }

            if (positionValid)
            {
                return candidatePosition;
            }
        }

        // Fallback: position near NPC if no valid position found
        return transform.position + new Vector3(
            Random.Range(-0.3f, 0.3f),
            Random.Range(-0.3f, 0.3f),
            0
        );
    }

    // Optional: Draw drop radius in editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, dropRadius);
    }
}