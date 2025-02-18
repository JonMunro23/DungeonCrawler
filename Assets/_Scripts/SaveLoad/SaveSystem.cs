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

    public static SaveData Save(string saveName)
    {

        HandeSaveData(saveName);

        SaveData saveData = new SaveData();
        for (int i = 0; i < saveDatas.Count; i++)
        {
            if (saveDatas[i].saveName == saveName)
            {
                saveData = saveDatas[i];
            }
        }

        string path = SaveFileName(saveName);
        File.WriteAllText(path, JsonUtility.ToJson(saveData, true));
        
        FileInfo fileInfo = new FileInfo(path);
        saveFileInfo.Add(fileInfo);

        return saveData;
    }

    static void HandeSaveData(string saveName)
    {
        GridController.Instance.Save(ref saveData);
        GridController.Instance.playerController.Save(ref saveData.playerData);

        saveData.saveName = saveName;
        //saveData.saveIndex = slotIndex;
        saveData.saveDate = System.DateTime.Now.ToString();

        for (int i = 0; i < saveDatas.Count; i++)
        {
            if (saveDatas[i].saveName == saveName)
            {
                saveDatas[i] = saveData;
                return;
            }
        }

        saveDatas.Add(saveData);
    }

    public static void Load(string saveName)
    {
        HandleLoadData(saveName);
    }

    static void HandleLoadData(string saveName)
    {
        SaveData data = new SaveData();

        foreach (SaveData saveData in saveDatas)
        {
            if(saveData.saveName == saveName)
            {
                data = saveData;
                break;
            }
        }

        GridController.Instance.Load(data);
        GridController.Instance.playerController.Load(data.playerData);
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
