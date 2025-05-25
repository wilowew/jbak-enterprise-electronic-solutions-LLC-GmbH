using UnityEngine;

public class EvidenceItem : MonoBehaviour
{
    public Evidence evidenceData;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            InventoryManager.Instance.AddEvidence(evidenceData);
            gameObject.SetActive(false);
        }
    }
}