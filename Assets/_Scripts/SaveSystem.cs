using System.IO;
using UnityEngine;

public class SaveSystem
{
    public static SaveData saveData = new SaveData();

    [System.Serializable]
    public struct SaveData
    {
        public PlayerSaveData playerData;
        public PlayerInventorySaveData playerInventoryData;
        public PlayerEquipmentSaveData playerEquipmentData;
        public PlayerWeaponSaveData playerWeaponData;
        public LevelSaveData LevelData;
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
        GridController.Instance.playerController.Save(ref saveData.playerData);
        GridController.Instance.playerController.playerInventoryManager.Save(ref saveData.playerInventoryData);
        GridController.Instance.playerController.playerEquipmentManager.Save(ref saveData.playerEquipmentData);
        GridController.Instance.playerController.playerWeaponManager.Save(ref saveData.playerWeaponData);
        GridController.Instance.Save(ref saveData.LevelData);
    }

    public static void Load()
    {
        string saveContent = File.ReadAllText(SaveFileName());

        saveData = JsonUtility.FromJson<SaveData>(saveContent);

        HandleLoadData();
    }

    static void HandleLoadData()
    {
        GridController.Instance.playerController.Load(saveData.playerData);
        GridController.Instance.playerController.playerInventoryManager.Load(saveData.playerInventoryData);
        GridController.Instance.playerController.playerEquipmentManager.Load(saveData.playerEquipmentData);
        GridController.Instance.playerController.playerWeaponManager.Load(saveData.playerWeaponData);
        GridController.Instance.Load(saveData.LevelData);
    }
}
