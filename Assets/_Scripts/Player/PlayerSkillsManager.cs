using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class UnlockedSKillData
{
    public PlayerSkillData skillData;
    public int skillLevel;

    public UnlockedSKillData(PlayerSkillData skillData, int skillLevel)
    {
        this.skillData = skillData;
        this.skillLevel = skillLevel;
    }
}

public class PlayerSkillsManager : MonoBehaviour
{
    [Header("Player Skill Spawning")]
    [SerializeField] List<PlayerSkillData> globalPlayerSkills = new List<PlayerSkillData>();
    [SerializeField] List<PlayerSkill> spawnedPlayerSkills = new List<PlayerSkill>();
    [SerializeField] List<PlayerSkill> unlockedPlayerSkills = new List<PlayerSkill>();
    [SerializeField] PlayerSkill playerSkillPrefab;


    public int startingSkillPoints;
    int availableSkillPoints;

    bool isSkillMenuOpen;


    public static Action<List<PlayerSkill>> onPlayerSkillsSpawned;
    public static Action<int> onSkillPointsUpdated;
    public static Action onSkillMenuOpened;
    public static Action onSkillMenuClosed;
    public static Action<PlayerSkill> onSkillUpdated;


    private void Start()
    {
        AddSkillPoints(startingSkillPoints);
    }

    private void OnEnable()
    {
        PlayerLevelManager.onPlayerLevelUp += OnPlayerLevelUp;
        PlayerSkill.onPlayerSkillClicked += OnPlayerSkillClicked;
    }

    private void OnDisable()
    {
        PlayerLevelManager.onPlayerLevelUp -= OnPlayerLevelUp;
        PlayerSkill.onPlayerSkillClicked -= OnPlayerSkillClicked;
    }

    void OnPlayerSkillClicked(PlayerSkill skillClicked)
    {
        if(availableSkillPoints > 0)
        {
            UnlockSkill(skillClicked);
        }
    }

    public void Init(CharacterData playerCharData)
    {
        SpawnPlayerSkills(playerCharData);
    }

    void SpawnPlayerSkills(CharacterData playerCharData)
    {
        foreach (PlayerSkillData globalSkillData in globalPlayerSkills)
        {
            PlayerSkill clone = Instantiate(playerSkillPrefab);
            clone.InitSkill(globalSkillData);
            spawnedPlayerSkills.Add(clone);
        }

        foreach (PlayerSkillData classSkillData in playerCharData.classSpecificSkills)
        {
            PlayerSkill clone = Instantiate(playerSkillPrefab);
            clone.InitSkill(classSkillData);
            spawnedPlayerSkills.Add(clone);
        }
        onPlayerSkillsSpawned?.Invoke(spawnedPlayerSkills);
    }

    void UnlockSkill(PlayerSkill skillClicked)
    {
        skillClicked.AddSkillLevel();
        RemoveSkillPoint();
        onSkillUpdated?.Invoke(skillClicked);

        if (!unlockedPlayerSkills.Contains(skillClicked))
            unlockedPlayerSkills.Add(skillClicked);
    }

    void LoadSkill(UnlockedSKillData dataToLoad)
    {
        foreach(PlayerSkill skill in spawnedPlayerSkills)
        {
            if(skill.skillData.skillName == dataToLoad.skillData.skillName)
            {
                for (int i = 0; i < dataToLoad.skillLevel; i++)
                {
                    UnlockSkill(skill);
                }
            }
        }
    }

    void ResetSkill(PlayerSkill skillToReset)
    {
        skillToReset.ResetSkill();
    }

    void OnPlayerLevelUp(int playerLevel)
    {
        AddSkillPoint();
    }

    public void ToggleSkillsMenu()
    {
        if (isSkillMenuOpen == true)
        {
            CloseSkillsMenu();
        }
        else if (isSkillMenuOpen == false)
        {
            OpenSkillsMenu();
        }
    }

    private void OpenSkillsMenu()
    {
        isSkillMenuOpen = true;
        HelperFunctions.SetCursorActive(true);
        onSkillMenuOpened?.Invoke();
    }

    public void CloseSkillsMenu()
    {
        isSkillMenuOpen = false;
        HelperFunctions.SetCursorActive(false);
        onSkillMenuClosed?.Invoke();
    }


    public void AddSkillPoint()
    {
        availableSkillPoints++;
        onSkillPointsUpdated?.Invoke(availableSkillPoints);
    }

    public void AddSkillPoints(int amountToAdd)
    {
        for (int i = 0; i < startingSkillPoints; i++)
            AddSkillPoint();
    }

    public void RemoveSkillPoint()
    {
        availableSkillPoints--;
        onSkillPointsUpdated?.Invoke(availableSkillPoints);
    }

    public void Save(ref PlayerSaveData data)
    {
        data.availableSkillPoints = availableSkillPoints;

        List<UnlockedSKillData> unlockedSKillData = new List<UnlockedSKillData>();
        foreach (PlayerSkill skill in unlockedPlayerSkills)
        {
            unlockedSKillData.Add(new UnlockedSKillData(skill.skillData, skill.currentSkillLevel));
        }

        data.unlockedSkills = unlockedSKillData;
    }


    public void Load(PlayerSaveData data)
    {
        ResetSkills();
        ResetSkillPoints();

        AddSkillPoints(data.availableSkillPoints);
        foreach (UnlockedSKillData unlockedSkillData in data.unlockedSkills)
        {
            LoadSkill(unlockedSkillData);
        }
    }

    private void ResetSkills()
    {
        foreach (PlayerSkill skill in unlockedPlayerSkills)
        {
            ResetSkill(skill);
        }
        unlockedPlayerSkills.Clear();
    }

    private void ResetSkillPoints()
    {
        availableSkillPoints = 0;
        onSkillPointsUpdated?.Invoke(availableSkillPoints);
    }
}
