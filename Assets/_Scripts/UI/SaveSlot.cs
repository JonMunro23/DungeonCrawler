using System;
using TMPro;
using UnityEngine;

public class SaveSlot : MonoBehaviour
{
    [SerializeField] int slotIndex;
    [SerializeField] string slotName;
    public SaveSystem.SaveData slotData;

    [SerializeField] TMP_Text saveNameText, areaNameText, gameTimeText, saveDataText;

    public static Action onSaveLoaded;
    public static Action<SaveSlot> onSaveDeleteButtonPressed;
    public static Action onCreateNewSaveButtonPressed;

    public void Init(SaveSystem.SaveData slotData)
    {
        this.slotData = slotData;
        slotIndex = slotData.saveIndex;
        slotName = slotData.saveName;

        UpdateSlotUI();
    }

    void UpdateSlotUI()
    {

        int seconds = Mathf.FloorToInt(slotData.gameTime % 60);
        int minutes = Mathf.FloorToInt((slotData.gameTime / 60) % 60);
        int hours = Mathf.FloorToInt((slotData.gameTime / 3600) % 24);

        string timeText = string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, seconds);

        saveNameText.text = $"{slotData.saveName}";
        areaNameText.text = $"{slotData.LevelData.currentLevelName}";
        gameTimeText.text = $"Game Time:  {timeText}";
        saveDataText.text = slotData.saveDate;

    }

    public void Save()
    {
        slotData = SaveSystem.Save(slotIndex, slotName);
        UpdateSlotUI();
    }

    public void Load()
    {
        onSaveLoaded?.Invoke();
        SaveSystem.Load(slotIndex, slotName);
    }

    public void CreateNewSaveButtonClicked()
    {
        onCreateNewSaveButtonPressed?.Invoke();
    }
    public void TryDeleteSave()
    {
        onSaveDeleteButtonPressed?.Invoke(this);
    }
}
