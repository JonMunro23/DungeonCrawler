using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Security.Policy;
using UnityEngine;

public class SaveSystem
{
    public static Dictionary<int, SaveData> saveSlotDictionary = new Dictionary<int, SaveData>();
    public static SaveData saveData = new SaveData();

    [System.Serializable]
    public struct SaveData
    {
        public string saveName;
        public float gameTime;
        public LevelSaveData LevelData;
        public PlayerSaveData playerData;
    }

    public static string SaveFileName(string saveName)
    {
        string saveFile = $"{Application.persistentDataPath}/{saveName}.meme";
        return saveFile;
    }

    public static SaveData Save(int slotIndex, string saveName)
    {
        HandeSaveData(slotIndex, saveName);
        File.WriteAllText(SaveFileName(saveName), JsonUtility.ToJson(saveSlotDictionary[slotIndex], true));
        return saveSlotDictionary[slotIndex];
    }

    static void HandeSaveData(int slotIndex, string saveName)
    {
        GridController.Instance.Save(ref saveData);
        GridController.Instance.playerController.Save(ref saveData.playerData);

        saveData.saveName = saveName;

        if (saveSlotDictionary.ContainsKey(slotIndex))
        {
            saveSlotDictionary[slotIndex] = saveData;
        }
        else
        {
            saveSlotDictionary.Add(slotIndex, saveData);
        }  

    }

    public static void Load(int slotIndex, string saveName)
    {
        string saveContent = File.ReadAllText(SaveFileName(saveName));

        saveSlotDictionary[slotIndex] = JsonUtility.FromJson<SaveData>(saveContent);

        HandleLoadData(slotIndex);
    }

    static void HandleLoadData(int slotIndex)
    {
        GridController.Instance.Load(saveSlotDictionary[slotIndex].LevelData);
        GridController.Instance.playerController.Load(saveSlotDictionary[slotIndex].playerData);
    }

    public static Dictionary<int, SaveData> GetSaves()
    {
        DirectoryInfo dir = new DirectoryInfo(Application.persistentDataPath);
        FileInfo[] info = dir.GetFiles("*.*");

        int i = 0;
        foreach (FileInfo f in info)
        {
            if (f.Extension == ".meme")
            {
                if (saveSlotDictionary.ContainsKey(i))
                    continue;

                saveSlotDictionary.Add(i, JsonUtility.FromJson<SaveData>(File.ReadAllText($"{Application.persistentDataPath}/{f.Name}")));
                Debug.Log($"Added {saveSlotDictionary[i].saveName}");
                i++;
            }
        }

        return saveSlotDictionary;
    }
}
