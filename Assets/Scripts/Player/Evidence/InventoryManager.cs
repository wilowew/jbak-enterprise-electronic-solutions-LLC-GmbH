using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    private List<Evidence> collectedEvidence = new List<Evidence>();
    private string savePath;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            savePath = Path.Combine(Application.persistentDataPath, "savefile.json");
            LoadEvidence();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddEvidence(Evidence newEvidence)
    {
        collectedEvidence.Add(newEvidence);
        UpdateUI();
        SaveEvidence();
    }

    public List<Evidence> GetEvidenceList()
    {
        return collectedEvidence;
    }

    private void SaveEvidence()
    {
        EvidenceSaveData saveData = new EvidenceSaveData { evidenceList = collectedEvidence };
        string json = JsonUtility.ToJson(saveData);
        File.WriteAllText(savePath, json);
    }

    private void LoadEvidence()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            EvidenceSaveData saveData = JsonUtility.FromJson<EvidenceSaveData>(json);
            collectedEvidence = saveData.evidenceList;
            UpdateUI();
        }
    }

    private void UpdateUI()
    {

    }

    [System.Serializable]
    private class EvidenceSaveData
    {
        public List<Evidence> evidenceList;
    }
}