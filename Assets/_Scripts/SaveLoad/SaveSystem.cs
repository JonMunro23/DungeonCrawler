using System.IO;
using UnityEngine;

public class SaveSystem
{
    public static SaveData saveData = new SaveData();

    [System.Serializable]
    public struct SaveData
    {
        public LevelSaveData LevelData;
        public PlayerSaveData playerData;
    }

    public static string SaveFileName()
    {
        string saveFile = Application.persistentDataPath + "/save" + ".meme";
        return saveFile;
    }

    public static void Save()
    {
        HandeSaveData();
        File.WriteAllText(SaveFileName(), JsonUtility.ToJson(saveData, true));
    }

    static void HandeSaveData()
    {
        GridController.Instance.Save(ref saveData.LevelData);
        GridController.Instance.playerController.Save(ref saveData.playerData);
    }

    public static void Load()
    {
        string saveContent = File.ReadAllText(SaveFileName());

        saveData = JsonUtility.FromJson<SaveData>(saveContent);

        HandleLoadData();
    }

    static void HandleLoadData()
    {
        GridController.Instance.Load(saveData.LevelData);
        GridController.Instance.playerController.Load(saveData.playerData);
    }
}
