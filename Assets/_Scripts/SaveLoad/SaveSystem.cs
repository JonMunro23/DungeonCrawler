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
        public LevelSaveData LevelData;
        public PlayerSaveData playerData;
    }

    public static string SaveFileName(int slotIndex)
    {
        string saveFile = $"{Application.persistentDataPath}/save{slotIndex}.meme";
        return saveFile;
    }

    public static void Save(int slotIndex)
    {
        HandeSaveData(slotIndex);
        File.WriteAllText(SaveFileName(slotIndex), JsonUtility.ToJson(saveSlotDictionary[slotIndex], true));
    }

    static void HandeSaveData(int slotIndex)
    {
        GridController.Instance.Save(ref saveData.LevelData);
        GridController.Instance.playerController.Save(ref saveData.playerData);

        if (saveSlotDictionary.ContainsKey(slotIndex))
        {
            saveSlotDictionary[slotIndex] = saveData;
        }
        else
        {
            saveSlotDictionary.Add(slotIndex, saveData);
        }  

    }

    public static void Load(int slotIndex)
    {
        string saveContent = File.ReadAllText(SaveFileName(slotIndex));

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
        return saveSlotDictionary;
    }
}
