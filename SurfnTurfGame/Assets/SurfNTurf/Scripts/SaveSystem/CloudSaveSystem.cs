using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CloudSaveSystem : MonoBehaviour
{
    [System.Serializable]
    public class SaveData
    {
        public int coinsCollectedCount = 0;
        public Vector3 playerPosition = new Vector3(0, 0, 0);
        public List<string> coinsCollected = new List<string>();
        public List<string> destructiblesBroken = new List<string>();
        public List<BoingData> boingDatas = new List<BoingData>();
        public List<string> surfboardsUnlocked = new List<string>();
        public int surfboardEquipped = 0;
        public List<GridData> allGrids = new List<GridData>();
        public bool finishedCookingTutorial = false;
        public List<ChallengeData> challengeDatas = new List<ChallengeData>();
    }

    private string saveFolder = "SaveData";
    private string saveFile = "savefile.json";
    public SaveData data = new SaveData();
    public bool IsInitialized { get; private set; } = false;

    //singlton instance;
    public static CloudSaveSystem Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        // Create a test save on start
        if (File.Exists(Path.Combine(Application.persistentDataPath, saveFolder, saveFile)))
        {
            Debug.Log("🔁Save file already exists. Loading existing data.");
            data = LoadFromFile();
        }
        else
        {
            Debug.Log("✴️No save file found. Creating a new one.");
            data = new SaveData
            {
                coinsCollected = new List<string> { }
            };
            SaveToFile(data);
        }

        IsInitialized = true;
    }

    public void SaveToFile(SaveData data)
    {
        string dirPath = Path.Combine(Application.persistentDataPath, saveFolder);
        if (!Directory.Exists(dirPath))
            Directory.CreateDirectory(dirPath);

        string json = JsonUtility.ToJson(data, true);
        string fullPath = Path.Combine(dirPath, saveFile);
        File.WriteAllText(fullPath, json);

        Debug.Log("✅Save written to: " + fullPath);
    }

    public SaveData LoadFromFile()
    {
        string fullPath = Path.Combine(Application.persistentDataPath, saveFolder, saveFile);

        if (File.Exists(fullPath))
        {
            string json = File.ReadAllText(fullPath);
            if (json != null)
            {
                Debug.Log("✅Save file loaded from: " + fullPath);
            }
            return JsonUtility.FromJson<SaveData>(json);
        }

        Debug.LogWarning("😵No save file found at: " + fullPath);
        return null;
    }

    public void DeleteSave()
    {
        IsInitialized = false;
        string fullPath = Path.Combine(Application.persistentDataPath, saveFolder, saveFile);
        if (File.Exists(fullPath))
        {
            data = new SaveData(); // Reset data to default
            File.Delete(fullPath);
            Debug.Log("💀Save file deleted at: " + fullPath);
        }
        else
        {
            Debug.LogWarning("😵No save file found to delete at: " + fullPath);
        }
        data.playerPosition = Vector3.zero;
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private void OnApplicationQuit()
    {
        SaveToFile(data);
    }
}