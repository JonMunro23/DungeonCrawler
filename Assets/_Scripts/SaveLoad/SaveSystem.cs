using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Policy;
using UnityEngine;

public class SaveSystem
{
    public static List<SaveData> saveDatas = new List<SaveData>();
    public static SaveData saveData = new SaveData();

    public static List<FileInfo> saveFileInfo = new List<FileInfo>();

    public const string saveFileExtenstion = ".meme";

    [System.Serializable]
    public struct SaveData
    {
        public string saveName;
        public int saveIndex;
        public float gameTime;
        public string saveDate;
        public LevelSaveData LevelData;
        public PlayerSaveData playerData;
    }

    public static string SaveFileName(string saveName)
    {
        string saveFile = $"{Application.persistentDataPath}/{saveName}{saveFileExtenstion}";
        return saveFile;
    }

    public static SaveData Save(int slotIndex, string saveName)
    {
        HandeSaveData(slotIndex, saveName);

        string path = SaveFileName(saveName);
        File.WriteAllText(path, JsonUtility.ToJson(saveDatas[slotIndex], true));
        
        FileInfo fileInfo = new FileInfo(path);
        saveFileInfo.Add(fileInfo);

        return saveDatas[slotIndex];
    }

    static void HandeSaveData(int slotIndex, string saveName)
    {
        GridController.Instance.Save(ref saveData);
        GridController.Instance.playerController.Save(ref saveData.playerData);

        saveData.saveName = saveName;
        saveData.saveIndex = slotIndex;
        saveData.saveDate = System.DateTime.Now.ToString();
        
        foreach (SaveData data in saveDatas)
        {
            if(data.saveIndex == slotIndex)
            {
                Debug.Log($"overwritten {saveData.saveName}");
                saveDatas[slotIndex] = saveData;
                return;
            }
        }

        saveDatas.Add(saveData);
    }

    public static void Load(int slotIndex, string saveName)
    {
        HandleLoadData(slotIndex);
    }

    static void HandleLoadData(int slotIndex)
    {
        GridController.Instance.Load(saveDatas[slotIndex]);
        GridController.Instance.playerController.Load(saveDatas[slotIndex].playerData);
    }

    public static void GetSavesFromDirectory()
    {
        DirectoryInfo dir = new DirectoryInfo(Application.persistentDataPath);
        FileInfo[] info = dir.GetFiles("*.*");

        //saveDatas.Clear();
        //saveFileInfo.Clear();

        foreach (FileInfo f in info)
        {
            if (f.Extension == saveFileExtenstion)
            {
                saveFileInfo.Add(f);
                SaveData newSaveData = JsonUtility.FromJson<SaveData>(File.ReadAllText($"{Application.persistentDataPath}/{f.Name}"));
                saveDatas.Add(newSaveData);
            }
        }
    }

    public static List<SaveData> GetSaveData()
    {
        return saveDatas;
    }

    public static void DeleteSaveData(SaveData data)
    {
        saveDatas.Remove(data);
        foreach (FileInfo fileInfo in saveFileInfo)
        {
            if(fileInfo.Name == $"{data.saveName}{saveFileExtenstion}")
            {
                fileInfo.Delete();
            }
        }
    }
}
