using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class SaveSlot : MonoBehaviour
{
    [SerializeField] int slotIndex;
    [SerializeField] string slotName;
    [SerializeField] SaveSystem.SaveData slotData;

    [SerializeField] TMP_Text saveNameText, areaNameText, gameTimeText;
    
    public void Init(int slotIndex, SaveSystem.SaveData slotData)
    {
        this.slotIndex = slotIndex;
        this.slotData = slotData;
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

    }

    public void Save()
    {
        slotData = SaveSystem.Save(slotIndex, slotName);
        UpdateSlotUI();
    }
}
